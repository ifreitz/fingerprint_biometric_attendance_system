using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for AdminProfile.xaml
    /// </summary>
    public partial class AdminProfile : Page
    {
        Database database;

        AdminDashboard _adminDashboard;

        static string appFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        string resourcePath = System.IO.Path.Combine(Directory.GetParent(appFolderPath).Parent.FullName, "Resources");

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

            if (File.Exists(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".png")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".png")));
            }
            else if (File.Exists(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".jpg")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".jpg")));
            }
            else if (File.Exists(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".jpeg")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + ".jpeg")));
            }
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

        private void BrowseImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select Picure";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                string imagePath = System.IO.Path.Combine(resourcePath, "admin_profile_" + _adminDashboard.admin.AdminId + System.IO.Path.GetExtension(op.SafeFileName));

                if (!Directory.Exists(resourcePath))
                {
                    Directory.CreateDirectory(resourcePath);
                }

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }

                File.Copy(op.FileName, imagePath);
                Image_Profile.Source = new BitmapImage(new Uri(op.FileName));
            }
        }
    }
}
