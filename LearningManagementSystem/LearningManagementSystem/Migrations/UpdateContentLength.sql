-- Script to increase Content column length in Posts and Topics tables
-- Run this script in SQL Server Management Studio or your database tool

USE LMS;
GO

-- Update Posts table
ALTER TABLE [dbo].[Posts]
ALTER COLUMN [Content] nvarchar(10000) NOT NULL;
GO

-- Update Topics table
ALTER TABLE [dbo].[Topics]
ALTER COLUMN [Content] nvarchar(10000) NOT NULL;
GO

PRINT 'Content columns updated successfully!';
GO
