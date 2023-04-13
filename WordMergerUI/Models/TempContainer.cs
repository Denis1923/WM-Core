namespace WordMergerUI.Models
{
    public class TempContainer
    {
        public string Name { get; set; }

        public Report Report { get; set; }

        public ReportPackage Package { get; set; }

        public string ID { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool IsChooseFormat { get; set; } = false;
    }
}
