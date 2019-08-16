using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        public DashboardPage dashboardPage;

        public AdminProfile adminProfile;

        public Database database;

        public Admin admin;

        public AdministratorsPage administratorsPage;

        public ManageStudentsPage manageStudentPage;

        public ActivitiesPage activitiesPage;

        public ViewActivityPage viewActivityPage;

        public AttendancePage attendancePage;

        private int adminId;

        public AdminDashboard()
        {
            InitializeComponent();
        }

        public int CurrentAdminId
        {
            get { return adminId; }
            set { this.adminId = value; }
        }

        private void OpenSideMenu(object sender, RoutedEventArgs e)
        {
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                SideMenu.IsLeftDrawerOpen = true;
            }
            else
            {
                SideMenu.IsLeftDrawerOpen = false;
            }
        }

        private void navigateActivitiesPage(object sender, RoutedEventArgs e)
        {
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            pageLabel.Text = "Administrator > Dashboard > Activities";
            if (activitiesPage == null)
            {
                activitiesPage = new ActivitiesPage(this);
                PageContainer.Content = activitiesPage;
            } else
            {
                PageContainer.Content = activitiesPage;
            }
        }

        private void navigateAdministratorListPage(object sender, RoutedEventArgs e)
        {
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            pageLabel.Text = "Administrator > Dashboard > List of Administrators";
            if (administratorsPage == null)
            {
                administratorsPage = new AdministratorsPage(database);
                PageContainer.Content = administratorsPage;
            }
            else
            {
                PageContainer.Content = administratorsPage;
            }
        }

        private void navigateManageStudentPage(object sender, RoutedEventArgs e)
        {
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            pageLabel.Text = "Administrator > Dashboard > Manage Students";
            if (manageStudentPage == null)
            {
                manageStudentPage = new ManageStudentsPage(database);
                PageContainer.Content = manageStudentPage;
            } else
            {
                PageContainer.Content = manageStudentPage;
            }
        }

        private void navigateDashboardPage(object sender, RoutedEventArgs e)
        {
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            pageLabel.Text = "Administrator > Dashboard > Home";

            if (dashboardPage == null)
            {
                dashboardPage = new DashboardPage();
                dashboardPage.CurrentAdminDashboard = this;
                PageContainer.Content = dashboardPage;
            } else
            {
                PageContainer.Content = dashboardPage;
            }
        }

        private void navigateAttendancePage(object sender, RoutedEventArgs e)
        {
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            pageLabel.Text = "Administrator > Dashboard > Attendance";

            if (attendancePage == null)
            {
                attendancePage = new AttendancePage(database);
                PageContainer.Content = attendancePage;
            } else
            {
                PageContainer.Content = attendancePage;
            }
        }

        private void LoadAdminData()
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM admninistrators WHERE id = "+adminId+"", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            admin = new Admin(
                                database.MyReader.GetInt32("id"),
                                database.MyReader.GetString("firstname"),
                                database.MyReader.GetString("middlename"),
                                database.MyReader.GetString("lastname"),
                                database.MyReader.GetString("username"),
                                database.MyReader.GetString("position"),
                                database.MyReader.GetString("createdOn"),
                                database.MyReader.GetString("fingerprint")
                            );
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                } catch (Exception)
                {
                    ErrorHeader.Text = "Database Error";
                    ErrorContent.Text = "Error while fetching data from database. Please try again later, Thank you!";
                    ErrorDialog.IsOpen = true;
                }
                database.Close();
            } else
            {
                ErrorHeader.Text = "Database Error";
                ErrorContent.Text = "Error while connecting to database. Please try again later, Thank you!";
                ErrorDialog.IsOpen = true;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }

        private void Logout()
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void WindowsLoaded(object sender, RoutedEventArgs e)
        {
            database = new Database();
            LoadAdminData();

            dashboardPage = new DashboardPage();
            dashboardPage.CurrentAdminDashboard = this;
            PageContainer.Content = dashboardPage;

            usernameLabel.Text = "Howdy, " + admin.Username;
        }

        private void ViewMyProfile(object sender, RoutedEventArgs e)
        {
            pageLabel.Text = "Administrator > Dashboard > My Profile";
            if (adminProfile == null)
            {
                adminProfile = new AdminProfile();
                adminProfile.CurrentDatabase = this.database;
                adminProfile.adminDashboard = this;
                PageContainer.Content = adminProfile;
            } else
            {
                PageContainer.Content = adminProfile;
            }
        }
    }

    public class Admin
    {
        public int AdminId;
        public String Firstname;
        public String Middlename;
        public String Lastname;
        public String Username;
        public String Position;
        public String DateStarted;
        public String Fingerprint;

        public Admin(int AdminId, String Firstname, String Middlename, String Lastname, String Username, String Position, String DateStarted, String Fingerprint)
        {
            this.AdminId = AdminId;
            this.Firstname = Firstname;
            this.Lastname = Lastname;
            this.Middlename = Middlename;
            this.Username = Username;
            this.Position = Position;
            this.DateStarted = DateStarted;
            this.Fingerprint = Fingerprint;
        }
    }
}
