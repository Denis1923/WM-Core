using System;
using System.Linq;

namespace WordMergeService.Helpers
{
    public class SharepointUploader
    {
        public void PutToSharepoint(PrintableDocument Doc, string reportcode, string groupKey, string sharepointcode, Guid BaseEntityId)
        {
            var multiService = new MultiselectUploaderServiceClient();

            try
            {
                var result = multiService.GetDocumentTypes(groupKey);

                if (result.IsErrors)
                    throw new Exception($"Ошибка при получении типов документов для загрузки документа в Sharepoint:\n{result.Message}");

                var documentTypes = result.RezultObject;

                if (documentTypes == null)
                    throw new Exception($"Не удалось получить типы документов. GroupKey \"{groupKey}\", {multiService.Endpoint.Address.Uri.AbsoluteUri}");

                var documentType = documentTypes.Where(i => i.SharepointCode == sharepointcode);

                if (!documentType.Any())
                    throw new Exception($"Не найден тип документа для загрузки с кодом \"{sharepointcode}\"");

                if (documentType.Count() > 1)
                    throw new Exception($"Найдено несколько типов документов для загрузки с кодом \"{sharepointcode}\". Количество: \"{documentType.Count()}\"");

                var documentTypeId = documentType.FirstOrDefault().Id;

                var res = multiService.UploadFile(Doc.Data, reportcode, documentTypeId, Doc.FileName, BaseEntityId.ToString(), groupKey, true);

                if (res.IsErrors)
                    throw new Exception(res.Message);
            }
            catch (Exception ex)
            {
                ProtocolWriter.LogError(ex);
                throw new Exception("Во время загрузки документа в Sharepoint произошла ошибка. Описание ошибки находится в логах.");
            }
        }
    }
}