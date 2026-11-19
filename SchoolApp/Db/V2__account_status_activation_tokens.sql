BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Roles_RoleId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Password');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Users] ALTER COLUMN [Password] nvarchar(60) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'DeletedAt');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Users] ALTER COLUMN [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    ALTER TABLE [Users] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    ALTER TABLE [Users] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'PhoneNumber');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT ' + @var2 + ';');
    EXEC(N'UPDATE [Teachers] SET [PhoneNumber] = N'''' WHERE [PhoneNumber] IS NULL');
    ALTER TABLE [Teachers] ALTER COLUMN [PhoneNumber] nvarchar(20) NOT NULL;
    ALTER TABLE [Teachers] ADD DEFAULT N'' FOR [PhoneNumber];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Teachers]') AND [c].[name] = N'DeletedAt');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Teachers] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Teachers] ALTER COLUMN [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'DeletedAt');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Students] ALTER COLUMN [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'DeletedAt');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [Courses] ALTER COLUMN [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    CREATE TABLE [ActivationTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [TokenHash] nvarchar(64) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        CONSTRAINT [PK_ActivationTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivationTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
        SET IDENTITY_INSERT [Capabilities] ON;
    EXEC(N'INSERT INTO [Capabilities] ([Id], [Description], [Name])
    VALUES (19, N''Create another admin user'', N''INSERT_ADMIN'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
        SET IDENTITY_INSERT [Capabilities] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
        SET IDENTITY_INSERT [RolesCapabilities] ON;
    EXEC(N'INSERT INTO [RolesCapabilities] ([CapabilitiesId], [RolesId])
    VALUES (19, 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
        SET IDENTITY_INSERT [RolesCapabilities] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Teachers_PhoneNumber] ON [Teachers] ([PhoneNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    CREATE INDEX [IX_ActivationTokens_UserId] ON [ActivationTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_ActivationTokens_TokenHash] ON [ActivationTokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261031234545_AccountStatusActivationTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261031234545_AccountStatusActivationTokens', N'10.0.13');
END;

COMMIT;
GO

