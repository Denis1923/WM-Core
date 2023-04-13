using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WordMergeEngine.Models
{
    public partial class Audit
    {
        [NotMapped]
        public Dictionary<string, string> ParametersToDictionary
        {
            get => string.IsNullOrEmpty(Parameters) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(Parameters);
            set
            {
                if (value != null)
                    Parameters = JsonConvert.SerializeObject(value);
            }
        }

        [NotMapped]
        public DataSet DataSetFromDB
        {
            get => string.IsNullOrEmpty(SourceDataSet) ? null : JsonConvert.DeserializeObject<DataSet>(SourceDataSet);
            set
            {
                if (value != null)
                    SourceDataSet = JsonConvert.SerializeObject(value);
            }
        }
    }
}
