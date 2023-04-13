SELECT 'GetDollarString' as Name, dbo.GetDollarString(11.11, 1) as Value
union all
SELECT 'GetEuroString', dbo.GetEuroString(11.11, 1)
union all
SELECT 'GetRubleString', dbo.GetRubleString(11.11, 1)
union all
SELECT 'GetRubleStringWithKopeika', dbo.GetRubleStringWithKopeika(11.11, 1)
union all
SELECT 'GetRubleStringWithKopeikaAndBrackets', dbo.GetRubleStringWithKopeikaAndBrackets(11.11, 1)
union all
SELECT 'GetRRubleString', dbo.GetRRubleString(11.11, 1)
union all
SELECT 'GetNumericString', dbo.GetNumericString(11.11, 1)
union all
SELECT 'GetNumericStringFS', dbo.GetNumericStringFS(11.11, 1, 1)
union all
SELECT 'GetNumericStringWithPrefixNumber', dbo.GetNumericStringWithPrefixNumber(11.11, 1,1)
union all
SELECT 'GetDayString', dbo.GetDayString(11.11, 1)
union all
SELECT 'GetPercentString', dbo.GetPercentString (15.2, 2)
union all
SELECT 'GetPercentStringWithoutAnd', dbo.GetPercentStringWithoutAnd('11.55', 1)
union all
SELECT 'GetPercentStringDec', dbo.GetPercentStringDec(15.5, 1,1)
union all
SELECT 'GetPercentStringWithDecimals', dbo.GetPercentStringWithDecimals(15.5, 1)
union all
SELECT 'GetMonthString', dbo.GetMonthString(15.5, 1)
union all
SELECT 'GetCalendarMonthString', dbo.GetCalendarMonthString(15.5, 1)
union all
SELECT 'GetNumberString', dbo.GetNumberString(15.5, 1)
union all
SELECT 'GetNumberStringWithPadeg', dbo.GetNumberStringWithPadeg(15.5, 1,1)
union all
SELECT 'DateDiffYear', dbo.DateDiffYear('01.02.2001', '02.03.2004')
union all
SELECT 'DateDiffMonth', dbo.DateDiffMonth('01.02.2001', '02.03.2004')
union all
SELECT 'DateDiffDay', dbo.DateDiffDay('01.02.2001', '02.03.2004')
union all
SELECT 'DateDiffAsString', dbo.DateDiffAsString('01.02.2001', '02.03.2004')
union all
SELECT 'GetValuta', dbo.GetValuta(19.20, 1,1)
union all
SELECT 'GetDateString', dbo.GetDateString('19.12.2003', 1,2)
union all
SELECT 'GetDateStringWithCommas', dbo.GetDateStringWithCommas('19.12.2003')
union all
SELECT 'GetDay', dbo.GetDay('19.12.2003')
union all
SELECT 'GetMonth', dbo.GetMonth('19.12.2003')
union all
SELECT 'GetYear', dbo.GetYear('19.12.2003')
union all
SELECT 'ToCrmDateTime', dbo.ToCrmDateTime('19.12.2003')
union all
SELECT 'FormatCrmDateTime', dbo.FormatCrmDateTime('2003-01-12T01:02:04', 'yyyy-MM-dd')
union all
SELECT 'FormatDate', dbo.FormatDate('19.12.2003', 'MM-dd-yyyy')
union all
SELECT 'DeclinationOfNames', dbo.DeclinationOfNames('Петров', 'Петр', 'Петрович', 1, 3)
union all
SELECT 'DeclinationOfNames', dbo.DeclinationOfNames('Милошевич', 'Павел', 'Зоранович', 1, 2)
union all
SELECT 'GetDollarStringWithCent', dbo.GetDollarStringWithCent(11.11, 1)
union all
SELECT 'GetDollarStringWithCentAndBrackets', dbo.GetDollarStringWithCentAndBrackets(11.11, 1)
union all
SELECT 'GetEuroStringWithEurocent', dbo.GetEuroStringWithEurocent(11.11, 1)
union all
SELECT 'GetEuroStringWithEurocentAndBrackets', dbo.GetEuroStringWithEurocentAndBrackets(11.11, 1)
union all
SELECT 'GetPoundsterlingStringWithPerry', dbo.GetPoundsterlingStringWithPerry(11.11, 1)
union all
SELECT 'GetPoundsterlingWithPerryAndBrackets', dbo.GetPoundsterlingWithPerryAndBrackets(11.11, 1)
union all
SELECT 'GetYuanStringWithFyn', dbo.GetYuanStringWithFyn(11.11, 1)
union all
SELECT 'GetYuanStringWithFynAndBrackets', dbo.GetYuanStringWithFynAndBrackets(11.11, 1)

SELECT 'CurrencyConverter' as name, dbo.CurrencyConverter(15.5, 1, 2, 31.11, 42.15) as value