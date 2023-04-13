using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Превью документа
    /// </summary>
    [DataContract]
    public class PreviewDocument
    {
        /// <summary>
        /// Список изображений страниц документа в формате base64
        /// </summary>
        [DataMember]
        public List<string> Data { get; set; }

        /// <summary>
        /// Список ошибок
        /// </summary>
        [DataMember]
        public List<string> Errors { get; set; }
    }
}