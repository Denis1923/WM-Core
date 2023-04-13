using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Assets.Enums
{
    public enum GroupTypeEnum
    {
        [Display(Name = "И")]
        And = 0,

        [Display(Name = "Или")]
        Or = 1
    }
}
