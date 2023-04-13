using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataByWords.DataByWords
{
    public class DataByWordsSql
    {
        [SqlFunction]
        public static string GetDollarString(decimal value, int padeg)
        {
            return DataByWords.GetDollarString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetEuroString(decimal value, int padeg)
        {
            return DataByWords.GetEuroString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetRubleString(decimal value, int padeg)
        {
            return DataByWords.GetRubleString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetRubleStringWithKopeika(decimal value, int padeg)
        {
            return DataByWords.GetRubleStringWithKopeika(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetRubleStringWithKopeikaAndBrackets(decimal value, int padeg)
        {
            return DataByWords.GetRubleStringWithKopeikaAndBrackets(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetRRubleString(decimal value, int padeg)
        {
            return DataByWords.GetRRubleString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetNumericString(decimal value, int padeg)
        {
            return DataByWords.GetNumericString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetNumericStringFS(decimal value, int padeg, int fs)
        {
            return DataByWords.GetNumericString(value, (DataByWords.Padej)padeg, (DataByWords.FractionSpelling)fs);
        }

        [SqlFunction]
        public static string GetNumericStringWithPrefixNumber(decimal value, int padeg, int fs)
        {
            return DataByWords.GetNumericStringWithPrefixNumber(value, (DataByWords.Padej)padeg, (DataByWords.FractionSpelling)fs);
        }

        [SqlFunction]
        public static string GetDayString(decimal value, int padeg)
        {
            return DataByWords.GetDayString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetPercentString(decimal value, int padeg)
        {
            return DataByWords.GetPercentString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetPercentStringWithoutAnd(string value, int padeg)
        {
            return DataByWords.GetPercentStringWithoutAnd(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetPercentStringDec(decimal value, int padeg, int decimals)
        {
            return DataByWords.GetPercentString(value, (DataByWords.Padej)padeg, decimals);
        }

        [SqlFunction]
        public static string GetPercentStringWithDecimals(decimal value, int padeg)
        {
            return DataByWords.GetPercentStringWithDecimals(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetMonthString(decimal value, int padeg)
        {
            return DataByWords.GetMonthString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetCalendarMonthString(decimal value, int padeg)
        {
            return DataByWords.GetCalendarMonthString(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetNumberString(decimal value, int fs)
        {
            return DataByWords.GetNumberString(value, (DataByWords.FractionSpelling)fs);
        }

        [SqlFunction]
        public static string GetNumberStringWithPadeg(decimal value, int padeg, int fs)
        {
            return DataByWords.GetNumberString(value, (DataByWords.Padej)padeg, (DataByWords.FractionSpelling)fs);
        }

        [SqlFunction]
        public static string DateDiffYear(string date1, string date2)
        {
            return DataByWords.DateDiffYear(date1, date2);
        }

        [SqlFunction]
        public static string DateDiffMonth(string date1, string date2)
        {
            return DataByWords.DateDiffMonth(date1, date2);
        }

        [SqlFunction]
        public static string DateDiffDay(string date1, string date2)
        {
            return DataByWords.DateDiffDay(date1, date2);
        }

        [SqlFunction]
        public static string DateDiffAsString(string date1, string date2)
        {
            return DataByWords.DateDiffAsString(date1, date2);
        }

        [SqlFunction]
        public static decimal CurrencyConverter(decimal dec, int from, int to, decimal dollarRate, decimal euroRate)
        {
            return DataByWords.CurrencyConverter(dec, from, to, dollarRate, euroRate);
        }

        [SqlFunction]
        public static string GetValuta(decimal dec, int padeg, int valuta)
        {
            return DataByWords.GetValuta(dec, (DataByWords.Padej)padeg, (DataByWords.Valuta)valuta);
        }

        [SqlFunction]
        public static string GetDateString(string datestr, int padej, int format)
        {
            return DataByWords.GetDateString(datestr, (DataByWords.Padej)padej, (DataByWords.DateFormat)format);
        }

        [SqlFunction]
        public static string GetDateStringWithCommas(string datestr)
        {
            return DataByWords.GetDateStringWithCommas(datestr);
        }

        [SqlFunction]
        public static string GetDay(string datestr)
        {
            return DataByWords.GetDay(datestr);
        }

        [SqlFunction]
        public static string GetMonth(string datestr)
        {
            return DataByWords.GetMonth(datestr);
        }

        [SqlFunction]
        public static string GetYear(string datestr)
        {
            return DataByWords.GetYear(datestr);
        }

        [SqlFunction]
        public static string ToCrmDateTime(string datestr)
        {
            return DataByWords.ToCrmDateTime(datestr);
        }

        [SqlFunction]
        public static string FormatCrmDateTime(string crmDateTime, string format)
        {
            return DataByWords.FormatCrmDateTime(crmDateTime, format);
        }

        [SqlFunction]
        public static string FormatDate(string datestr, string format)
        {
            return DataByWords.FormatDate(datestr, format);
        }

        [SqlFunction]
        public static string DeclinationOfNames(string lastname, string firstname, string middlename, int sex, int padej)
        {
            return DataByWords.DeclinationOfNames(lastname, firstname, middlename, (DataByWords.Sex)sex, (DataByWords.Padej)padej);
        }

        [SqlFunction]
        public static string GetDollarStringWithCent(decimal value, int padeg)
        {
            return DataByWords.GetDollarStringWithCent(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetDollarStringWithCentAndBrackets(decimal value, int padeg)
        {
            return DataByWords.GetDollarStringWithCentAndBrackets(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetEuroStringWithEurocent(decimal value, int padeg)
        {
            return DataByWords.GetEuroStringWithEurocent(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetEuroStringWithEurocentAndBrackets(decimal value, int padeg)
        {
            return DataByWords.GetEuroStringWithEurocentAndBrackets(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetPoundsterlingStringWithPerry(decimal value, int padeg)
        {
            return DataByWords.GetPoundsterlingStringWithPerry(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetPoundsterlingWithPerryAndBrackets(decimal value, int padeg)
        {
            return DataByWords.GetPoundsterlingWithPerryAndBrackets(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetYuanStringWithFyn(decimal value, int padeg)
        {
            return DataByWords.GetYuanStringWithFyn(value, (DataByWords.Padej)padeg);
        }

        [SqlFunction]
        public static string GetYuanStringWithFynAndBrackets(decimal value, int padeg)
        {
            return DataByWords.GetYuanStringWithFynAndBrackets(value, (DataByWords.Padej)padeg);
        }
    }
}
