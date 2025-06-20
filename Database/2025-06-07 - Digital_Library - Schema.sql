USE [Digital_Library ]
GO
/****** Object:  UserDefinedTableType [dbo].[EmbeddingTableType]    Script Date: 07-06-25 10:43:42 ******/
CREATE TYPE [dbo].[EmbeddingTableType] AS TABLE(
	[vector_index] [int] NOT NULL,
	[vector_value] [float] NOT NULL
)
GO
/****** Object:  Table [dbo].[Books]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Books](
	[book_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[book_title] [nvarchar](255) NOT NULL,
	[book_author] [nvarchar](255) NULL,
	[book_year_published] [int] NULL,
	[book_date_added] [datetime] NULL,
	[book_pages_count] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[book_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Embeddings]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Embeddings](
	[embedding_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[embedding_page_id_fkey] [int] NOT NULL,
	[embedding_text_type] [nvarchar](50) NOT NULL,
	[embedding_text] [nvarchar](max) NOT NULL,
	[embedding_value] [varbinary](max) NOT NULL,
	[embedding_date] [datetime] NULL,
	[embedding_norm] [float] NULL,
PRIMARY KEY CLUSTERED 
(
	[embedding_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Embeddings_Values]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Embeddings_Values](
	[embval_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[embval_embedding_id_fkey] [int] NOT NULL,
	[embval_vector_index] [int] NOT NULL,
	[embval_vector_value] [float] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[embval_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Images]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Images](
	[image_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[image_page_id_fkey] [int] NOT NULL,
	[image_data] [varbinary](max) NOT NULL,
	[image_description] [nvarchar](255) NULL,
	[image_index] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[image_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Page_Species]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Page_Species](
	[species_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[species_page_id_fkey] [int] NOT NULL,
	[species_order] [varchar](max) NULL,
	[species_order_common] [varchar](max) NULL,
	[species_family] [varchar](max) NULL,
	[species_genus] [varchar](max) NULL,
	[species_genus_conf] [varchar](max) NULL,
	[species_genus_source] [varchar](max) NULL,
	[species_species] [varchar](max) NULL,
	[species_species_conf] [varchar](max) NULL,
	[species_species_type] [varchar](max) NULL,
 CONSTRAINT [PK__Page_Spe__4ECBF36DF8232B11] PRIMARY KEY CLUSTERED 
(
	[species_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pages]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pages](
	[page_id_pkey] [int] IDENTITY(1,1) NOT NULL,
	[page_book_id_fkey] [int] NOT NULL,
	[page_number] [int] NOT NULL,
	[page_raw_text] [nvarchar](max) NULL,
	[page_ocr_text] [nvarchar](max) NULL,
	[page_raw_text_cleaned] [nvarchar](max) NULL,
	[page_raw_text_llm] [nvarchar](max) NULL,
	[page_raw_text_llm_explain] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[page_id_pkey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Books] ADD  DEFAULT (getdate()) FOR [book_date_added]
GO
ALTER TABLE [dbo].[Embeddings] ADD  DEFAULT (getdate()) FOR [embedding_date]
GO
ALTER TABLE [dbo].[Embeddings]  WITH CHECK ADD  CONSTRAINT [FK_Embeddings_Pages] FOREIGN KEY([embedding_page_id_fkey])
REFERENCES [dbo].[Pages] ([page_id_pkey])
GO
ALTER TABLE [dbo].[Embeddings] CHECK CONSTRAINT [FK_Embeddings_Pages]
GO
ALTER TABLE [dbo].[Embeddings_Values]  WITH CHECK ADD FOREIGN KEY([embval_embedding_id_fkey])
REFERENCES [dbo].[Embeddings] ([embedding_id_pkey])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Images]  WITH CHECK ADD FOREIGN KEY([image_page_id_fkey])
REFERENCES [dbo].[Pages] ([page_id_pkey])
GO
ALTER TABLE [dbo].[Page_Species]  WITH CHECK ADD  CONSTRAINT [fk_page_species] FOREIGN KEY([species_page_id_fkey])
REFERENCES [dbo].[Pages] ([page_id_pkey])
GO
ALTER TABLE [dbo].[Page_Species] CHECK CONSTRAINT [fk_page_species]
GO
ALTER TABLE [dbo].[Pages]  WITH CHECK ADD  CONSTRAINT [FK_Pages_Books] FOREIGN KEY([page_book_id_fkey])
REFERENCES [dbo].[Books] ([book_id_pkey])
GO
ALTER TABLE [dbo].[Pages] CHECK CONSTRAINT [FK_Pages_Books]
GO
/****** Object:  StoredProcedure [dbo].[SearchSimilarPages]    Script Date: 07-06-25 10:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SearchSimilarPages] 
    @query_embedding EmbeddingTableType READONLY,
    @top_n INT = 5,
    @book_title NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @query_norm FLOAT;
    SELECT @query_norm = SQRT(SUM(q.vector_value * q.vector_value))
    FROM @query_embedding q;

    SELECT TOP (@top_n) 
        e.embedding_page_id_fkey AS page_id,
        b.book_title,
        e.embedding_text_type AS text_type,
        e.embedding_text AS full_text,
        p.page_raw_text_cleaned,
        p.page_raw_text_llm,
        SUM(ev.embval_vector_value * q.vector_value) / (e.embedding_norm * @query_norm) AS similarity_score,
		p.[page_number] As page_number
    FROM Embeddings e
    JOIN Pages p ON e.embedding_page_id_fkey = p.page_id_pkey
    JOIN Books b ON p.page_book_id_fkey = b.book_id_pkey
    JOIN Embeddings_Values ev WITH(INDEX(idx_embedding_id)) ON e.embedding_id_pkey = ev.embval_embedding_id_fkey
    JOIN @query_embedding q ON ev.embval_vector_index = q.vector_index
    WHERE @book_title IS NULL OR b.book_title = @book_title
    GROUP BY 
        e.embedding_page_id_fkey,
        b.book_title,
        e.embedding_text_type,
        e.embedding_text,
        p.page_raw_text_cleaned,
        p.page_raw_text_llm,
        e.embedding_norm,
		p.[page_number]
    ORDER BY similarity_score DESC;
END;
GO
