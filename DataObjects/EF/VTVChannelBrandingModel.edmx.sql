
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/19/2016 16:06:24
-- Generated from EDMX file: D:\_PROJECT\ChiDuc\WPF\VTVChannelBranding\DataObjects\EF\VTVChannelBrandingModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [db_VTVChannelBranding];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_TrafficEvents_Channels]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TrafficEvents] DROP CONSTRAINT [FK_TrafficEvents_Channels];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Channels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Channels];
GO
IF OBJECT_ID(N'[dbo].[TrafficEvents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TrafficEvents];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'ChannelEntities'
CREATE TABLE [dbo].[ChannelEntities] (
    [Name] varchar(50)  NOT NULL,
    [Description] nvarchar(256)  NULL,
    [LastTrafficUpdateFilePath] nvarchar(256)  NULL,
    [LastTrafficUpdateFileTime] datetime  NOT NULL
);
GO

-- Creating table 'TrafficEventEntities'
CREATE TABLE [dbo].[TrafficEventEntities] (
    [ProgramCode] nvarchar(256)  NOT NULL,
    [ChannelName] varchar(50)  NULL,
    [ProgramTitle1] nvarchar(256)  NULL,
    [ProgramTitle2] nvarchar(256)  NULL,
    [CreateTime] datetime  NOT NULL,
    [UpdateTime] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Name] in table 'ChannelEntities'
ALTER TABLE [dbo].[ChannelEntities]
ADD CONSTRAINT [PK_ChannelEntities]
    PRIMARY KEY CLUSTERED ([Name] ASC);
GO

-- Creating primary key on [ProgramCode] in table 'TrafficEventEntities'
ALTER TABLE [dbo].[TrafficEventEntities]
ADD CONSTRAINT [PK_TrafficEventEntities]
    PRIMARY KEY CLUSTERED ([ProgramCode] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ChannelName] in table 'TrafficEventEntities'
ALTER TABLE [dbo].[TrafficEventEntities]
ADD CONSTRAINT [FK_TrafficEvents_Channels]
    FOREIGN KEY ([ChannelName])
    REFERENCES [dbo].[ChannelEntities]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TrafficEvents_Channels'
CREATE INDEX [IX_FK_TrafficEvents_Channels]
ON [dbo].[TrafficEventEntities]
    ([ChannelName]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------