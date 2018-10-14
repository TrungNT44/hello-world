USE [aspnet-SeedSimple-20130125152905]
GO
/****** Object:  UserDefinedFunction [dbo].[UDF_FIRST_LETTER_FROM_WORD]    Script Date: 05/03/2015 10:06:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Function [dbo].[UDF_FIRST_LETTER_FROM_WORD]
(
   @String Varchar(Max) -- Variable for string
)
RETURNS Varchar(Max)
BEGIN
Declare @Xml Xml
Declare @firstletter Varchar(Max)
Declare @delimiter Varchar(5)
 
SET @delimiter=' '
SET @Xml = cast(('<a>'+replace(@String,@delimiter,'</a><a>')+'</a>') AS XML)
 
;With CTE AS (SELECT A.value('.', 'varchar(max)') as [Column]
FROM @Xml.nodes('a') AS FN(a) )
SELECT @firstletter =Stuff((SELECT '' + LEFT([Column],1)
FROM CTE
FOR XML PATH('') ),1,0,'')
 
RETURN (@firstletter)
END
