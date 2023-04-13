using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Assets.Enums
{
    public enum SpLoadEnum
    {
        [Display(Name = "Загрузка в SP")]
        LoadInSp,

        [Display(Name = "Загрузка в существующий документ и SP")]
        LoadInDocAndSp,

        [Display(Name = "Создание документа и загрузка в SP")]
        CreateDocAndLoadSp
    }
}
