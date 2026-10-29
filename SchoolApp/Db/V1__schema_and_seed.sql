IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Capabilities] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(255) NULL,
        CONSTRAINT [PK_Capabilities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [RolesCapabilities] (
        [CapabilitiesId] int NOT NULL,
        [RolesId] int NOT NULL,
        CONSTRAINT [PK_RolesCapabilities] PRIMARY KEY ([CapabilitiesId], [RolesId]),
        CONSTRAINT [FK_RolesCapabilities_Capabilities_CapabilitiesId] FOREIGN KEY ([CapabilitiesId]) REFERENCES [Capabilities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolesCapabilities_Roles_RolesId] FOREIGN KEY ([RolesId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [Email] nvarchar(50) NOT NULL,
        [Password] nvarchar(60) NOT NULL,
        [Firstname] nvarchar(50) NOT NULL,
        [Lastname] nvarchar(50) NOT NULL,
        [RoleId] int NOT NULL,
        [InsertedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] int NOT NULL IDENTITY,
        [AM] nvarchar(10) NOT NULL,
        [Institution] nvarchar(50) NOT NULL,
        [Department] nvarchar(50) NULL,
        [UserId] int NOT NULL,
        [InsertedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Students_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Teachers] (
        [Id] int NOT NULL IDENTITY,
        [Institution] nvarchar(50) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [UserId] int NOT NULL,
        [InsertedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Teachers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(50) NOT NULL,
        [TeacherId] int NULL,
        [InsertedAt] datetime2 NOT NULL,
        [ModifiedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Teachers] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE TABLE [StudentsCourses] (
        [CoursesId] int NOT NULL,
        [StudentsId] int NOT NULL,
        CONSTRAINT [PK_StudentsCourses] PRIMARY KEY ([CoursesId], [StudentsId]),
        CONSTRAINT [FK_StudentsCourses_Courses_CoursesId] FOREIGN KEY ([CoursesId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentsCourses_Students_StudentsId] FOREIGN KEY ([StudentsId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Capabilities_Name] ON [Capabilities] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_Description] ON [Courses] ([Description]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_TeacherId] ON [Courses] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Roles_Name] ON [Roles] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_RolesCapabilities_CapabilityId] ON [RolesCapabilities] ([CapabilitiesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_RolesCapabilities_RolesId] ON [RolesCapabilities] ([RolesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_Students_Institution] ON [Students] ([Institution]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Students_AM] ON [Students] ([AM]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Students_UserId] ON [Students] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_StudentsCourses_StudentsId] ON [StudentsCourses] ([StudentsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_Teachers_Institution] ON [Teachers] ([Institution]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Teachers_UserId] ON [Teachers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022161444_initialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022161444_initialCreate', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    ALTER TABLE [Courses] DROP CONSTRAINT [FK_Courses_Teachers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    ALTER TABLE [Students] DROP CONSTRAINT [FK_Students_Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    EXEC sp_rename N'[Students].[AM]', N'Am', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    EXEC sp_rename N'[Students].[UQ_Students_AM]', N'UQ_Students_Am', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    ALTER TABLE [Students] ADD CONSTRAINT [FK_Students_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023000914_AlignConstraintNames'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023000914_AlignConstraintNames', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028211512_SeedRoles'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [Name])
    VALUES (1, N''ADMIN''),
    (2, N''EMPLOYEE''),
    (3, N''TEACHER''),
    (4, N''STUDENT'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028211512_SeedRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028211512_SeedRoles', N'10.0.13');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029003758_SeedCapabilities'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
        SET IDENTITY_INSERT [Capabilities] ON;
    EXEC(N'INSERT INTO [Capabilities] ([Id], [Description], [Name])
    VALUES (1, N''Create a new teacher'', N''INSERT_TEACHER''),
    (2, N''View teacher list and details'', N''VIEW_TEACHERS''),
    (3, N''View teacher'', N''VIEW_TEACHER''),
    (4, N''Modify existing teacher'', N''EDIT_TEACHER''),
    (5, N''Remove a teacher'', N''DELETE_TEACHER''),
    (6, N''View only own teacher details'', N''VIEW_ONLY_TEACHER''),
    (7, N''Create a new student'', N''INSERT_STUDENT''),
    (8, N''View student list and details'', N''VIEW_STUDENTS''),
    (9, N''View student'', N''VIEW_STUDENT''),
    (10, N''Modify existing student'', N''EDIT_STUDENT''),
    (11, N''Remove a student'', N''DELETE_STUDENT''),
    (12, N''View only own student details'', N''VIEW_ONLY_STUDENT''),
    (13, N''Create a new course'', N''INSERT_COURSE''),
    (14, N''View course list and details'', N''VIEW_COURSES''),
    (15, N''View course'', N''VIEW_COURSE''),
    (16, N''Modify existing course'', N''EDIT_COURSE''),
    (17, N''Remove a course'', N''DELETE_COURSE''),
    (18, N''View the list of all users (registry)'', N''VIEW_USERS'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
        SET IDENTITY_INSERT [Capabilities] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029003758_SeedCapabilities'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
        SET IDENTITY_INSERT [RolesCapabilities] ON;
    EXEC(N'INSERT INTO [RolesCapabilities] ([CapabilitiesId], [RolesId])
    VALUES (1, 1),
    (1, 2),
    (2, 1),
    (2, 2),
    (3, 1),
    (3, 2),
    (4, 1),
    (5, 1),
    (6, 1),
    (6, 3),
    (7, 1),
    (7, 2),
    (8, 1),
    (8, 2),
    (9, 1),
    (9, 2),
    (10, 1),
    (11, 1),
    (12, 1),
    (12, 4),
    (13, 1),
    (14, 1),
    (14, 2),
    (15, 1),
    (15, 2),
    (16, 1),
    (17, 1),
    (18, 1),
    (18, 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
        SET IDENTITY_INSERT [RolesCapabilities] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261029003758_SeedCapabilities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261029003758_SeedCapabilities', N'10.0.13');
END;

COMMIT;
GO

