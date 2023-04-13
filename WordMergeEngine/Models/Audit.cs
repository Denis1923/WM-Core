using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Audit : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? ReportCode { get; set; }

    public string? Template { get; set; }

    public Guid? RowId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public Guid? CreatedBy { get; set; }

    public string? Parameters { get; set; }

    public string? DocumentLink { get; set; }

    public string? BarCode { get; set; }

    public string? IntegrationDocumentId { get; set; }

    public string? IntegrationDocumentLocation { get; set; }

    public Guid? IntegrationExternalId { get; set; }

    public string? SourceDataSet { get; set; }
}
