CREATE TABLE [dbo].[RequestReduceFiles](
	[RequestReduceFileId] [uniqueidentifier] NOT NULL,
	[Key] [uniqueidentifier] NOT NULL,
	[FileName] [nvarchar](50) NOT NULL,
	[Content] [varbinary](max) NOT NULL,
	[OriginalName] [nvarchar](max) NULL,
	[IsExpired] [bit] NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RequestReduceFileId] ASC
))
GO
