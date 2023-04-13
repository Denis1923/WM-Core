using System.Text.RegularExpressions;

namespace DataByWords.DataByWords
{
    internal class DeclinationOfNames
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

        public enum Sex
        {
            Male = 1,
            Female
        }

        private string surname;

        private string name;

        private string otchestvo;

        private string pol;

        private string fam_r;

        private string nam_r;

        private string otch_r;

        private string fam_d;

        private string nam_d;

        private string otch_d;

        private string fam_t;

        private string nam_t;

        private string otch_t;

        private string fam_v;

        private string nam_v;

        private string otch_v;

        private string fam_p;

        private string nam_p;

        private string otch_p;

        public string GetFio(Padej padeg)
        {
            DoDeclination();
            switch (padeg)
            {
                case Padej.Rod:
                    return GetRP();
                case Padej.Dat:
                    return GetDP();
                case Padej.Vin:
                    return GetVP();
                case Padej.Tvor:
                    return GetTP();
                case Padej.Pred:
                    return GetPP();
                default:
                    return surname + " " + name + " " + otchestvo;
            }
        }

        public DeclinationOfNames(string l, string n, string o)
        {
            surname = l;
            name = n;
            otchestvo = o;
            pol = "";
        }

        public DeclinationOfNames(string l, string n, string o, string sex)
        {
            surname = l;
            name = n;
            otchestvo = o;
            pol = sex;
        }

        private string GetRP()
        {
            return fam_r + " " + nam_r + ((!string.IsNullOrEmpty(otchestvo)) ? (" " + otch_r) : "");
        }

        private string GetDP()
        {
            return fam_d + " " + nam_d + ((!string.IsNullOrEmpty(otchestvo)) ? (" " + otch_d) : "");
        }

        private string GetVP()
        {
            return fam_v + " " + nam_v + ((!string.IsNullOrEmpty(otchestvo)) ? (" " + otch_v) : "");
        }

        private string GetTP()
        {
            return fam_t + " " + nam_t + ((!string.IsNullOrEmpty(otchestvo)) ? (" " + otch_t) : "");
        }

        private string GetPP()
        {
            return fam_p + " " + nam_p + ((!string.IsNullOrEmpty(otchestvo)) ? (" " + otch_p) : "");
        }

        private void Init()
        {
            fam_r = "";
            nam_r = "";
            otch_r = "";
            fam_d = "";
            nam_d = "";
            otch_d = "";
            fam_t = "";
            nam_t = "";
            otch_t = "";
            fam_v = "";
            nam_v = "";
            otch_v = "";
            fam_p = "";
            nam_p = "";
            otch_p = "";
        }

        public void DoDeclination()
        {
            string a = name;
            Init();
            string[] array;
            string[] array2;
            bool flag;
            string a2;
            if (!string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(pol))
                {
                    if (otchestvo.Length > 2)
                    {
                        pol = ((otchestvo.Substring(otchestvo.Length - 3) == "ич") ? "1" : "2");
                    }
                    else
                    {
                        pol = "1";
                    }
                }
                if (name == "Павел")
                {
                    name = "Павл";
                }
                array = new string[10]
                {
                    "а",
                    "е",
                    "ё",
                    "и",
                    "о",
                    "у",
                    "ы",
                    "э",
                    "ю",
                    "я"
                };
                array2 = new string[10]
                {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8",
                    "9",
                    "0"
                };
                a2 = "false";
                Regex regex = new Regex("[A-z]");
                flag = false;
                if (regex.IsMatch(surname) && !string.IsNullOrEmpty(surname))
                {
                    goto IL_01be;
                }
                if (regex.IsMatch(name) && !string.IsNullOrEmpty(name))
                {
                    goto IL_01be;
                }
                if (regex.IsMatch(otchestvo) && !string.IsNullOrEmpty(otchestvo))
                {
                    goto IL_01be;
                }
                goto IL_01c1;
            }
            return;
        IL_0460:
            string a3;
            if (a2 == "true" && surname.LastIndexOf("ая") == surname.Length - 2)
            {
                fam_r = surname.Substring(0, surname.Length - 2) + "ой";
                fam_d = surname.Substring(0, surname.Length - 2) + "ой";
                fam_t = surname.Substring(0, surname.Length - 2) + "ой";
                fam_v = surname.Substring(0, surname.Length - 2) + "ую";
                fam_p = surname.Substring(0, surname.Length - 2) + "ой";
            }
            else if (a2 == "false" && surname.LastIndexOf("ч") == surname.Length - 1)
            {
                fam_r = surname + "а";
                fam_d = surname + "у";
                fam_t = surname + "ем";
                fam_v = surname + "а";
                fam_p = surname + "е";
            }
            else if (a2 == "false" && surname.LastIndexOf("ий") == surname.Length - 2)
            {
                fam_r = surname.Substring(0, surname.Length - 2) + "ого";
                fam_d = surname.Substring(0, surname.Length - 2) + "ому";
                fam_t = surname.Substring(0, surname.Length - 2) + "им";
                fam_v = surname.Substring(0, surname.Length - 2) + "ого";
                fam_p = surname.Substring(0, surname.Length - 2) + "ом";
            }
            else if (a2 == "false" && (surname.LastIndexOf("ый") == surname.Length - 2 || surname.LastIndexOf("ой") == surname.Length - 2))
            {
                fam_r = surname.Substring(0, surname.Length - 2) + "ого";
                fam_d = surname.Substring(0, surname.Length - 2) + "ому";
                fam_t = surname.Substring(0, surname.Length - 2) + "ым";
                fam_v = surname.Substring(0, surname.Length - 2) + "ого";
                fam_p = surname.Substring(0, surname.Length - 2) + "ом";
            }
            else if (a2 == "false" && (surname.LastIndexOf("ов") == surname.Length - 2 || surname.LastIndexOf("ин") == surname.Length - 2 || surname.LastIndexOf("ев") == surname.Length - 2))
            {
                fam_r = surname + "а";
                fam_d = surname + "у";
                fam_t = surname + "ым";
                fam_v = surname + "а";
                fam_p = surname + "е";
            }
            else if (a2 == "false" && surname.LastIndexOf("ч") != surname.Length - 1 && surname.LastIndexOf("ий") != surname.Length - 2 && surname.LastIndexOf("ый") != surname.Length - 2 && surname.LastIndexOf("ой") != surname.Length - 2 && surname.LastIndexOf("ь") != surname.Length - 1 && surname.LastIndexOf("их") != surname.Length - 2 && surname.LastIndexOf("ых") != surname.Length - 2 && surname.LastIndexOf("ов") != surname.Length - 2 && surname.LastIndexOf("ин") != surname.Length - 2 && surname.LastIndexOf("ев") != surname.Length - 2 && a3 == "false")
            {
                fam_r = surname + "а";
                fam_d = surname + "у";
                fam_t = surname + "ом";
                fam_v = surname + "а";
                fam_p = surname + "е";
            }
            goto IL_0b2e;
        IL_01c1:
            if (surname != null && surname.Length > 2 && !flag)
            {
                int num = 0;
                while (num <= 9)
                {
                    if (surname.LastIndexOf(array[num]) != surname.Length - 1)
                    {
                        num++;
                        continue;
                    }
                    a2 = "true";
                    break;
                }
                a3 = "false";
                int num2 = 0;
                while (num2 <= 9)
                {
                    if (surname.LastIndexOf(array2[num2]) != surname.Length - 1)
                    {
                        num2++;
                        continue;
                    }
                    a3 = "true";
                    break;
                }
                if ((!(a2 == "true") || surname.LastIndexOf("а") == surname.Length - 1 || surname.LastIndexOf("ая") == surname.Length - 2) && !(a3 == "true") && (!(a2 == "false") || (surname.LastIndexOf("ь") != surname.Length - 1 && surname.LastIndexOf("их") != surname.Length - 2 && surname.LastIndexOf("ых") != surname.Length - 2)))
                {
                    if (a2 == "true" && surname.LastIndexOf("а") == surname.Length - 1)
                    {
                        fam_r = surname.Substring(0, surname.Length - 1) + "ой";
                        fam_d = surname.Substring(0, surname.Length - 1) + "ой";
                        fam_t = surname.Substring(0, surname.Length - 1) + "ой";
                        fam_v = surname.Substring(0, surname.Length - 1) + "у";
                        fam_p = surname.Substring(0, surname.Length - 1) + "ой";
                    }
                    goto IL_0460;
                }
                fam_r = surname;
                fam_d = surname;
                fam_t = surname;
                fam_v = surname;
                fam_p = surname;
                goto IL_0460;
            }
            fam_r = surname;
            fam_d = surname;
            fam_t = surname;
            fam_v = surname;
            fam_p = surname;
            goto IL_0b2e;
        IL_01be:
            flag = true;
            goto IL_01c1;
        IL_13a9:
            if (a2 == "false" && name.LastIndexOf("ий") == name.Length - 2)
            {
                nam_p = nam_p.Substring(0, name.Length - 1) + "и";
            }
            goto IL_143f;
        IL_0f6b:
            nam_r = name;
            nam_d = name;
            nam_t = name;
            nam_v = name;
            nam_p = name;
            goto IL_13a9;
        IL_0b2e:
            if (name.Length > 2 && !flag)
            {
                a2 = "false";
                int num3 = 0;
                while (num3 <= 9)
                {
                    if (name.LastIndexOf(array[num3]) != name.Length - 1)
                    {
                        num3++;
                        continue;
                    }
                    a2 = "true";
                    break;
                }
                string a4 = "false";
                int num4 = 0;
                while (num4 <= 9)
                {
                    if (name.LastIndexOf(array2[num4]) != name.Length - 1)
                    {
                        num4++;
                        continue;
                    }
                    a4 = "true";
                    break;
                }
                if (a2 == "true" && name.LastIndexOf("а") == name.Length - 1 && name.LastIndexOf("ка") != name.Length - 2)
                {
                    nam_r = name.Substring(0, name.Length - 1) + "ы";
                    nam_d = name.Substring(0, name.Length - 1) + "е";
                    nam_t = name.Substring(0, name.Length - 1) + "ой";
                    nam_v = name.Substring(0, name.Length - 1) + "у";
                    nam_p = name.Substring(0, name.Length - 1) + "е";
                }
                else if (a2 == "true" && name.LastIndexOf("ка") == name.Length - 2)
                {
                    nam_r = name.Substring(0, name.Length - 1) + "и";
                    nam_d = name.Substring(0, name.Length - 1) + "е";
                    nam_t = name.Substring(0, name.Length - 1) + "ой";
                    nam_v = name.Substring(0, name.Length - 1) + "у";
                    nam_p = name.Substring(0, name.Length - 1) + "е";
                }
                else if (a2 == "true" && name.LastIndexOf("я") == name.Length - 1 && name.LastIndexOf("ия") != name.Length - 2)
                {
                    nam_r = name.Substring(0, name.Length - 1) + "и";
                    nam_d = name.Substring(0, name.Length - 1) + "е";
                    nam_t = name.Substring(0, name.Length - 1) + "ей";
                    nam_v = name.Substring(0, name.Length - 1) + "ю";
                    nam_p = name.Substring(0, name.Length - 1) + "ие";
                }
                else
                {
                    if (a2 == "true" && name.LastIndexOf("а") != name.Length - 1 && name.LastIndexOf("я") != name.Length - 1)
                    {
                        goto IL_0f6b;
                    }
                    if (a4 == "true")
                    {
                        goto IL_0f6b;
                    }
                    if (a2 == "false" && (name.LastIndexOf("й") == name.Length - 1 || name.LastIndexOf("рь") == name.Length - 2))
                    {
                        nam_r = name.Substring(0, name.Length - 1) + "я";
                        nam_d = name.Substring(0, name.Length - 1) + "ю";
                        nam_t = name.Substring(0, name.Length - 1) + "ем";
                        nam_v = name.Substring(0, name.Length - 1) + "я";
                        nam_p = name.Substring(0, name.Length - 1) + "е";
                    }
                    else if (a2 == "false" && name.LastIndexOf("ь") == name.Length - 1 && name.LastIndexOf("рь") != name.Length - 2)
                    {
                        nam_r = name.Substring(0, name.Length - 1) + "и";
                        nam_d = name.Substring(0, name.Length - 1) + "и";
                        nam_t = name.Substring(0, name.Length - 1) + "ей";
                        nam_v = name;
                        nam_p = name.Substring(0, name.Length - 1) + "е";
                    }
                    else if (a2 == "true" && name.LastIndexOf("ия") == name.Length - 2)
                    {
                        nam_r = name.Substring(0, name.Length - 1) + "и";
                        nam_d = name.Substring(0, name.Length - 1) + "и";
                        nam_t = name.Substring(0, name.Length - 1) + "ей";
                        nam_v = name.Substring(0, name.Length - 1) + "ю";
                        nam_p = name.Substring(0, name.Length - 1) + "и";
                    }
                    else if (a2 == "false" && name.LastIndexOf("ь") != name.Length - 1 && name.LastIndexOf("й") != name.Length - 1 && a4 == "false")
                    {
                        nam_r = name + "а";
                        nam_d = name + "у";
                        nam_t = name + "ом";
                        nam_v = name + "а";
                        nam_p = name + "е";
                    }
                }
                goto IL_13a9;
            }
            nam_r = name;
            nam_d = name;
            nam_t = name;
            nam_v = name;
            nam_p = name;
            goto IL_143f;
        IL_143f:
            if (otchestvo.Length <= 2 || flag)
            {
                otch_r = otchestvo;
                otch_d = otchestvo;
                otch_t = otchestvo;
                otch_v = otchestvo;
                otch_p = otchestvo;
            }
            else if (otchestvo.LastIndexOf("на") == otchestvo.Length - 2)
            {
                otch_r = otchestvo.Substring(0, otchestvo.Length - 1) + "ы";
                otch_d = otchestvo.Substring(0, otchestvo.Length - 1) + "е";
                otch_t = otchestvo.Substring(0, otchestvo.Length - 1) + "ой";
                otch_v = otchestvo.Substring(0, otchestvo.Length - 1) + "у";
                otch_p = otchestvo.Substring(0, otchestvo.Length - 1) + "е";
            }
            else if (otchestvo.LastIndexOf("ич") == otchestvo.Length - 2)
            {
                otch_r = otchestvo + "а";
                otch_d = otchestvo + "у";
                otch_t = otchestvo + "ем";
                otch_v = otchestvo + "а";
                otch_p = otchestvo + "е";
            }
            else
            {
                otch_r = otchestvo;
                otch_d = otchestvo;
                otch_t = otchestvo;
                otch_v = otchestvo;
                otch_p = otchestvo;
            }
            if (pol != null && surname != null && pol == "2")
            {
                a2 = "false";
                int num5 = 0;
                while (num5 <= 9)
                {
                    if (surname.LastIndexOf(array[num5]) != surname.Length - 1)
                    {
                        num5++;
                        continue;
                    }
                    a2 = "true";
                    break;
                }
                if (a2 == "false")
                {
                    fam_r = surname;
                    fam_d = surname;
                    fam_t = surname;
                    fam_v = surname;
                    fam_p = surname;
                }
            }
            if (a == "Ольга")
            {
                nam_r = "Ольги";
            }
        }
    }
}
