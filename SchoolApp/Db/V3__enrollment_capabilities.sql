BEGIN TRANSACTION;
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
    SET IDENTITY_INSERT [Capabilities] ON;
INSERT INTO [Capabilities] ([Id], [Description], [Name])
VALUES (20, N'Enroll a student in a course', N'ENROLL_STUDENT'),
(21, N'Remove a student from a course', N'UNENROLL_STUDENT');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Name') AND [object_id] = OBJECT_ID(N'[Capabilities]'))
    SET IDENTITY_INSERT [Capabilities] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
    SET IDENTITY_INSERT [RolesCapabilities] ON;
INSERT INTO [RolesCapabilities] ([CapabilitiesId], [RolesId])
VALUES (20, 1),
(20, 2),
(21, 1),
(21, 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CapabilitiesId', N'RolesId') AND [object_id] = OBJECT_ID(N'[RolesCapabilities]'))
    SET IDENTITY_INSERT [RolesCapabilities] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261116233627_AddEnrollmentCapabilities', N'10.0.13');

COMMIT;
GO

