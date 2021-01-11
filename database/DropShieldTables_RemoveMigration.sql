DELETE FROM [umbracoKeyValue]
WHERE [key] like '%+Shield'
--AND name != 'ShieldMediaProtection'
GO

DROP TABLE [ShieldJournals]
GO

DROP TABLE [ShieldApps]
GO

DROP TABLE [ShieldEnvironments]
GO