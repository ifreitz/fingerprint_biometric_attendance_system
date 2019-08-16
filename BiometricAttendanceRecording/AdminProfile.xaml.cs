using System;
using System.Windows;
using System.Windows.Controls;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for AdminProfile.xaml
    /// </summary>
    public partial class AdminProfile : Page
    {
        Database database;

        AdminDashboard _adminDashboard;

        public AdminProfile()
        {
            InitializeComponent();
        }

        public AdminDashboard adminDashboard
        {
            get { return this._adminDashboard; }
            set { this._adminDashboard = value; }
        }

        public Database CurrentDatabase
        {
            get { return this.database; }
            set { this.database = value; }
        }

        public void PopulateInformation(String lastname, String firstname, String middlename, String username, String position, String dateCreated)
        {
            FirstName.Text = firstname;
            LastName.Text = lastname;
            MiddleName.Text = middlename;
            Username.Text = username;
            Position.Text = position;
            CreatedOn.Text = dateCreated;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            PopulateInformation(
                _adminDashboard.admin.Lastname,
                _adminDashboard.admin.Firstname,
                _adminDashboard.admin.Middlename,
                _adminDashboard.admin.Username,
                _adminDashboard.admin.Position,
                _adminDashboard.admin.DateStarted
            );
        }

        private void UpdateProfile(object sender, RoutedEventArgs e)
        {
            if (database.Connect())
            {
                if (database.Update("UPDATE `admninistrators` SET `firstname`='"+FirstName.Text+"',`middlename`='"+MiddleName.Text+"',`lastname`='"+LastName.Text+"',`username`='"+Username.Text+"',`position`='"+Position.Text+"' WHERE id = "+_adminDashboard.admin.AdminId+""))
                {
                    Badged_Update.Badge = "Updated";
                } else
                {
                    Badged_Update.Badge = "Failed";
                }
                database.Close();
            } else
            {
                Badged_Update.Badge = "Failed";
            }
        }
    }
}
