using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Assets.Enums
{
    public enum ChangeNameTypeEnum
    {
        [Display(Name = "Добавлять дату и время")]
        WithDateAndTime,

        [Display(Name = "Добавлять только дату")]
        WithDate,

        [Display(Name = "Не добавлять дату и время")]
        NoDateNoTime,    
    }
}
