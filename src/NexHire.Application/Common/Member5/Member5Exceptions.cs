namespace NexHire.Application.Common;

public abstract class Member5Exception : Exception
{
    protected Member5Exception(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public sealed class Member5ValidationException : Member5Exception
{
    public Member5ValidationException(string code, string message) : base(code, message) { }
}

public sealed class Member5ForbiddenException : Member5Exception
{
    public Member5ForbiddenException(string code, string message) : base(code, message) { }
}

public sealed class Member5NotFoundException : Member5Exception
{
    public Member5NotFoundException(string code, string message) : base(code, message) { }
}

public sealed class Member5ConflictException : Member5Exception
{
    public Member5ConflictException(string code, string message) : base(code, message) { }
}

public sealed class Member5DependencyException : Member5Exception
{
    public Member5DependencyException(string code, string message) : base(code, message) { }
}
