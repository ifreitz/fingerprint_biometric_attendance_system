using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for StudentDashboard.xaml
    /// </summary>
    public partial class StudentDashboard : Window
    {
        public Database database;

        private StudentProfilePage studenProfilePage;

        private StudentAttendancePage studentAttendancePage;

        public string StudentId;

        public string Fullname;

        public string Course;

        public string Sex;

        public string Year;

        public StudentDashboard(string StudentId, Database database)
        {
            InitializeComponent();

            this.StudentId = StudentId;
            this.database = database;
            GetStudentInformation();
            SetInformation();
        }

        private void SetInformation()
        {
            usernameLabel.Text = "Howdy, " + Fullname;
        }

        private void GetStudentInformation()
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT fullname, sex, course, year FROM students WHERE student_id = '"+StudentId+"'", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    while (database.MyReader.Read())
                    {
                        Fullname = database.MyReader.GetString("fullname");
                        Course = database.MyReader.GetString("course");
                        Sex = database.MyReader.GetString("sex");
                        Year = database.MyReader.GetInt32("year").ToString();
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                } catch (Exception e)
                {
                    MessageBox.Show("Cannot fetch data from database.\nPlease try again later.\n" + e, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logout();
                }
            } else
            {
                MessageBox.Show("Cannot connect to database.\nPlease try again later.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logout();
            }
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }

        private void NavigateStudentProfilePage(object sender, RoutedEventArgs e)
        {
            pageLabel.Text = "Student > Dashboard > Profile";
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            if (studenProfilePage == null)
            {
                studenProfilePage = new StudentProfilePage(this);
                PageContainer.Content = studenProfilePage;
            } else
            {
                PageContainer.Content = studenProfilePage;
            }
        }

        private void NavigateStudentAttendancePage(object sender, RoutedEventArgs e)
        {
            pageLabel.Text = "Student > Dashboard > Attendance";
            btn_sideMenu.IsChecked = false;
            SideMenu.IsLeftDrawerOpen = false;
            if (studentAttendancePage == null)
            {
                studentAttendancePage = new StudentAttendancePage();
                PageContainer.Content = studentAttendancePage;
            }
            else
            {
                PageContainer.Content = studentAttendancePage;
            }
        }

        private void WindowsWasLoaded(object sender, RoutedEventArgs e)
        {
            studenProfilePage = new StudentProfilePage(this);
            PageContainer.Content = studenProfilePage;
        }

        private void Logout()
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
