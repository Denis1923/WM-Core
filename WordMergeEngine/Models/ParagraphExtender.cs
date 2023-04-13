using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WordMergeEngine.Models
{
    public partial class Paragraph : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotMapped]
        public bool PassConditions { get; set; }

        [NotMapped]
        public string Errors { get; set; }

        public string DisplayName
        {
            get
            {
                var s = Name;

                for (int i = 0; i < this.Level; i++)
                    s = $"    {s}";

                if (!string.IsNullOrEmpty(Condition)) s += "◘";
                return s;
            }
        }

        public string FilterExpression
        {
            get
            {
                var s = string.Empty;
                if (ActiveFrom != null)
                {
                    s += $"Действует с {ActiveFrom.Value.ToShortDateString()}";
                    if (ActiveTill != null)
                    {
                        s += $" по {ActiveTill.Value.ToShortDateString()}";
                    }
                }
                else if (ActiveTill != null)
                {
                    s += $"Действует по {ActiveTill.Value.ToShortDateString()}";
                }
                return s;
            }
        }

        public string RelativeNo
        {
            get
            {
                if (PassConditions == false) return string.Empty;

                if (Numerable != true) return string.Empty;

                if (IsCustomNumbering) return CustomNo;

                var levels = new int[9];

                for (int i = 1; i < 9; i++) levels[i] = 0;

                var prev_level = 0;

                var list = DocumentContent.Paragraphs.Where(p => p.OrderNo <= OrderNo && p.Numerable == true && (p.IsFixNumeration || p.PassConditions == true) && p.Deleted == false && !p.IsCustomNumbering).OrderBy(p => p.OrderNo);
                foreach (var pp in list)
                {
                    if (prev_level == pp.Level)
                    {
                        levels[pp.Level.GetValueOrDefault()] += 1;
                    }
                    else
                    {
                        if (pp.Level > prev_level)
                        {
                            for (int i = prev_level + 1; i < 9; i++)
                            {
                                levels[i] = 1;
                            }
                        }
                        else
                            levels[pp.Level.GetValueOrDefault()] += 1;
                        prev_level = pp.Level.GetValueOrDefault();
                    }
                }

                if (levels[0] == 0)
                    levels[0] = 1;

                if (Level >= 0 && Level < 9)
                {
                    return string.Join(".", levels.Select(x => x.ToString()).Take(Level.Value + 1));
                }

                return string.Empty;
            }
        }

        public string GlobalPostFix
        {
            get
            {
                return ParentParagraph != null ? "(G)" : string.Empty;
            }
        }
    }
}
