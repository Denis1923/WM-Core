using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Информация о фильтре
    /// </summary>
    [DataContract]
    public class Filter
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// ID родительского фильтра
        /// </summary>
        [DataMember]
        public Guid? ParentFilterId { get; set; }

        /// <summary>
        /// Название фильтра
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Отображаемое название фильтра
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Тип фильтра
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Список параметров, полученных из запроса
        /// </summary>
        [DataMember]
        public LabelValueEntry[] List { get; set; }

        /// <summary>
        /// Значение фильтра
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Знак сравнения родительских значений
        /// </summary>
        [DataMember]
        public string ParentOperator { get; set; }

        /// <summary>
        /// Возможные значения родительского фильтра
        /// </summary>
        [DataMember]
        public List<string> ParentValue { get; set; } = new List<string>();

        /// <summary>
        /// Порядок отображения
        /// </summary>
        [DataMember]
        public decimal Order { get; set; }
    }
}