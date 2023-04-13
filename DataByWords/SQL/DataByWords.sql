EXEC dbo.sp_configure 'clr enabled',1 RECONFIGURE
GO
 
ALTER DATABASE Temp_WordMergerDB SET TRUSTWORTHY ON;
GO

CREATE ASSEMBLY DataByWords
FROM '\\vm-dev-sql\c$\DataByWords.dll'
WITH PERMISSION_SET = SAFE
GO

CREATE FUNCTION dbo.GetDollarString (@value decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDollarString
GO

CREATE FUNCTION dbo.GetEuroString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetEuroString
GO

CREATE FUNCTION dbo.GetRubleString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetRubleString
GO

CREATE FUNCTION dbo.GetRubleStringWithKopeika (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetRubleStringWithKopeika
GO

CREATE FUNCTION dbo.GetRubleStringWithKopeikaAndBrackets (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetRubleStringWithKopeikaAndBrackets
GO

CREATE FUNCTION dbo.GetRRubleString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetRRubleString
GO

CREATE FUNCTION dbo.GetNumericString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetNumericString
GO

CREATE FUNCTION dbo.GetNumericStringFS (@value  decimal(15,2), @padej int, @fs int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetNumericStringFS
GO

CREATE FUNCTION dbo.GetNumericStringWithPrefixNumber (@value  decimal(15,2), @padej int, @fs int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetNumericStringWithPrefixNumber
GO


CREATE FUNCTION dbo.GetDayString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDayString
GO

CREATE FUNCTION dbo.GetPercentString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPercentString
GO

CREATE FUNCTION dbo.GetPercentStringWithoutAnd (@value  NVARCHAR(255), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPercentStringWithoutAnd
GO

CREATE FUNCTION dbo.GetPercentStringDec (@value  decimal(15,2), @padej int, @decimals int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPercentStringDec
GO

CREATE FUNCTION dbo.GetPercentStringWithDecimals (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPercentStringWithDecimals
GO

CREATE FUNCTION dbo.GetMonthString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetMonthString
GO

CREATE FUNCTION dbo.GetCalendarMonthString (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetCalendarMonthString
GO

CREATE FUNCTION dbo.GetNumberString (@value  decimal(15,2), @fs int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetNumberString
GO

CREATE FUNCTION dbo.GetNumberStringWithPadeg (@value  decimal(15,2), @padeg int, @fs int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetNumberStringWithPadeg
GO

CREATE FUNCTION dbo.DateDiffYear (@date1 NVARCHAR(255), @date2 NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].DateDiffYear
GO

CREATE FUNCTION dbo.DateDiffMonth (@date1 NVARCHAR(255), @date2 NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].DateDiffMonth
GO

CREATE FUNCTION dbo.DateDiffDay (@date1 NVARCHAR(255), @date2 NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].DateDiffDay
GO

CREATE FUNCTION dbo.DateDiffAsString (@date1 NVARCHAR(255), @date2 NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].DateDiffAsString
GO

CREATE FUNCTION dbo.CurrencyConverter (@dec decimal(15,2), @from int, @to int, @dollarRate decimal(15,2), @eurorate decimal(15,2))
RETURNS decimal(15,2) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].CurrencyConverter
GO

CREATE FUNCTION dbo.GetValuta (@dec decimal(15,2), @padeg int, @valuta int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetValuta
GO

CREATE FUNCTION dbo.GetDateString (@datestr NVARCHAR(255), @padeg int, @valuta int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDateString
GO

CREATE FUNCTION dbo.GetDateStringWithCommas (@datestr NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDateStringWithCommas
GO

CREATE FUNCTION dbo.GetDay (@datestr NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDay
GO

CREATE FUNCTION dbo.GetMonth (@datestr NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetMonth
GO

CREATE FUNCTION dbo.GetYear (@datestr NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetYear
GO

CREATE FUNCTION dbo.ToCrmDateTime (@datestr NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].ToCrmDateTime
GO

CREATE FUNCTION dbo.FormatCrmDateTime (@crmDateTime NVARCHAR(255), @format NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].FormatCrmDateTime
GO

CREATE FUNCTION dbo.FormatDate (@datestr NVARCHAR(255), @format NVARCHAR(255))
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].FormatDate
GO

CREATE FUNCTION dbo.DeclinationOfNames (@lastname NVARCHAR(255), @firstname NVARCHAR(255), @middlename NVARCHAR(255), @sex int, @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].DeclinationOfNames
GO

CREATE FUNCTION dbo.GetDollarStringWithCent (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDollarStringWithCent
GO

CREATE FUNCTION dbo.GetDollarStringWithCentAndBrackets (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetDollarStringWithCentAndBrackets
GO

CREATE FUNCTION dbo.GetEuroStringWithEurocent (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetEuroStringWithEurocent
GO

CREATE FUNCTION dbo.GetEuroStringWithEurocentAndBrackets (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetEuroStringWithEurocentAndBrackets
GO

CREATE FUNCTION dbo.GetPoundsterlingStringWithPerry (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPoundsterlingStringWithPerry
GO

CREATE FUNCTION dbo.GetPoundsterlingWithPerryAndBrackets (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetPoundsterlingWithPerryAndBrackets
GO

CREATE FUNCTION dbo.GetYuanStringWithFyn (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetYuanStringWithFyn
GO

CREATE FUNCTION dbo.GetYuanStringWithFynAndBrackets (@value  decimal(15,2), @padej int)
RETURNS NVARCHAR(255) --WITH EXECUTE AS CALLER
AS EXTERNAL NAME [DataByWords].[DataByWords.DataByWordsSql].GetYuanStringWithFynAndBrackets
GO