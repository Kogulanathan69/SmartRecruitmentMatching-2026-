using FluentAssertions;
using Moq;
using Xunit;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.UnitTests.Auth;

public sealed class AuthOtpTests
{
    private static (AuthService Sut, Mock<IUserRepository> Users, Mock<IPasswordHasher> Passwords, Mock<ITokenService> Tokens, Mock<IEmailSender> Email) Create()
    {
        var users = new Mock<IUserRepository>();
        var passwords = new Mock<IPasswordHasher>();
        var tokens = new Mock<ITokenService>();
        var email = new Mock<IEmailSender>();
        tokens.Setup(x => x.HashToken(It.IsAny<string>())).Returns((string value) => "hash:" + value);
        return (new AuthService(users.Object, passwords.Object, tokens.Object, email.Object), users, passwords, tokens, email);
    }

    [Fact]
    public async Task Register_creates_unverified_user_and_sends_six_digit_otp()
    {
        var (sut, users, passwords, _, email) = Create();
        User? created = null;
        users.Setup(x => x.EmailExistsAsync("NEW@EXAMPLE.COM", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        users.Setup(x => x.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => created = u).Returns(Task.CompletedTask);
        users.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        passwords.Setup(x => x.Hash("Strong@123")).Returns("password-hash");
        email.Setup(x => x.SendEmailAsync("new@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await sut.RegisterAsync(new RegisterRequestDto("new@example.com", "Strong@123", "Strong@123", "New", "User", null, "JobSeeker"));

        created.Should().NotBeNull();
        created!.IsEmailVerified.Should().BeFalse();
        created.EmailVerificationOtpExpiresAtUtc.Should().BeAfter(DateTime.UtcNow.AddMinutes(9));
        created.EmailVerificationOtpFailedAttempts.Should().Be(0);
        var body = (string)email.Invocations.Single().Arguments[2];
        var otp = System.Text.RegularExpressions.Regex.Match(body, @"\b\d{6}\b").Value;
        otp.Should().MatchRegex(@"^\d{6}$");
        created.EmailVerificationOtpHash.Should().Be("hash:" + otp);
    }

    [Fact]
    public async Task Verify_correct_otp_marks_email_verified_and_clears_otp_state()
    {
        var (sut, users, _, _, _) = Create();
        var user = new User { Email = "user@example.com", NormalizedEmail = "USER@EXAMPLE.COM", IsEmailVerified = false, EmailVerificationOtpHash = "hash:123456", EmailVerificationOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5), EmailVerificationOtpLastSentAtUtc = DateTime.UtcNow.AddMinutes(-1), EmailVerificationOtpFailedAttempts = 2 };
        users.Setup(x => x.GetByEmailAsync("USER@EXAMPLE.COM", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        users.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.VerifyEmailOtpAsync(new VerifyEmailOtpRequestDto("user@example.com", "123456"));

        ok.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerificationOtpHash.Should().BeNull();
        user.EmailVerificationOtpExpiresAtUtc.Should().BeNull();
        user.EmailVerificationOtpLastSentAtUtc.Should().BeNull();
        user.EmailVerificationOtpFailedAttempts.Should().Be(0);
    }

    [Fact]
    public async Task Verify_wrong_otp_increments_failed_attempts()
    {
        var (sut, users, _, _, _) = Create();
        var user = new User { Email = "user@example.com", NormalizedEmail = "USER@EXAMPLE.COM", IsEmailVerified = false, EmailVerificationOtpHash = "hash:123456", EmailVerificationOtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5), EmailVerificationOtpFailedAttempts = 0 };
        users.Setup(x => x.GetByEmailAsync("USER@EXAMPLE.COM", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        users.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.VerifyEmailOtpAsync(new VerifyEmailOtpRequestDto("user@example.com", "999999"));

        ok.Should().BeFalse();
        user.EmailVerificationOtpFailedAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Resend_inside_sixty_seconds_is_blocked_without_email_send()
    {
        var (sut, users, _, _, email) = Create();
        var user = new User { Email = "user@example.com", NormalizedEmail = "USER@EXAMPLE.COM", IsEmailVerified = false, EmailVerificationOtpLastSentAtUtc = DateTime.UtcNow.AddSeconds(-10) };
        users.Setup(x => x.GetByEmailAsync("USER@EXAMPLE.COM", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var ok = await sut.ResendEmailOtpAsync(new ResendEmailOtpRequestDto("user@example.com"));

        ok.Should().BeFalse();
        email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_blocks_valid_password_until_email_is_verified()
    {
        var (sut, users, passwords, _, _) = Create();
        var user = new User { Email = "user@example.com", NormalizedEmail = "USER@EXAMPLE.COM", PasswordHash = "hash", Status = UserStatus.Active, IsEmailVerified = false };
        users.Setup(x => x.GetByEmailAsync("USER@EXAMPLE.COM", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        passwords.Setup(x => x.Verify("Strong@123", "hash")).Returns(true);

        var act = () => sut.LoginAsync(new LoginRequestDto("user@example.com", "Strong@123"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not verified*");
    }
}