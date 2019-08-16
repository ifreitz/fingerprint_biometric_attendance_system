using System.Windows.Controls;
using System.Windows.Input;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public AdminDashboard adminDashboard;
        
        public DashboardPage()
        {
            InitializeComponent();
        }

        public AdminDashboard CurrentAdminDashboard
        {
            set { adminDashboard = value; }
        }
        
        private void NavigateActivitiesPage(object sender, MouseButtonEventArgs e)
        {
            adminDashboard.pageLabel.Text = "Administrator > Dashboard > Activities";
            if (adminDashboard.activitiesPage == null)
            {
                adminDashboard.activitiesPage = new ActivitiesPage(adminDashboard);
                adminDashboard.PageContainer.Content = adminDashboard.activitiesPage;
            } else
            {
                adminDashboard.PageContainer.Content = adminDashboard.activitiesPage;
            }
        }

        private void NavigateStudentListPage(object sender, MouseButtonEventArgs e)
        {
            adminDashboard.pageLabel.Text = "Administrator > Dashboard > Manage Students";
            if (adminDashboard.manageStudentPage == null)
            {
                adminDashboard.manageStudentPage = new ManageStudentsPage(adminDashboard.database);
                adminDashboard.PageContainer.Content = adminDashboard.manageStudentPage;
            } else
            {
                adminDashboard.PageContainer.Content = adminDashboard.manageStudentPage;
            }
        }

        private void NavigateAdministratorsPage(object sender, MouseButtonEventArgs e)
        {
            adminDashboard.pageLabel.Text = "Administrator > Dashboard > List of Administrators";
            if (adminDashboard.administratorsPage == null)
            {
                adminDashboard.administratorsPage = new AdministratorsPage(adminDashboard.database);
                adminDashboard.PageContainer.Content = adminDashboard.administratorsPage;
            } else
            {
                adminDashboard.PageContainer.Content = adminDashboard.administratorsPage;
            }
        }

        private void NavigateAttendancePage(object sender, MouseButtonEventArgs e)
        {
            adminDashboard.pageLabel.Text = "Administrator > Dashboard > Attendance";
            if (adminDashboard.attendancePage == null)
            {
                adminDashboard.attendancePage = new AttendancePage();
                adminDashboard.PageContainer.Content = adminDashboard.attendancePage;
            } else
            {
                adminDashboard.PageContainer.Content = adminDashboard.attendancePage;
            }
        }
    }
}
