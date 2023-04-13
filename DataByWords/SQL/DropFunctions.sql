USE Temp_WordMergerDB
GO

IF OBJECT_ID (N'dbo.GetDollarString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDollarString 
	PRINT 'dbo.GetDollarString успешно удалена'
END
ELSE
	PRINT 'dbo.GetDollarString уже удалена'

IF OBJECT_ID (N'dbo.GetEuroString', N'FS') IS NOT NULL
BEGIN 
	DROP FUNCTION dbo.GetEuroString 
	PRINT 'dbo.GetEuroString успешно удалена'
END
ELSE
	PRINT 'dbo.GetEuroString уже удалена'


IF OBJECT_ID (N'dbo.GetRubleString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetRubleString 
	PRINT 'dbo.GetRubleString успешно удалена'
END
ELSE
	PRINT 'dbo.GetRubleString уже удалена'

IF OBJECT_ID (N'dbo.GetRubleStringWithKopeika', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetRubleStringWithKopeika 
		PRINT 'dbo.GetRubleStringWithKopeika успешно удалена'
END
ELSE
	PRINT 'dbo.GetRubleStringWithKopeika уже удалена'


IF OBJECT_ID (N'dbo.GetRubleStringWithKopeikaAndBrackets', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetRubleStringWithKopeikaAndBrackets 
	PRINT 'dbo.GetRubleStringWithKopeikaAndBrackets успешно удалена'
END
ELSE
	PRINT 'dbo.GetRubleStringWithKopeikaAndBrackets уже удалена'


IF OBJECT_ID (N'dbo.GetRRubleString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetRRubleString 
	PRINT 'dbo.GetRRubleString успешно удалена'
END
ELSE
	PRINT 'dbo.GetRRubleString уже удалена'


IF OBJECT_ID (N'dbo.GetNumericString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetNumericString 
	PRINT 'dbo.GetNumericString успешно удалена'
END
ELSE
	PRINT 'dbo.GetNumericString уже удалена'


IF OBJECT_ID (N'dbo.GetNumericStringFS', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetNumericStringFS 
	PRINT 'dbo.GetNumericStringFS успешно удалена'
END
ELSE
	PRINT 'dbo.GetNumericStringFS уже удалена'


IF OBJECT_ID (N'dbo.GetNumericStringWithPrefixNumber', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetNumericStringWithPrefixNumber 
	PRINT 'dbo.GetNumericStringWithPrefixNumber успешно удалена'
END
ELSE
	PRINT 'dbo.GetNumericStringWithPrefixNumber уже удалена'


IF OBJECT_ID (N'dbo.GetDayString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDayString 
	PRINT 'dbo.GetDayString успешно удалена'
END
ELSE
	PRINT 'dbo.GetDayString уже удалена'


IF OBJECT_ID (N'dbo.GetPercentString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPercentString 
	PRINT 'dbo.GetPercentString успешно удалена'
END
ELSE
	PRINT 'dbo.GetPercentString уже удалена'


IF OBJECT_ID (N'dbo.GetPercentStringWithoutAnd', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPercentStringWithoutAnd 
	PRINT 'dbo.GetPercentStringWithoutAnd успешно удалена'
END
ELSE
	PRINT 'dbo.GetPercentStringWithoutAnd уже удалена'


IF OBJECT_ID (N'dbo.GetPercentStringDec', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPercentStringDec 
	PRINT 'dbo.GetPercentStringDec успешно удалена'
END
ELSE
	PRINT 'dbo.GetPercentStringDec уже удалена'


IF OBJECT_ID (N'dbo.GetPercentStringWithDecimals', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPercentStringWithDecimals 
	PRINT 'dbo.GetPercentStringWithDecimals успешно удалена'
END
ELSE
	PRINT 'dbo.GetPercentStringWithDecimals уже удалена'


IF OBJECT_ID (N'dbo.GetMonthString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetMonthString 
	PRINT 'dbo.GetMonthString успешно удалена'
END
ELSE
	PRINT 'dbo.GetMonthString уже удалена'


IF OBJECT_ID (N'dbo.GetCalendarMonthString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetCalendarMonthString 
	PRINT 'dbo.GetCalendarMonthString успешно удалена'
END
ELSE
	PRINT 'dbo.GetCalendarMonthString уже удалена'


IF OBJECT_ID (N'dbo.GetNumberString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetNumberString 
	PRINT 'dbo.GetNumberString успешно удалена'
END
ELSE
	PRINT 'dbo.GetNumberString уже удалена'


IF OBJECT_ID (N'dbo.GetNumberStringWithPadeg', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetNumberStringWithPadeg 
	PRINT 'dbo.GetNumberStringWithPadeg успешно удалена'
END
ELSE
	PRINT 'dbo.GetNumberStringWithPadeg уже удалена'


IF OBJECT_ID (N'dbo.DateDiffYear', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.DateDiffYear 
	PRINT 'dbo.DateDiffYear успешно удалена'
END
ELSE
	PRINT 'dbo.DateDiffYear уже удалена'


IF OBJECT_ID (N'dbo.DateDiffMonth', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.DateDiffMonth 
	PRINT 'dbo.DateDiffMonth успешно удалена'
END
ELSE
	PRINT 'dbo.DateDiffMonth уже удалена'


IF OBJECT_ID (N'dbo.DateDiffDay', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.DateDiffDay 
	PRINT 'dbo.DateDiffDay успешно удалена'
END
ELSE
	PRINT 'dbo.DateDiffDay уже удалена'


IF OBJECT_ID (N'dbo.DateDiffAsString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.DateDiffAsString 
	PRINT 'dbo.DateDiffAsString успешно удалена'
END
ELSE
	PRINT 'dbo.DateDiffAsString уже удалена'


IF OBJECT_ID (N'dbo.CurrencyConverter', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.CurrencyConverter 
	PRINT 'dbo.CurrencyConverter успешно удалена'
END
ELSE
	PRINT 'dbo.CurrencyConverter уже удалена'


IF OBJECT_ID (N'dbo.GetValuta', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetValuta 
	PRINT 'dbo.GetValuta успешно удалена'
END
ELSE
	PRINT 'dbo.GetValuta уже удалена'


IF OBJECT_ID (N'dbo.GetDateString', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDateString 
	PRINT 'dbo.GetDateString успешно удалена'
END
ELSE
	PRINT 'dbo.GetDateString уже удалена'


IF OBJECT_ID (N'dbo.GetDateStringWithCommas', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDateStringWithCommas 
	PRINT 'dbo.GetDateStringWithCommas успешно удалена'
END
ELSE
	PRINT 'dbo.GetDateStringWithCommas уже удалена'


IF OBJECT_ID (N'dbo.GetDay', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDay 
	PRINT 'dbo.GetDay успешно удалена'
END
ELSE
	PRINT 'dbo.GetDay уже удалена'


IF OBJECT_ID (N'dbo.GetMonth', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetMonth 
	PRINT 'dbo.GetMonth успешно удалена'
END
ELSE
	PRINT 'dbo.GetMonth уже удалена'


IF OBJECT_ID (N'dbo.GetYear', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetYear 
	PRINT 'dbo.GetYear успешно удалена'
END
ELSE
	PRINT 'dbo.GetYear уже удалена'


IF OBJECT_ID (N'dbo.ToCrmDateTime', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.ToCrmDateTime 
	PRINT 'dbo.ToCrmDateTime успешно удалена'
END
ELSE
	PRINT 'dbo.ToCrmDateTime уже удалена'


IF OBJECT_ID (N'dbo.FormatCrmDateTime', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.FormatCrmDateTime 
	PRINT 'dbo.FormatCrmDateTime успешно удалена'
END
ELSE
	PRINT 'dbo.FormatCrmDateTime уже удалена'


IF OBJECT_ID (N'dbo.FormatDate', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.FormatDate 
	PRINT 'dbo.FormatDate успешно удалена'
END
ELSE
	PRINT 'dbo.FormatDate уже удалена'


IF OBJECT_ID (N'dbo.DeclinationOfNames', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.DeclinationOfNames 
	PRINT 'dbo.DeclinationOfNames успешно удалена'
END
ELSE
	PRINT 'dbo.DeclinationOfNames уже удалена'

IF OBJECT_ID (N'dbo.GetDollarStringWithCent', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDollarStringWithCent 
		PRINT 'dbo.GetDollarStringWithCent успешно удалена'
END
ELSE
	PRINT 'dbo.GetDollarStringWithCent уже удалена'


IF OBJECT_ID (N'dbo.GetDollarStringWithCentAndBrackets', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetDollarStringWithCentAndBrackets 
	PRINT 'dbo.GetDollarStringWithCentAndBrackets успешно удалена'
END
ELSE
	PRINT 'dbo.GetDollarStringWithCentAndBrackets уже удалена'

IF OBJECT_ID (N'dbo.GetEuroStringWithEurocent', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetEuroStringWithEurocent 
		PRINT 'dbo.GetEuroStringWithEurocent успешно удалена'
END
ELSE
	PRINT 'dbo.GetEuroStringWithEurocent уже удалена'


IF OBJECT_ID (N'dbo.GetEuroStringWithEurocentAndBrackets', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetEuroStringWithEurocentAndBrackets 
	PRINT 'dbo.GetEuroStringWithEurocentAndBrackets успешно удалена'
END
ELSE
	PRINT 'dbo.GetEuroStringWithEurocentAndBrackets уже удалена'

IF OBJECT_ID (N'dbo.GetPoundsterlingStringWithPerry', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPoundsterlingStringWithPerry 
		PRINT 'dbo.GetPoundsterlingStringWithPerry успешно удалена'
END
ELSE
	PRINT 'dbo.GetPoundsterlingStringWithPerry уже удалена'


IF OBJECT_ID (N'dbo.GetPoundsterlingWithPerryAndBrackets', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetPoundsterlingWithPerryAndBrackets 
	PRINT 'dbo.GetPoundsterlingWithPerryAndBrackets успешно удалена'
END
ELSE
	PRINT 'dbo.GetPoundsterlingWithPerryAndBrackets уже удалена'

IF OBJECT_ID (N'dbo.GetYuanStringWithFyn', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetYuanStringWithFyn 
		PRINT 'dbo.GetYuanStringWithFyn успешно удалена'
END
ELSE
	PRINT 'dbo.GetYuanStringWithFyn уже удалена'


IF OBJECT_ID (N'dbo.GetYuanStringWithFynAndBrackets', N'FS') IS NOT NULL
BEGIN  
	DROP FUNCTION dbo.GetYuanStringWithFynAndBrackets 
	PRINT 'dbo.GetYuanStringWithFynAndBrackets успешно удалена'
END
ELSE
	PRINT 'dbo.GetYuanStringWithFynAndBrackets уже удалена'