IF OBJECT_ID('[dbo].[Devices]', 'U') IS NOT NULL
	DROP TABLE [dbo].[Devices];

CREATE TABLE [dbo].[devices]
(
    [DeviceId] NVARCHAR(128) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL,
    [Location] NVARCHAR(128) NOT NULL,
    [Type] NVARCHAR(128) NOT NULL,
    [AssetId] NVARCHAR(128) NOT NULL
)