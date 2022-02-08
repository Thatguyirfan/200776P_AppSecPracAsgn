--Table to store verification codes
DROP TABLE IF EXISTS VerificationTable;
CREATE TABLE [dbo].[VerificationTable] (
    [Id]          UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Email]       NVARCHAR (100)   NOT NULL,
    [Code]        NVARCHAR (15)    NOT NULL,
    [DateCreated] DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC),
    FOREIGN KEY ([Email]) REFERENCES [dbo].[Account] ([Email])
);

--Table to store account details
DROP TABLE IF EXISTS Account;
CREATE TABLE [dbo].[Account] (
    [Id]            UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FName]         NVARCHAR (50)    NOT NULL,
    [LName]         NVARCHAR (50)    NOT NULL,
    [Email]         NVARCHAR (100)   NOT NULL,
    [PasswordHash]  NVARCHAR (MAX)   NOT NULL,
    [DOB]           SMALLDATETIME    NOT NULL,
    [Photo]         VARBINARY (MAX)  NOT NULL,
    [CCInfo]        NVARCHAR (MAX)   NOT NULL,
    [PasswordSalt]  NVARCHAR (MAX)   NOT NULL,
    [IV]            NVARCHAR (MAX)   NOT NULL,
    [Key]           NVARCHAR (MAX)   NOT NULL,
    [EmailVerified] INT              DEFAULT ((0)) NOT NULL,
    [AttemptsLeft]  INT              DEFAULT ((3)) NOT NULL,
    [LastPwdUpdate] DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

--Table to store audit trail for Account table
DROP TABLE IF EXISTS AccountAudit;
CREATE TABLE [dbo].[AccountAudit] (
    [Id]            UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [AccountId]     UNIQUEIDENTIFIER NOT NULL,
    [FName]         NVARCHAR (50)    NOT NULL,
    [LName]         NVARCHAR (50)    NOT NULL,
    [Email]         NVARCHAR (100)   NOT NULL,
    [PasswordHash]  NVARCHAR (MAX)   NOT NULL,
    [DOB]           SMALLDATETIME    NOT NULL,
    [Photo]         VARBINARY (MAX)  NOT NULL,
    [CCInfo]        NVARCHAR (MAX)   NOT NULL,
    [PasswordSalt]  NVARCHAR (MAX)   NOT NULL,
    [IV]            NVARCHAR (MAX)   NOT NULL,
    [Key]           NVARCHAR (MAX)   NOT NULL,
    [EmailVerified] INT              NOT NULL,
    [AttemptsLeft]  INT              NOT NULL,
    [UpdatedBy]     NVARCHAR (128)   NOT NULL,
    [UpdatedOn]     DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Trigger to store previous values in audit table
--CREATE TRIGGER AccountAuditRecord ON [dbo].[Account]
--	FOR DELETE, INSERT, UPDATE
--	AS
--	BEGIN
--		INSERT INTO [dbo].[AccountAudit]
--		(AccountId, FName, LName, Email, PasswordHash, DOB, Photo, CCInfo, 
--		PasswordSalt, IV, [Key], EmailVerified, AttemptsLeft, UpdatedBy, UpdatedOn)
--		SELECT i.Id, i.FName, i.LName, i.Email, i.PasswordHash, i.DOB, i.Photo, i.CCInfo, 
--		i.PasswordSalt, i.IV, i.[Key], i.EmailVerified, i.AttemptsLeft, SUSER_SNAME(), getdate()
--		FROM [dbo].[Account] a
--		INNER JOIN inserted i ON a.Id = i.Id
--	END