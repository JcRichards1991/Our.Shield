DELETE FROM [umbracoKeyValue]
WHERE [key] like '%+Shield'
AND [Key] Not Like '%MediaProtection%'
GO

DROP TABLE [ShieldApps]
GO

DROP TABLE [ShieldEnvironments]
GO