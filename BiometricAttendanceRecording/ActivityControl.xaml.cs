using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for ActivityControl.xaml
    /// </summary>
    public partial class ActivityControl : UserControl
    {
        public ActivitiesPage activityPage;
        
        private int Id;

        private string ActivityName;

        private string Description;

        private string Date;

        private string Time;

        public ActivityControl(int Id, string ActivityName, string Description, string Date, string Time, ActivitiesPage activityPage)
        {
            InitializeComponent();

            this.Id = Id;
            this.ActivityName = ActivityName;
            this.Description = Description;
            this.Date = Date;
            this.Time = Time;
            this.activityPage = activityPage;
            InitializeInformation();
        }

        private void InitializeInformation()
        {
            TxtBlock_Name.Text = ActivityName;
            TxtBlock_Description.Text = Description;
            TxtBlock_Date.Text = Date + " " + Time;
        }

        private void NavigateViewActivity(object sender, RoutedEventArgs e)
        {
            SnackBar_Confirm.IsActive = false;
            activityPage.adminDashboard.pageLabel.Text = "Administrator > Dashboard > Activities > " + ActivityName;
            if (activityPage.adminDashboard.viewActivityPage == null)
            {
                activityPage.adminDashboard.viewActivityPage = new ViewActivityPage(activityPage.database);
                activityPage.adminDashboard.PageContainer.Content = activityPage.adminDashboard.viewActivityPage;
                activityPage.adminDashboard.viewActivityPage.PopulateInformation(this.Id, this.ActivityName, this.Description, this.Date, this.Time);
                activityPage.adminDashboard.viewActivityPage.CurrentActivityControl = this;
            } else
            {
                activityPage.adminDashboard.PageContainer.Content = activityPage.adminDashboard.viewActivityPage;
                activityPage.adminDashboard.viewActivityPage.PopulateInformation(this.Id, this.ActivityName, this.Description, this.Date, this.Time);
                activityPage.adminDashboard.viewActivityPage.CurrentActivityControl = this;
            }
        }

        private void DeleteThisActivity(object sender, RoutedEventArgs e)
        {
            SnackBar_Confirm.IsActive = true;
        }

        private void CancelDeletion(object sender, RoutedEventArgs e)
        {
            SnackBar_Confirm.IsActive = false;
        }

        private void ConfirmDeletion(object sender, RoutedEventArgs e)
        {
            if (activityPage.database.Connect())
            {
                if (activityPage.database.Delete("DELETE FROM activities WHERE id = "+Id+""))
                {
                    activityPage.Wrap_Activies.Children.Remove(this);
                } else
                {
                    MessageBox.Show("Unable to delete activity!\nPlease try again later.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                MessageBox.Show("Cannot connect to database!\nPlease try again later.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
