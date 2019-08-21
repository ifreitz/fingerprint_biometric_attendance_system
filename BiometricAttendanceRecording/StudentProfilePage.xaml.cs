using Microsoft.Win32;
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
using System.IO;
using System.Reflection;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for StudentProfilePage.xaml
    /// </summary>
    public partial class StudentProfilePage : Page
    {
        private StudentDashboard studentDashboard;

        static string appFolderPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        string resourcePath = System.IO.Path.Combine(Directory.GetParent(appFolderPath).Parent.FullName, "Resources");

        public StudentProfilePage(StudentDashboard studentDashboard)
        {
            InitializeComponent();
            this.studentDashboard = studentDashboard;
            PopulateAvailabelCourses();
            PopulateYearAndSex();
            SetInformation();
        }

        private void PopulateYearAndSex()
        {
            Year.Items.Add(new TextBlock().Text = "1");
            Year.Items.Add(new TextBlock().Text = "2");
            Year.Items.Add(new TextBlock().Text = "3");
            Year.Items.Add(new TextBlock().Text = "4");
            Sex.Items.Add(new TextBlock().Text = "M");
            Sex.Items.Add(new TextBlock().Text = "F");
        }

        private void PopulateAvailabelCourses()
        {
            if (studentDashboard.database.Connect())
            {
                try
                {
                    studentDashboard.database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM courses", studentDashboard.database.MyConn);
                    studentDashboard.database.MyReader = studentDashboard.database.MyComm.ExecuteReader();
                    if (studentDashboard.database.MyReader.HasRows)
                    {
                        while (studentDashboard.database.MyReader.Read())
                        {
                            Course.Items.Add(new TextBlock().Text = studentDashboard.database.MyReader.GetString("name"));
                        }
                    }
                    studentDashboard.database.MyReader.Close();
                    studentDashboard.database.MyReader.Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot fetch data from database!", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                studentDashboard.database.Close();
            }
        }

        private void UpdateInformation(object sender, RoutedEventArgs e)
        {
            if (studentDashboard.database.Connect())
            {
                if (studentDashboard.database.Update("UPDATE students SET fullname='" + Name.Text + "',sex='" + Sex.Text + "',course='" + Course.Text + "',year=" + Year.Text + " WHERE student_id=" + Student_ID.Text + ""))
                {
                    Badged_Update.Badge = "Updated";
                } else
                {
                    Badged_Update.Badge = "Failed to update";
                }
            } else
            {
                Badged_Update.Badge = "Failed to update";
            }
        }

        private void SetInformation()
        {
            Student_ID.Text = studentDashboard.StudentId;
            Name.Text = studentDashboard.Fullname;
            Sex.Text = studentDashboard.Sex;
            Course.Text = studentDashboard.Course;
            Year.Text = studentDashboard.Year;
            
            if (File.Exists(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".png")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".png")));
            } else if (File.Exists(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".jpg")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".jpg")));
            } else if (File.Exists(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".jpeg")))
            {
                Image_Profile.Source = new BitmapImage(new Uri(System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + ".jpeg")));
            }
        }

        private void BrowseImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select Picure";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                string imagePath = System.IO.Path.Combine(resourcePath, studentDashboard.StudentId + System.IO.Path.GetExtension(op.SafeFileName));

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
