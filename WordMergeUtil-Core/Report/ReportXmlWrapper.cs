using System.Collections.Generic;
using WordMergeEngine.Models;

namespace WordMergeUtil_Core.Report
{
    public class ReportXmlWrapper
    {
        public string ToolVersion;
        public List<XmlReport> XmlReports;
        public List<XmlDocument> XmlDocuments;
    }

    public class XmlReport
    {
        public WordMergeEngine.Models.Report Report;
        public List<DataSource> DataSources = new List<DataSource>();
        public List<Condition> Conditions = new List<Condition>();
        public List<Parameter> Parameters = new List<Parameter>();
    }

    public class XmlPackage
    {
        public ReportPackage Package;
        public List<XmlPackageEntry> Entries = new List<XmlPackageEntry>();
    }

    public class XmlPackageEntry
    {
        public ReportPackageEntry Entry;
        public List<Condition> Conditions = new List<Condition>();
    }

    public class ReportPackagesXml
    {
        public string ToolVersion;
        public List<XmlPackage> Packages = new List<XmlPackage>();
    }

    public class DataSourceXmlWrapper
    {
        public string ToolVersion;
        public List<XmlDataSource> XmlDataSources;
    }

    public class XmlDataSource
    {
        public List<DataSource> DataSources = new List<DataSource>();
    }

    public class ConditionXmlWrapper
    {
        public string ToolVersion;
        public List<XmlCondition> XmlConditions;
    }

    public class XmlCondition
    {
        public Condition Condition = new Condition();
    }

    public class ParameterXmlWrapper
    {
        public string ToolVersion;
        public List<XmlParameter> XmlParameters;
    }

    public class XmlParameter
    {
        public Parameter Parameter = new Parameter();
    }

    public class DocumentXmlWrapper
    {
        public string ToolVersion;
        public List<XmlDocument> XmlDocuments;
    }

    public class XmlDocument
    {
        public Document Document;
        public List<DocumentContent> DocumentContents = new List<DocumentContent>();
        public List<Paragraph> Paragraphs = new List<Paragraph>();
        public List<ParagraphContent> ParagraphContents = new List<ParagraphContent>();
    }

    public class FilterXmlWrapper
    {
        public string ToolVersion;
        public List<XmlFilter> XmlFilters;
    }

    public class XmlFilter
    {
        public Filter Filter = new Filter();
    }
}
