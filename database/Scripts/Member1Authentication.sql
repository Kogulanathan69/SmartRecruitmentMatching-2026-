IF DB_ID('NexHireDb') IS NULL CREATE DATABASE NexHireDb;
GO
USE NexHireDb;
GO
IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users(Id uniqueidentifier NOT NULL PRIMARY KEY,FirstName nvarchar(60) NOT NULL,LastName nvarchar(60) NOT NULL,PhoneNumber nvarchar(30) NULL,Email nvarchar(256) NOT NULL,NormalizedEmail nvarchar(256) NOT NULL,PasswordHash nvarchar(100) NOT NULL,Role nvarchar(20) NOT NULL,Status nvarchar(20) NOT NULL,PasswordResetTokenHash nvarchar(64) NULL,PasswordResetExpiresAtUtc datetime2 NULL,CreatedAtUtc datetime2 NOT NULL,CONSTRAINT UQ_Users_NormalizedEmail UNIQUE(NormalizedEmail));
GO
IF OBJECT_ID('dbo.RefreshTokens','U') IS NULL
CREATE TABLE dbo.RefreshTokens(Id uniqueidentifier NOT NULL PRIMARY KEY,UserId uniqueidentifier NOT NULL,TokenHash nvarchar(64) NOT NULL,CreatedAtUtc datetime2 NOT NULL,ExpiresAtUtc datetime2 NOT NULL,RevokedAtUtc datetime2 NULL,CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE(TokenHash),CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE);
GO
