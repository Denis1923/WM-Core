using Microsoft.AspNetCore.Mvc;
using WordMergerUI.Helpers;

namespace WordMergerUI.Controllers
{
    public class ImportController : Controller
    {
        private string IntegrationCode
        {
            get
            {
                if (Session["IntegrationCode"] == null)
                    return string.Empty;

                return (string)Session["IntegrationCode"];
            }
            set
            {
                Session["IntegrationCode"] = value;
            }
        }

        public ActionResult Index(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new ApplicationException("Отсутствует значение поля 'Код интеграции документа'");

                IntegrationCode = code;
            }
            catch (ApplicationException ex)
            {
                ViewBag.errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.closeWindow = "True";
                ViewBag.errorMessage = $"Необработанное исключение: {ex.Message}";
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult Import()
        {
            ViewBag.closeWindow = "False";

            try
            {
                if (string.IsNullOrEmpty(IntegrationCode))
                    throw new ApplicationException("Отсутствует значение поля 'Код интеграции документа'");

                if (Request.Files == null || Request.Files.Count == 0)
                    throw new ApplicationException("Не выбран файл");

                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileContent = new byte[0];

                    using (var br = new BinaryReader(file.InputStream))
                        fileContent = br.ReadBytes((int)file.InputStream.Length);

                    var result = new WordMergerHelper(null).UploadDocument(IntegrationCode, fileContent, file.FileName);

                    if (!result.Success)
                        throw new ApplicationException(result.Message);

                    ViewBag.closeWindow = "True";
                    ViewBag.result = result.Message;
                }
                else
                    throw new ApplicationException("Загружаемый файл пуст");
            }
            catch (ApplicationException ex)
            {
                ViewBag.errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.closeWindow = "True";
                ViewBag.errorMessage = $"Необработанное исключение: {ex.Message}";
            }

            return View("Index");
        }
    }
}
