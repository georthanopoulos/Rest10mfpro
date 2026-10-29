-- V0: create database and users (run once, as sa). Replace the <...> placeholders; never commit real passwords.
CREATE DATABASE Rest10mfpro COLLATE Greek_100_CI_AI;
GO

-- Server-level logins
CREATE LOGIN george WITH PASSWORD = '<GEORGE_PASSWORD>', DEFAULT_DATABASE = Rest10mfpro;          -- schema owner (runs migration scripts)
CREATE LOGIN rest10mfpro_app WITH PASSWORD = '<APP_PASSWORD>', DEFAULT_DATABASE = Rest10mfpro;    -- used by the application at runtime
GO

USE Rest10mfpro;
GO

-- Schema owner: may create/alter tables
CREATE USER george FOR LOGIN george;
ALTER ROLE db_owner ADD MEMBER george;

-- Application user: data only (least privilege)
CREATE USER rest10mfpro_app FOR LOGIN rest10mfpro_app;
ALTER ROLE db_datareader ADD MEMBER rest10mfpro_app;
ALTER ROLE db_datawriter ADD MEMBER rest10mfpro_app;
GO