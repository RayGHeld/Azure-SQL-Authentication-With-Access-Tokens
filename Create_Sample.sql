/****** Object:  Table [dbo].[users]    Script Date: 6/1/2021 2:06:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[users](
	[id] [uniqueidentifier] NOT NULL,
	[user_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_id]  DEFAULT (newid()) FOR [id]
GO


USE [your_database]
GO

INSERT INTO [dbo].[users]
           ([user_name])
     VALUES
           ('user1@somedomain.com')
		  ,('user2@somedomain.com')
		  ,('user3@somedomain.com')
GO

