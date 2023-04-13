using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Assets.Enums
{
    public enum OperatorEnum
    {
        [Display(Name = "=")]
        Equal = 0,

        [Display(Name = "!=")]
        NotEqual = 1
    }
}
