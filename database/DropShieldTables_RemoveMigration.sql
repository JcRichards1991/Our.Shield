DELETE FROM [umbracoMigration]
WHERE name like 'Shield%'
GO

DROP TABLE [ShieldJournal]
GO

DROP TABLE [ShieldConfiguration]
GO

DROP TABLE [ShieldDomain]
GO

DROP TABLE [ShieldEnvironment]
GO
