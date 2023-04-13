using System;
using System.Windows;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for DateInputDialog.xaml
    /// </summary>
    public partial class DateInputDialog : Window
    {
        public DateInputDialog()
        {
            InitializeComponent();
            DatePickerControl.SelectedDate = DateTime.Today;
        }

        public DateTime? SelectedData
        {
            get
            {
                return DatePickerControl.SelectedDate;
            }
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void RadButton_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
