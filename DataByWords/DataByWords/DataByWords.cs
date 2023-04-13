using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataByWords.DataByWords
{
    public class DataByWords
    {
        public enum Padej
        {
            Imen = 1,
            Rod,
            Dat,
            Vin,
            Tvor,
            Pred
        }

        public enum DateFormat
        {
            ShortDate = 1,
            LongDate
        }

        public enum Valuta
        {
            Rouble = 1,
            Dollar,
            Euro
        }

        public enum FractionSpelling
        {
            Off,
            On,
            PartsAsDigits
        }

        public enum Sex
        {
            Male = 1,
            Female
        }

        private static bool myDecimalTryParce(string Value, out decimal value)
        {
            try
            {
                value = Math.Abs(Convert.ToDecimal(Value.Replace(".", ",")));
                return true;
            }
            catch
            {
                try
                {
                    value = Math.Abs(Convert.ToDecimal(Value.Replace(",", ".")));
                    return true;
                }
                catch
                {
                    value = -1m;
                    return false;
                }
            }
        }

        public static string GetDollarString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, "доллар", "доллара", "долларов", "доллара", "долларов", "долларов", "доллару", "долларам", "долларам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100) " + empty3;
        }

        public static string GetDollarString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetDollarString(value, padeg);
        }

        public static string GetEuroString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100) " + empty3;
        }

        public static string GetEuroString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetEuroString(value, padeg);
        }

        public static string GetRubleString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, "рубль", "рубля", "рублей", "рубля", "рублей", "рублей", "рублю", "рублям", "рублям", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100) " + empty3;
        }

        public static string GetRubleString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetRubleString(value, padeg);
        }

        public static string GetRubleStringWithKopeika(decimal val, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string text = string.Empty;
            CurPhrase(val, "рубль", "рубля", "рублей", "рубля", "рублей", "рублей", "рублю", "рублям", "рублям", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref text);
            int num = 0;
            if (val.ToString().Contains("."))
            {
                Convert.ToInt32(val.ToString().Split('.')[0]);
                num = Convert.ToInt32(val.ToString().Split('.')[1]);
            }
            else if (val.ToString().Contains(","))
            {
                Convert.ToInt32(val.ToString().Split(',')[0]);
                num = Convert.ToInt32(val.ToString().Split(',')[1]);
            }
            else
            {
                Convert.ToInt32(val.ToString());
                num = 0;
            }
            text = getPraseEnding(num, "копейка", "копейки", "копеек", "копейку", "копейки", "копеек", "копейке", "копейкам", "копейкам", padeg);
            return empty2 + " " + empty3 + " " + empty4 + " " + text;
        }

        public static string GetRubleStringWithKopeika(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal val;
            if (!myDecimalTryParce(Value, out val))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetRubleStringWithKopeika(val, padeg);
        }

        public static string GetRubleStringWithKopeikaAndBrackets(decimal val, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string text = string.Empty;
            CurPhrase(val, "рубль", "рубля", "рублей", "рубля", "рублей", "рублей", "рублю", "рублям", "рублям", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref text, false);
            int num;
            if (val.ToString().Contains("."))
            {
                Convert.ToInt32(val.ToString().Split('.')[0]);
                num = Convert.ToInt32(val.ToString().Split('.')[1]);
            }
            else if (val.ToString().Contains(","))
            {
                Convert.ToInt32(val.ToString().Split(',')[0]);
                num = Convert.ToInt32(val.ToString().Split(',')[1]);
            }
            else
            {
                Convert.ToInt32(val.ToString());
                num = 0;
            }
            text = getPraseEnding(num, "копейка", "копейки", "копеек", "копейку", "копейки", "копеек", "копейке", "копейкам", "копейкам", padeg);
            return "(" + empty2 + ") " + empty3 + " " + empty4 + " " + text;
        }

        public static string GetRubleStringWithKopeikaAndBrackets(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal val;
            if (!myDecimalTryParce(Value, out val))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetRubleStringWithKopeikaAndBrackets(val, padeg);
        }

        public static string GetRRubleString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, "российский рубль", "российского рубля", "российских рублей", "российского рубля", "российских рублей", "российских рублей", "российскому рублю", "российским рублям", "российским рублям", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100) " + empty3;
        }

        public static string GetRRubleString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetRRubleString(value, padeg);
        }

        public static string GetNumericString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100)";
        }

        public static string GetNumericString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetNumericString(value, padeg);
        }

        public static string GetNumericString(decimal value, Padej padeg, FractionSpelling fs)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", padeg, fs, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2.TrimStart('(') + empty4.TrimEnd(')') + ")";
        }

        public static string GetNumericString(string Value, Padej padeg, FractionSpelling fs)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetNumericString(value, padeg, fs);
        }

        public static string GetNumericStringWithPrefixNumber(decimal value, Padej padeg, FractionSpelling fs)
        {
            return value.ToString() + " (" + GetNumberString(value, padeg, fs) + ")";
        }

        public static string GetNumericStringWithPrefixNumber(string Value, Padej padeg, FractionSpelling fs)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetNumericStringWithPrefixNumber(value, padeg, fs);
        }

        public static string GetDayString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, " день", " дня", " дней", " дня", " дней", " дней", " дню", " дням", " дням", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return empty2 + empty3;
        }

        public static string GetDayString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetDayString(value, padeg);
        }

        public static string GetPercentString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, " процент", " процента", " процентов", " процента", " процентов", " процентов", " проценту", " процентам", " процентам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + empty4 + ")" + empty3;
        }

        public static string GetPercentString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetPercentString(value, padeg);
        }

        public static string GetPercentStringWithoutAnd(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal money;
            if (!myDecimalTryParce(Value, out money))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(money, " процент", " процента", " процентов", " процента", " процентов", " процентов", " проценту", " процентам", " процентам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2.Replace(" и", string.Empty) + empty4 + ")" + empty3;
        }

        public static string GetPercentString(decimal value, Padej padeg, int decimals)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, " процент", " процента", " процентов", " процента", " процентов", " процентов", " проценту", " процентам", " процентам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            decimal num = decimal.Round(value, decimals);
            return ((decimals == 2) ? num.ToString("N2") : num.ToString()) + "% (" + empty2 + empty4 + ")" + empty3;
        }

        public static string GetPercentString(string Value, Padej padeg, int decimals)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetPercentString(value, padeg, decimals);
        }

        public static string GetPercentStringWithDecimals(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase(value, "процент", "процента", "процентов", "процента", "процентов", "процентов", "проценту", "процентам", "процентам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + " и " + empty4 + "/100) " + empty3;
        }

        public static string GetPercentStringWithDecimals(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetPercentStringWithDecimals(value, padeg);
        }

        public static string GetMonthString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, "месяц", "месяца", "месяцев", "месяца", "месяцев", "месяцев", "месяцу", "месяцам", "месяцам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return "(" + empty2 + ") " + empty3;
        }

        public static string GetMonthString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetMonthString(value, padeg);
        }

        public static string GetMonthStringSql(decimal value, int padeg)
        {
            return GetMonthString(value, (Padej)padeg);
        }

        public static string GetCalendarMonthString(decimal value, Padej padeg)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, "календарный месяц", "календарных месяца", "календарных месяцев", "календарных месяца", "календарных месяцев", "календарных месяцев", "календарному месяцу", "календарным месяцам", "календарным месяцам", "", "", "", padeg, ref empty2, ref empty3, ref empty4, ref empty5);
            return Convert.ToInt32(value).ToString() + " (" + empty2 + ") " + empty3;
        }

        public static string GetCalendarMonthString(string Value, Padej padeg)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetCalendarMonthString(value, padeg);
        }

        public static string GetNumberString(decimal value, FractionSpelling fs)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Padej.Imen, fs, ref empty2, ref empty3, ref empty4, ref empty5);
            return empty2 + empty4;
        }

        public static string GetNumberString(string Value, FractionSpelling fs)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetNumberString(value, fs);
        }

        public static string GetNumberString(decimal value, Padej padeg, FractionSpelling fs)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            string empty3 = string.Empty;
            string empty4 = string.Empty;
            string empty5 = string.Empty;
            CurPhrase2(value, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, padeg, fs, ref empty2, ref empty3, ref empty4, ref empty5);
            return empty2 + empty4;
        }

        public static string GetNumberString(string Value, Padej padeg, FractionSpelling fs)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal value;
            if (!myDecimalTryParce(Value, out value))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetNumberString(value, padeg, fs);
        }

        public static string DateDiffYear(string date1, string date2)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(date1, out dateTime))
            {
                throw new Exception("Не удалось преобразовать в дату первый входной параметр функции!");
            }

            DateTime dateTime2;
            if (!DateTime.TryParse(date2, out dateTime2))
            {
                throw new Exception("Не удалось преобразовать в дату второй входной параметр функции!");
            }
            if (dateTime > dateTime2)
            {
                DateTime dateTime3 = dateTime2;
                dateTime2 = dateTime;
                dateTime = dateTime3;
            }
            int num = dateTime2.Year - dateTime.Year;
            if (dateTime.Month == dateTime2.Month)
            {
                if (dateTime.Day < dateTime2.Day)
                {
                    return (num - 1).ToString();
                }
            }
            else if (dateTime2.Month < dateTime.Month)
            {
                return (num - 1).ToString();
            }
            return num.ToString();
        }

        public static string DateDiffMonth(string date1, string date2)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(date1, out dateTime))
            {
                throw new Exception("Не удалось преобразовать в дату первый входной параметр функции!");
            }
            DateTime dateTime2;
            if (!DateTime.TryParse(date2, out dateTime2))
            {
                throw new Exception("Не удалось преобразовать в дату второй входной параметр функции!");
            }
            if (dateTime > dateTime2)
            {
                DateTime dateTime3 = dateTime2;
                dateTime2 = dateTime;
                dateTime = dateTime3;
            }
            bool flag = false;
            int num = dateTime2.Year - dateTime.Year;
            if (dateTime.Month == dateTime2.Month)
            {
                if (dateTime.Day < dateTime2.Day)
                {
                    num--;
                    flag = true;
                }
            }
            else if (dateTime2.Month < dateTime.Month)
            {
                num--;
                flag = true;
            }
            int num2 = (!flag) ? (dateTime2.Month - dateTime.Month) : (12 - dateTime.Month + dateTime2.Month);
            if (dateTime2.Day < dateTime.Day)
            {
                num2--;
            }
            return (num * 12 + num2).ToString();
        }

        public static string DateDiffDay(string date1, string date2)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(date1, out dateTime))
            {
                throw new Exception("Не удалось преобразовать в дату первый входной параметр функции!");
            }
            DateTime value;
            if (!DateTime.TryParse(date2, out value))
            {
                throw new Exception("Не удалось преобразовать в дату второй входной параметр функции!");
            }
            return Math.Abs(dateTime.Subtract(value).Days).ToString();
        }

        public static string DateDiffAsString(string dateFrom, string dateTo)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(dateFrom, out dateTime))
            {
                throw new Exception("Не удалось преобразовать в дату первый входной параметр функции!");
            }
            DateTime dateTime2;
            if (!DateTime.TryParse(dateTo, out dateTime2))
            {
                throw new Exception("Не удалось преобразовать в дату второй входной параметр функции!");
            }
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            DateTime dateTime3 = (dateTime <= dateTime2) ? dateTime : dateTime2;
            DateTime t = (dateTime <= dateTime2) ? dateTime2 : dateTime;
            DateTime t2 = (!DateTime.IsLeapYear(dateTime3.Year) || DateTime.IsLeapYear(t.Year) || dateTime3.Month != 2 || dateTime3.Day != 29) ? new DateTime(t.Year, dateTime3.Month, dateTime3.Day) : new DateTime(t.Year, dateTime3.Month, dateTime3.Day - 1);
            num = t.Year - dateTime3.Year - ((t2 > t) ? 1 : 0);
            num2 = t.Month - dateTime3.Month + 12 * ((t2 > t) ? 1 : 0);
            num3 = t.Day - dateTime3.Day;
            if (num3 < 0)
            {
                if (t.Day == DateTime.DaysInMonth(t.Year, t.Month) && (dateTime3.Day >= DateTime.DaysInMonth(t.Year, t.Month) || dateTime3.Day >= DateTime.DaysInMonth(t.Year, dateTime3.Month)))
                {
                    num3 = 0;
                }
                else
                {
                    num2--;
                    if (DateTime.DaysInMonth(t.Year, t.Month) == DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) && t.Month != dateTime3.Month)
                    {
                        int num4 = (t.Month - 1 > 0) ? DateTime.DaysInMonth(t.Year, t.Month - 1) : 31;
                        num3 = num4 + num3;
                    }
                    else
                    {
                        num3 = DateTime.DaysInMonth(t.Year, (t.Month == 1) ? 12 : (t.Month - 1)) + num3;
                    }
                }
            }
            string text = "";
            if (num > 0)
            {
                text = text + num + "г. ";
            }
            if (num2 > 0)
            {
                text = text + num2 + "мес. ";
            }
            if (num3 > 0)
            {
                text = text + num3 + "д.";
            }
            return text;
        }

        private static string getPraseEnding(int value, string rublImen, string rublyaImen, string rubleyImen, string rublyaRod, string rubleyRod, string rubley_Rod, string rubluDat, string rublyamDat, string rublyam_Dat, Padej padej)
        {
            int num = Convert.ToInt32(value.ToString()[value.ToString().Length - 1].ToString());
            switch (padej)
            {
                case Padej.Imen:
                    if (value >= 11 && value <= 19)
                    {
                        return rubleyImen;
                    }
                    if (num == 1)
                    {
                        return rublImen;
                    }
                    if (num >= 2 && num <= 4)
                    {
                        return rublyaImen;
                    }
                    if ((num < 5 || num > 9) && num != 0)
                    {
                        break;
                    }
                    return rubleyImen;
                case Padej.Rod:
                    if (value >= 11 && value <= 19)
                    {
                        return rubley_Rod;
                    }
                    if (num == 1)
                    {
                        return rublyaRod;
                    }
                    if (num >= 2 && num <= 4)
                    {
                        return rubleyRod;
                    }
                    if ((num < 5 || num > 9) && num != 0)
                    {
                        break;
                    }
                    return rubley_Rod;
                case Padej.Dat:
                    if (value >= 11 && value <= 19)
                    {
                        return rubley_Rod;
                    }
                    if (num == 1)
                    {
                        return rubluDat;
                    }
                    if (num >= 2 && num <= 4)
                    {
                        return rublyamDat;
                    }
                    if ((num < 5 || num > 9) && num != 0)
                    {
                        break;
                    }
                    return rublyam_Dat;
            }
            return string.Empty;
        }

        private static string NumPhrase(ulong Value, bool IsMale, bool firstSymbol)
        {
            if (Value == 0)
            {
                if (!firstSymbol)
                {
                    return "ноль";
                }
                return "Ноль";
            }
            string[] array = new string[20]
            {
                "",
                " од",
                " дв",
                " три",
                " четыре",
                " пять",
                " шесть",
                " семь",
                " восемь",
                " девять",
                " десять",
                " одиннадцать",
                " двенадцать",
                " тринадцать",
                " четырнадцать",
                " пятнадцать",
                " шестнадцать",
                " семнадцать",
                " восемнадцать",
                " девятнадцать"
            };
            string[] array2 = new string[10]
            {
                "",
                "",
                " двадцать",
                " тридцать",
                " сорок",
                " пятьдесят",
                " шестьдесят",
                " семьдесят",
                " восемьдесят",
                " девяносто"
            };
            string[] array3 = new string[10]
            {
                "",
                " сто",
                " двести",
                " триста",
                " четыреста",
                " пятьсот",
                " шестьсот",
                " семьсот",
                " восемьсот",
                " девятьсот"
            };
            string[] array4 = new string[8]
            {
                "",
                "",
                " тысяч",
                " миллион",
                " миллиард",
                " триллион",
                " квадрилион",
                " квинтилион"
            };
            string text = "";
            byte b = 1;
            while (Value != 0)
            {
                ushort num = (ushort)(Value % 1000uL);
                Value = (Value - num) / 1000uL;
                if (num > 0)
                {
                    byte b2 = (byte)((num - (int)num % 100) / 100);
                    byte b3 = (byte)((int)num % 10);
                    byte b4 = (byte)((num - b2 * 100 - b3) / 10);
                    if (b4 == 1)
                    {
                        b3 = (byte)(b3 + 10);
                    }
                    bool isMale = b > 2 || (b == 1 && IsMale);
                    text = array3[b2] + array2[b4] + array[b3] + EndDek1(b3, isMale) + array4[b] + EndTh(b, b3) + text;
                }
                b = (byte)(b + 1);
            }
            if (firstSymbol)
            {
                text = text.Substring(1, 1).ToUpper() + text.Substring(2);
            }
            return text;
        }

        private static void CurPhrase(decimal money, string word1, string word234, string wordmore, string word1_r, string word234_r, string wordmore_r, string word1_d, string word234_d, string wordmore_d, string sword1, string sword234, string swordmore, Padej padeg, ref string one, ref string two, ref string three, ref string four, bool allowTwo = true)
        {
            money = decimal.Round(Math.Abs(money), 2);
            decimal num = decimal.Truncate(money);
            ulong num2 = decimal.ToUInt64(num);
            string text = NumPhrase(num2, true, true) + " ";
            one = text.Trim();
            byte b = (byte)(num2 % 100uL);
            if (b > 19)
            {
                b = (byte)((int)b % 10);
            }
            switch (padeg)
            {
                case Padej.Rod:
                    switch (b)
                    {
                        case 1:
                            two = word1_r;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234_r;
                            break;
                        default:
                            two = wordmore_r;
                            break;
                    }
                    break;
                case Padej.Imen:
                    switch (b)
                    {
                        case 1:
                            two = word1;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234;
                            break;
                        default:
                            two = wordmore;
                            break;
                    }
                    break;
                case Padej.Dat:
                    switch (b)
                    {
                        case 1:
                            two = word1_d;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234_d;
                            break;
                        default:
                            two = wordmore_d;
                            break;
                    }
                    break;
            }
            byte b2 = decimal.ToByte((money - num) * 100m);
            three = ((b2 < 10) ? "0" : "") + b2.ToString();
            if (allowTwo)
            {
                if (b2 > 0)
                {
                    switch (padeg)
                    {
                        case Padej.Rod:
                            two = wordmore_r;
                            break;
                        case Padej.Imen:
                            two = wordmore;
                            break;
                        case Padej.Dat:
                            two = wordmore_d;
                            break;
                    }
                }
            }
            if (b2 > 19)
            {
                b2 = (byte)((int)b2 % 10);
            }
            switch (b2)
            {
                case 1:
                    four = sword1;
                    break;
                case 2:
                case 3:
                case 4:
                    four = sword234;
                    break;
                default:
                    four = swordmore;
                    break;
            }
        }

        private static void CurPhrase2(decimal money, string word1, string word234, string wordmore, string word1_r, string word234_r, string wordmore_r, string word1_d, string word234_d, string wordmore_d, string sword1, string sword234, string swordmore, Padej padeg, ref string one, ref string two, ref string three, ref string four)
        {
            CurPhrase2(money, word1, word234, wordmore, word1_r, word234_r, wordmore_r, word1_d, word234_d, wordmore_d, sword1, sword234, swordmore, padeg, FractionSpelling.Off, ref one, ref two, ref three, ref four);
        }

        private static void CurPhrase2(decimal money, string word1, string word234, string wordmore, string word1_r, string word234_r, string wordmore_r, string word1_d, string word234_d, string wordmore_d, string sword1, string sword234, string swordmore, Padej padeg, FractionSpelling fs, ref string one, ref string two, ref string three, ref string four)
        {
            money = decimal.Round(money, 2);
            decimal num = decimal.Truncate(money);
            ulong num2 = decimal.ToUInt64(num);
            string text = NumPhrase(num2, true, true) + " ";
            one = text.Trim();
            byte b = (byte)(num2 % 100uL);
            if (b > 19)
            {
                b = (byte)((int)b % 10);
            }
            switch (padeg)
            {
                case Padej.Rod:
                    switch (b)
                    {
                        case 1:
                            two = word1_r;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234_r;
                            break;
                        default:
                            two = wordmore_r;
                            break;
                    }
                    break;
                case Padej.Imen:
                    switch (b)
                    {
                        case 1:
                            two = word1;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234;
                            break;
                        default:
                            two = wordmore;
                            break;
                    }
                    break;
                case Padej.Dat:
                    switch (b)
                    {
                        case 1:
                            two = word1_d;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            two = word234_d;
                            break;
                        default:
                            two = wordmore_d;
                            break;
                    }
                    break;
            }
            byte b2 = decimal.ToByte((money - num) * 100m);
            if (fs != FractionSpelling.PartsAsDigits)
            {
                if (b2 != 0)
                {
                    char[] trimChars = new char[1]
                    {
                    '0'
                    };
                    int num3 = (money - num).ToString().Trim(trimChars).Length - 1;
                    if ((int)b2 % 10 == 0)
                    {
                        three = NumPhrase((ulong)((int)b2 / 10), false, false);
                    }
                    else
                    {
                        three = NumPhrase(b2, false, false);
                    }
                    int num4 = int.Parse(b2.ToString().TrimEnd(trimChars));
                    if (num3 == 1)
                    {
                        if (num4 == 1)
                        {
                            three += " десятая";
                        }
                        else
                        {
                            three += " десятых";
                        }
                    }
                    else if (num4 == 1)
                    {
                        three += " сотая";
                    }
                    else
                    {
                        three += " сотых";
                    }
                }
                else if (fs.GetHashCode() == 1)
                {
                    three += " ноль десятых";
                }
            }
            else
            {
                three = b2.ToString();
                if (b2.ToString().Length == 1)
                    three = $"0{three}";
                three = $" {three}/100";
            }
            if (b2 > 0 || fs.GetHashCode() == 1)
            {
                switch (padeg)
                {
                    case Padej.Rod:
                        two = wordmore_r;
                        break;
                    case Padej.Imen:
                        two = wordmore;
                        break;
                    case Padej.Dat:
                        two = wordmore_d;
                        break;
                }
                if (fs != FractionSpelling.PartsAsDigits)
                {
                    one = NumPhrase(num2, false, true);
                    byte b3 = byte.Parse(num2.ToString()[num2.ToString().Length - 1].ToString());
                    if (b3 == 1 && num2 != 11)
                    {
                        one += " целая";
                    }
                    else
                    {
                        one += " целых";
                    }
                }
                one += " и";
            }
            else if (b2 == 0 && fs == FractionSpelling.PartsAsDigits)
            {
                one += " и";
            }
            if (b2 > 19)
            {
                b2 = (byte)((int)b2 % 10);
            }
            switch (b2)
            {
                case 1:
                    four = sword1;
                    break;
                case 2:
                case 3:
                case 4:
                    four = sword234;
                    break;
                default:
                    four = swordmore;
                    break;
            }
        }

        private static string EndTh(byte ThNum, byte Dek)
        {
            bool flag = Dek >= 2 && Dek <= 4;
            bool flag2 = Dek > 4 || Dek == 0;
            if (ThNum > 2 && flag)
            {
                goto IL_002a;
            }
            if (ThNum == 2 && Dek == 1)
            {
                goto IL_002a;
            }
            if (ThNum > 2 && flag2)
            {
                return "ов";
            }
            if (ThNum == 2 && flag)
            {
                return "и";
            }
            return "";
        IL_002a:
            return "а";
        }

        private static string EndDek1(byte Dek, bool IsMale)
        {
            if (Dek <= 2)
            {
                switch (Dek)
                {
                    case 0:
                        break;
                    case 1:
                        if (IsMale)
                        {
                            return "ин";
                        }
                        return "на";
                    default:
                        if (IsMale)
                        {
                            return "а";
                        }
                        return "е";
                }
            }
            return "";
        }

        public static decimal CurrencyConverter(decimal dec, int from, int to, decimal dollarRate, decimal euroRate)
        {
            decimal d = dec;
            decimal num = euroRate;
            switch (to)
            {
                case 3:
                    switch (from)
                    {
                        case 1:
                            if (num == 0m)
                            {
                                throw new Exception("Не задан курс евро");
                            }
                            d = dec / num;
                            break;
                        case 2:
                            if (dollarRate == 0m)
                            {
                                throw new Exception("Не задан курс доллара");
                            }
                            if (num == 0m)
                            {
                                throw new Exception("Не задан курс евро");
                            }
                            d = dec * dollarRate / num;
                            break;
                    }
                    break;
                case 1:
                    switch (from)
                    {
                        case 3:
                            if (num == 0m)
                            {
                                throw new Exception("Не задан курс евро " + num.ToString());
                            }
                            d = dec * num;
                            break;
                        case 2:
                            if (dollarRate == 0m)
                            {
                                throw new Exception("Не задан курс доллара");
                            }
                            d = dec * dollarRate;
                            break;
                    }
                    break;
                case 2:
                    switch (from)
                    {
                        case 1:
                            if (dollarRate == 0m)
                            {
                                throw new Exception("Не задан курс доллара");
                            }
                            d = dec / dollarRate;
                            break;
                        case 3:
                            if (num == 0m)
                            {
                                throw new Exception("Не задан курс евро");
                            }
                            d = dec * num / dollarRate;
                            break;
                    }
                    break;
            }
            return Math.Round(d, 2);
        }

        public static string GetValuta(decimal dec, Padej padeg, Valuta valuta)
        {
            string result = string.Empty;
            switch (valuta)
            {
                case Valuta.Rouble:
                    result = GetRuble(dec, padeg);
                    break;
                case Valuta.Dollar:
                    result = GetDollar(dec, padeg);
                    break;
                case Valuta.Euro:
                    result = GetEuro(dec, padeg);
                    break;
            }
            return result;
        }

        public static string GetValuta(string Value, Padej padeg, Valuta valuta)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            decimal dec;
            if (!myDecimalTryParce(Value, out dec))
            {
                return "ВНИМАНИЕ: Не удалось преобразовать значение " + Value + " к десятичному значению!";
            }
            return GetValuta(dec, padeg, valuta);
        }

        private static string GetRuble(decimal dec, Padej padeg)
        {
            string text = dec.ToString("N2") + " ";
            text.Replace(".", ",");
            return text + GetRubleString(dec, padeg);
        }

        private static string GetDollar(decimal dec, Padej padeg)
        {
            string text = dec.ToString("N2") + " ";
            text.Replace(".", ",");
            return text + GetDollarString(dec, padeg);
        }

        private static string GetEuro(decimal dec, Padej padeg)
        {
            string text = dec.ToString("N2") + " ";
            text.Replace(".", ",");
            return text + GetEuroString(dec, padeg);
        }

        public static string GetDateString(string datestr, Padej padej, DateFormat format)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            if (datestr.Trim().Length == 0)
            {
                return string.Empty;
            }
            DateTime dateTime = DateTime.Parse(datestr);
            bool flag = false;
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.DateTimeFormat.ShortDatePattern = "d.M.yyyy";
            string text = string.Empty;
            int day = dateTime.Day;
            int month = dateTime.Month;
            string str = string.Empty;
            int year = dateTime.Year;
            string empty = string.Empty;
            switch (month)
            {
                case 1:
                    text = "января";
                    break;
                case 2:
                    text = "февраля";
                    break;
                case 3:
                    text = "марта";
                    break;
                case 4:
                    text = "апреля";
                    break;
                case 5:
                    text = "мая";
                    break;
                case 6:
                    text = "июня";
                    break;
                case 7:
                    text = "июля";
                    break;
                case 8:
                    text = "августа";
                    break;
                case 9:
                    text = "сентября";
                    break;
                case 10:
                    text = "октября";
                    break;
                case 11:
                    text = "ноября";
                    break;
                case 12:
                    text = "декабря";
                    break;
            }
            switch (day)
            {
                case 1:
                    str = "Перв";
                    break;
                case 2:
                    str = "Втор";
                    break;
                case 3:
                    str = "Третье";
                    flag = true;
                    break;
                case 4:
                    str = "Четверт";
                    break;
                case 5:
                    str = "Пят";
                    break;
                case 6:
                    str = "Шест";
                    break;
                case 7:
                    str = "Седьм";
                    break;
                case 8:
                    str = "Восьм";
                    break;
                case 9:
                    str = "Девят";
                    break;
                case 10:
                    str = "Десят";
                    break;
                case 11:
                    str = "Одиннадцат";
                    break;
                case 12:
                    str = "Двенадцат";
                    break;
                case 13:
                    str = "Тринадцат";
                    break;
                case 14:
                    str = "Четырнадцат";
                    break;
                case 15:
                    str = "Пятнадцат";
                    break;
                case 16:
                    str = "Шестнадцат";
                    break;
                case 17:
                    str = "Семнадцат";
                    break;
                case 18:
                    str = "Восемнадцат";
                    break;
                case 19:
                    str = "Девятнадцат";
                    break;
                case 20:
                    str = "Двадцат";
                    break;
                case 21:
                    str = "Двадцать перв";
                    break;
                case 22:
                    str = "Двадцать втор";
                    break;
                case 23:
                    str = "Двадцать третье";
                    flag = true;
                    break;
                case 24:
                    str = "Двадцать четверт";
                    break;
                case 25:
                    str = "Двадцать пят";
                    break;
                case 26:
                    str = "Двадцать шест";
                    break;
                case 27:
                    str = "Двадцать седьм";
                    break;
                case 28:
                    str = "Двадцать восьм";
                    break;
                case 29:
                    str = "Двадцать девят";
                    break;
                case 30:
                    str = "Тридцат";
                    break;
                case 31:
                    str = "Тридцать перв";
                    break;
            }
            str = AddEnding(str, padej, ref flag);
            int num;
            switch (year)
            {
                case 2000:
                    empty = "двухтысячного";
                    break;
                case 1900:
                    empty = "тысяча девятисотого";
                    break;
                default:
                    {
                        empty = ((!(year.ToString().Substring(0, 2) == "19")) ? (empty + "две тысячи ") : (empty + "тысяча девятьсот "));
                        num = int.Parse(year.ToString().Substring(2, 2));
                        switch (int.Parse(year.ToString().Substring(1, 1)))
                        {
                            case 1:
                                empty += "сто ";
                                break;
                            case 2:
                                empty += "двести ";
                                break;
                            case 3:
                                empty += "триста ";
                                break;
                            case 4:
                                empty += "четыреста ";
                                break;
                            case 5:
                                empty += "пятьсот ";
                                break;
                            case 6:
                                empty += "шестьсот ";
                                break;
                            case 7:
                                empty += "семьсот ";
                                break;
                            case 8:
                                empty += "восемьсот ";
                                break;
                            case 9:
                                empty += "девятьсот ";
                                break;
                        }
                        if (num >= 1 && num <= 19)
                        {
                            goto IL_046c;
                        }
                        if (num.ToString().Substring(1, 1) == "0")
                        {
                            goto IL_046c;
                        }
                        int num2 = int.Parse(year.ToString().Substring(2, 1));
                        int num3 = int.Parse(year.ToString().Substring(3, 1));
                        switch (num2)
                        {
                            case 2:
                                empty += "двадцать ";
                                break;
                            case 3:
                                empty += "тридцать ";
                                break;
                            case 4:
                                empty += "сорок ";
                                break;
                            case 5:
                                empty += "пятьдесят ";
                                break;
                            case 6:
                                empty += "шестьдесят ";
                                break;
                            case 7:
                                empty += "семьдесят ";
                                break;
                            case 8:
                                empty += "восемьдесят ";
                                break;
                            case 9:
                                empty += "девяносто ";
                                break;
                        }
                        switch (num3)
                        {
                            case 1:
                                empty += "первое";
                                break;
                            case 2:
                                empty += "второе";
                                break;
                            case 3:
                                empty += "третье";
                                break;
                            case 4:
                                empty += "четвертое";
                                break;
                            case 5:
                                empty += "пятое";
                                break;
                            case 6:
                                empty += "шестое";
                                break;
                            case 7:
                                empty += "седьмое";
                                break;
                            case 8:
                                empty += "восьмое";
                                break;
                            case 9:
                                empty += "девятое";
                                break;
                        }
                        break;
                    }
                IL_046c:
                    switch (num)
                    {
                        case 1:
                            empty += "первого";
                            break;
                        case 2:
                            empty += "второго";
                            break;
                        case 3:
                            empty += "третьего";
                            break;
                        case 4:
                            empty += "четвертого";
                            break;
                        case 5:
                            empty += "пятого";
                            break;
                        case 6:
                            empty += "шестого";
                            break;
                        case 7:
                            empty += "седьмого";
                            break;
                        case 8:
                            empty += "восьмого";
                            break;
                        case 9:
                            empty += "девятого";
                            break;
                        case 10:
                            empty += "десятого";
                            break;
                        case 11:
                            empty += "одиннадцатого";
                            break;
                        case 12:
                            empty += "двенадцатого";
                            break;
                        case 13:
                            empty += "тринадцатого";
                            break;
                        case 14:
                            empty += "четырнадцатого";
                            break;
                        case 15:
                            empty += "пятнадцатого";
                            break;
                        case 16:
                            empty += "шестнадцатого";
                            break;
                        case 17:
                            empty += "семнадцатого";
                            break;
                        case 18:
                            empty += "восемнадцатого";
                            break;
                        case 19:
                            empty += "девятнадцатого";
                            break;
                        case 20:
                            empty += "двадцатого";
                            break;
                        case 30:
                            empty += "тридцатого";
                            break;
                        case 40:
                            empty += "сорокового";
                            break;
                        case 50:
                            empty += "пятидесятого";
                            break;
                        case 60:
                            empty += "шестидесятого";
                            break;
                        case 70:
                            empty += "семидесятого";
                            break;
                        case 80:
                            empty += "восьмидесятого";
                            break;
                        case 90:
                            empty += "девяностого";
                            break;
                    }
                    break;
            }
            return ((format == DateFormat.LongDate) ? str : day.ToString()) + " " + text + " " + ((format == DateFormat.LongDate) ? empty : year.ToString()) + " года";
        }

        public static string GetDateStringWithCommas(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            if (datestr.Trim().Length == 0)
            {
                return string.Empty;
            }
            DateTime dateTime = DateTime.Parse(datestr);
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.DateTimeFormat.ShortDatePattern = "d.M.yyyy";
            string text = string.Empty;
            int day = dateTime.Day;
            int month = dateTime.Month;
            string empty2 = string.Empty;
            int year = dateTime.Year;
            string empty = string.Empty;
            switch (month)
            {
                case 1:
                    text = "января";
                    break;
                case 2:
                    text = "февраля";
                    break;
                case 3:
                    text = "марта";
                    break;
                case 4:
                    text = "апреля";
                    break;
                case 5:
                    text = "мая";
                    break;
                case 6:
                    text = "июня";
                    break;
                case 7:
                    text = "июля";
                    break;
                case 8:
                    text = "августа";
                    break;
                case 9:
                    text = "сентября";
                    break;
                case 10:
                    text = "октября";
                    break;
                case 11:
                    text = "ноября";
                    break;
                case 12:
                    text = "декабря";
                    break;
            }
            switch (day)
            {
                default:
                    {
                        int num;
                        switch (year)
                        {
                            case 2000:
                                empty = "двухтысячного";
                                break;
                            case 1900:
                                empty = "тысяча девятисотого";
                                break;
                            default:
                                {
                                    empty = ((!(year.ToString().Substring(0, 2) == "19")) ? (empty + "две тысячи ") : (empty + "тысяча девятьсот "));
                                    num = int.Parse(year.ToString().Substring(2, 2));
                                    switch (int.Parse(year.ToString().Substring(1, 1)))
                                    {
                                        case 1:
                                            empty += "сто ";
                                            break;
                                        case 2:
                                            empty += "двести ";
                                            break;
                                        case 3:
                                            empty += "триста ";
                                            break;
                                        case 4:
                                            empty += "четыреста ";
                                            break;
                                        case 5:
                                            empty += "пятьсот ";
                                            break;
                                        case 6:
                                            empty += "шестьсот ";
                                            break;
                                        case 7:
                                            empty += "семьсот ";
                                            break;
                                        case 8:
                                            empty += "восемьсот ";
                                            break;
                                        case 9:
                                            empty += "девятьсот ";
                                            break;
                                    }
                                    if (num >= 1 && num <= 19)
                                    {
                                        goto IL_030d;
                                    }
                                    if (num.ToString().Substring(1, 1) == "0")
                                    {
                                        goto IL_030d;
                                    }
                                    int num2 = int.Parse(year.ToString().Substring(2, 1));
                                    int num3 = int.Parse(year.ToString().Substring(3, 1));
                                    switch (num2)
                                    {
                                        case 2:
                                            empty += "двадцать ";
                                            break;
                                        case 3:
                                            empty += "тридцать ";
                                            break;
                                        case 4:
                                            empty += "сорок ";
                                            break;
                                        case 5:
                                            empty += "пятьдесят ";
                                            break;
                                        case 6:
                                            empty += "шестьдесят ";
                                            break;
                                        case 7:
                                            empty += "семьдесят ";
                                            break;
                                        case 8:
                                            empty += "восемьдесят ";
                                            break;
                                        case 9:
                                            empty += "девяносто ";
                                            break;
                                    }
                                    switch (num3)
                                    {
                                        case 1:
                                            empty += "первое";
                                            break;
                                        case 2:
                                            empty += "второе";
                                            break;
                                        case 3:
                                            empty += "третье";
                                            break;
                                        case 4:
                                            empty += "четвертое";
                                            break;
                                        case 5:
                                            empty += "пятое";
                                            break;
                                        case 6:
                                            empty += "шестое";
                                            break;
                                        case 7:
                                            empty += "седьмое";
                                            break;
                                        case 8:
                                            empty += "восьмое";
                                            break;
                                        case 9:
                                            empty += "девятое";
                                            break;
                                    }
                                    break;
                                }
                            IL_030d:
                                switch (num)
                                {
                                    case 1:
                                        empty += "первого";
                                        break;
                                    case 2:
                                        empty += "второго";
                                        break;
                                    case 3:
                                        empty += "третьего";
                                        break;
                                    case 4:
                                        empty += "четвертого";
                                        break;
                                    case 5:
                                        empty += "пятого";
                                        break;
                                    case 6:
                                        empty += "шестого";
                                        break;
                                    case 7:
                                        empty += "седьмого";
                                        break;
                                    case 8:
                                        empty += "восьмого";
                                        break;
                                    case 9:
                                        empty += "девятого";
                                        break;
                                    case 10:
                                        empty += "десятого";
                                        break;
                                    case 11:
                                        empty += "одиннадцатого";
                                        break;
                                    case 12:
                                        empty += "двенадцатого";
                                        break;
                                    case 13:
                                        empty += "тринадцатого";
                                        break;
                                    case 14:
                                        empty += "четырнадцатого";
                                        break;
                                    case 15:
                                        empty += "пятнадцатого";
                                        break;
                                    case 16:
                                        empty += "шестнадцатого";
                                        break;
                                    case 17:
                                        empty += "семнадцатого";
                                        break;
                                    case 18:
                                        empty += "восемнадцатого";
                                        break;
                                    case 19:
                                        empty += "девятнадцатого";
                                        break;
                                    case 20:
                                        empty += "двадцатого";
                                        break;
                                    case 30:
                                        empty += "тридцатого";
                                        break;
                                    case 40:
                                        empty += "сорокового";
                                        break;
                                    case 50:
                                        empty += "пятидесятого";
                                        break;
                                    case 60:
                                        empty += "шестидесятого";
                                        break;
                                    case 70:
                                        empty += "семидесятого";
                                        break;
                                    case 80:
                                        empty += "восьмидесятого";
                                        break;
                                    case 90:
                                        empty += "девяностого";
                                        break;
                                }
                                break;
                        }
                        return "«" + ((day < 10) ? "0" : string.Empty) + day.ToString() + "» " + text + " " + year.ToString();
                    }
            }
        }

        public static string GetDay(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            if (datestr.Trim().Length == 0)
            {
                return string.Empty;
            }
            DateTime dateTime = DateTime.Parse(datestr);
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.DateTimeFormat.ShortDatePattern = "d.M.yyyy";
            return dateTime.Day.ToString();
        }

        public static string GetMonth(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            if (datestr.Trim().Length == 0)
            {
                return string.Empty;
            }
            DateTime dateTime = DateTime.Parse(datestr);
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.DateTimeFormat.ShortDatePattern = "d.M.yyyy";
            return dateTime.Month.ToString();
        }

        public static string GetYear(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
            {
                return string.Empty;
            }
            if (datestr.Trim().Length == 0)
            {
                return string.Empty;
            }
            DateTime dateTime = DateTime.Parse(datestr);
            CultureInfo cultureInfo = new CultureInfo("ru-RU");
            cultureInfo.DateTimeFormat.ShortDatePattern = "d.M.yyyy";
            return dateTime.Year.ToString();
        }

        private static string AddEnding(string str, Padej padej, ref bool myagkiiZnak)
        {
            switch (padej)
            {
                case Padej.Imen:
                    str += (myagkiiZnak ? string.Empty : "ое");
                    break;
                case Padej.Rod:
                    str += (myagkiiZnak ? "го" : "ого");
                    break;
                case Padej.Dat:
                    str += (myagkiiZnak ? "му" : "ому");
                    break;
            }
            myagkiiZnak = false;
            return str;
        }

        public static string ToCrmDateTime(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return string.Empty;
            }
            CultureInfo provider = new CultureInfo("ru-RU");
            string str = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Parse(date, provider)).Hours.ToString("+00;-00;+00");
            return DateTime.Parse(date, provider).ToString("yyyy-MM-ddTHH:mm:ss" + str + ":00");
        }

        public static string FormatCrmDateTime(string crmDateTime, string format)
        {
            return FormatCrmDateTime(crmDateTime, 'T', '-', ':', format);
        }

        private static string FormatCrmDateTime(string crmDateTime, char dtSeparator, char dateSeparator, char timeSeparator, string format)
        {
            if (string.IsNullOrEmpty(crmDateTime))
            {
                return string.Empty;
            }
            string[] array = crmDateTime.Split(dtSeparator);
            string[] array2 = array[0].Split(dateSeparator);
            string[] array3 = (!string.IsNullOrEmpty(array[1])) ? array[1].Split(timeSeparator) : null;
            DateTime time = new DateTime(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]), Convert.ToInt32(array3[0]), Convert.ToInt32(array3[1]), 0);
            time = TimeZone.CurrentTimeZone.ToLocalTime(time);
            return time.ToString(format);
        }

        public static string FormatDate(string date, string format)
        {
            if (string.IsNullOrEmpty(date))
            {
                return string.Empty;
            }
            return DateTime.Parse(date).ToString(format);
        }

        public static string DeclinationOfNames(string lastname, string firstname, string middlename, Sex sex, Padej padej)
        {
            int num = (int)sex;
            DeclinationOfNames declinationOfNames = new DeclinationOfNames(lastname, firstname, middlename, num.ToString());
            return declinationOfNames.GetFio((DeclinationOfNames.Padej)padej);
        }

        public static string GetDollarStringWithCent(decimal value, Padej padeg)
        {
            var dollar_str = string.Empty;
            var dollar_name = string.Empty;
            var cent_str = string.Empty;
            var cent_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "доллар", "доллара", "долларов", "доллара", "долларов", "долларов", "доллару", "долларам", "долларам", "", "", "", padeg, ref dollar_str, ref dollar_name, ref cent_str, ref cent_name);

            cent_name = getPraseEnding(Convert.ToInt32(fract_value), "цент", "цента", "центов", "цента", "центов", "центов", "центу", "центам", "центам", padeg);

            return dollar_str + " " + dollar_name + " " + fract_value + " " + cent_name;
        }

        public static string GetDollarStringWithCentAndBrackets(decimal value, Padej padeg)
        {
            var dollar_str = string.Empty;
            var dollar_name = string.Empty;
            var cent_str = string.Empty;
            var cent_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "доллар", "доллара", "долларов", "доллара", "долларов", "долларов", "доллару", "долларам", "долларам", "", "", "", padeg, ref dollar_str, ref dollar_name, ref cent_str, ref cent_name);

            cent_name = getPraseEnding(Convert.ToInt32(fract_value), "цент", "цента", "центов", "цента", "центов", "центов", "центу", "центам", "центам", padeg);

            return "(" + dollar_str + ") " + dollar_name + " " + fract_value + " " + cent_name;
        }

        public static string GetEuroStringWithEurocent(decimal value, Padej padeg)
        {
            var evro_str = string.Empty;
            var evro_name = string.Empty;
            var cent_str = string.Empty;
            var cent_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "", "", "", padeg, ref evro_str, ref evro_name, ref cent_str, ref cent_name);

            cent_name = getPraseEnding(Convert.ToInt32(fract_value), "евроцент", "евроцента", "евроцентов", "евроцента", "евроцентов", "евроцентов", "евроценту", "евроцентам", "евроцентам", padeg);

            return evro_str + " " + evro_name + " " + fract_value + " " + cent_name;
        }

        public static string GetEuroStringWithEurocentAndBrackets(decimal value, Padej padeg)
        {
            var evro_str = string.Empty;
            var evro_name = string.Empty;
            var cent_str = string.Empty;
            var cent_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "евро", "", "", "", padeg, ref evro_str, ref evro_name, ref cent_str, ref cent_name);

            cent_name = getPraseEnding(Convert.ToInt32(fract_value), "евроцент", "евроцента", "евроцентов", "евроцента", "евроцентов", "евроцентов", "евроценту", "евроцентам", "евроцентам", padeg);

            return "(" + evro_str + ") " + evro_name + " " + fract_value + " " + cent_name;
        }


        public static string GetPoundsterlingStringWithPerry(decimal value, Padej padeg)
        {
            var pound_str = string.Empty;
            var pound_name = string.Empty;
            var perry_str = string.Empty;
            var perry_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "фунт стерлингов", "фунта стерлингов", "фунтов стерлингов", "фунта стерлингов", "фунтов стерлингов", "фунтов стерлингов", "фунту стерлингов", "фунтам стерлингов", "фунтам стерлингов", "", "", "", padeg, ref pound_str, ref pound_name, ref perry_str, ref perry_name);

            perry_name = getPraseEnding(Convert.ToInt32(fract_value), "пенс", "пенса", "пенсов", "пенса", "пенсов", "пенсов", "пенсу", "пенсам", "пенсам", padeg);

            return pound_str + " " + pound_name + " " + fract_value + " " + perry_name;
        }

        public static string GetPoundsterlingWithPerryAndBrackets(decimal value, Padej padeg)
        {
            var pound_str = string.Empty;
            var pound_name = string.Empty;
            var perry_str = string.Empty;
            var perry_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "фунт стерлингов", "фунта стерлингов", "фунтов стерлингов", "фунта стерлингов", "фунтов стерлингов", "фунтов стерлингов", "фунту стерлингов", "фунтам стерлингов", "фунтам стерлингов", "", "", "", padeg, ref pound_str, ref pound_name, ref perry_str, ref perry_name);

            perry_name = getPraseEnding(Convert.ToInt32(fract_value), "пенс", "пенса", "пенсов", "пенса", "пенсов", "пенсов", "пенсу", "пенсам", "пенсам", padeg);

            return "(" + pound_str + ") " + pound_name + " " + fract_value + " " + perry_name;
        }

        public static string GetYuanStringWithFyn(decimal value, Padej padeg)
        {
            var yuan_str = string.Empty;
            var yuan_name = string.Empty;
            var fyn_str = string.Empty;
            var fyn_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "юань", "юаня", "юаней", "юаня", "юаней", "юаней", "юаню", "юаням", "юаням", "", "", "", padeg, ref yuan_str, ref yuan_name, ref fyn_str, ref fyn_name);

            fyn_name = getPraseEnding(Convert.ToInt32(fract_value), "фынь", "фыня", "фыней", "фыня", "фыней", "фыней", "фыню", "фыням", "фыням", padeg);

            return yuan_str + " " + yuan_name + " " + fract_value + " " + fyn_name;
        }

        public static string GetYuanStringWithFynAndBrackets(decimal value, Padej padeg)
        {
            var yuan_str = string.Empty;
            var yuan_name = string.Empty;
            var fyn_str = string.Empty;
            var fyn_name = string.Empty;
            value = Math.Round(value, 2);
            var int_value = Math.Truncate(value);
            var fract_value = "00";
            if (value.ToString().Contains("."))
            {
                fract_value = value.ToString().Split('.')[1];
            }
            else if (value.ToString().Contains(","))
            {
                fract_value = value.ToString().Split(',')[1];
            }

            CurPhrase(int_value, "юань", "юаня", "юаней", "юаня", "юаней", "юаней", "юаню", "юаням", "юаням", "", "", "", padeg, ref yuan_str, ref yuan_name, ref fyn_str, ref fyn_name);

            fyn_name = getPraseEnding(Convert.ToInt32(fract_value), "фынь", "фыня", "фыней", "фыня", "фыней", "фыней", "фыню", "фыням", "фыням", padeg);

            return "(" + yuan_str + ") " + yuan_name + " " + fract_value + " " + fyn_name;
        }
    }
}
