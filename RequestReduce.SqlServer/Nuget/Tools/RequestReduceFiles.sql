CREATE TABLE [dbo].[RequestReduceFiles](
	[RequestReduceFileId] [uniqueidentifier] NOT NULL,
	[Key] [uniqueidentifier] NOT NULL,
	[FileName] [nvarchar](150) NOT NULL,
	[Content] [varbinary](max) NOT NULL,
	[OriginalName] [nvarchar](max) NULL,
	[IsExpired] [bit] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RequestReduceFileId] ASC
))
GO
