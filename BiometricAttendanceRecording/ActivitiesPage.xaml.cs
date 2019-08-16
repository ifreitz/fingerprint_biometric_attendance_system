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
    /// Interaction logic for ActivitiesPage.xaml
    /// </summary>
    public partial class ActivitiesPage : Page
    {
        public AdminDashboard adminDashboard;

        public Database database;
        
        public ActivitiesPage(AdminDashboard adminDashboard)
        {
            InitializeComponent();
            this.adminDashboard = adminDashboard;
            this.database = adminDashboard.database;
            PopulateActiviesPanel("SELECT * FROM activities");
        }
        
        private void PopulateActiviesPanel(string query)
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand(query, database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            Wrap_Activies.Children.Add(
                                new ActivityControl(
                                    database.MyReader.GetInt32("id"),
                                    database.MyReader.GetString("Name"),
                                    database.MyReader.GetString("Description"),
                                    database.MyReader.GetDateTime("Date").ToShortDateString(),
                                    database.MyReader.GetString("Time"),
                                    this
                                )
                            );
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot fetch activities from database, please try again later.\n\n" + e, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                MessageBox.Show("Cannot connect to database, please try again later.\n\n", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterActivities(object sender, RoutedEventArgs e)
        {
            string From = DatePicker_FilterFrom.SelectedDate.Value.Year + "-" + DatePicker_FilterFrom.SelectedDate.Value.Month + "-" + DatePicker_FilterFrom.SelectedDate.Value.Day;
            string To = DatePicker_FilterTo.SelectedDate.Value.Year + "-" + DatePicker_FilterTo.SelectedDate.Value.Month + "-" + DatePicker_FilterTo.SelectedDate.Value.Day;
            
            Wrap_Activies.Children.Clear();
            PopulateActiviesPanel("SELECT * FROM activities WHERE (Date BETWEEN '"+From+"' AND '"+To+"')");
        }

        private void ViewAllActivities(object sender, RoutedEventArgs e)
        {
            DatePicker_FilterFrom.Text = "";
            DatePicker_FilterTo.Text = "";
            Wrap_Activies.Children.Clear();
            PopulateActiviesPanel("SELECT * FROM activities");
        }

        private void SubmitForm(object sender, RoutedEventArgs e)
        {
            string Name = txtBox_Name.Text.Replace("'", "");
            string Description = txtBox_Description.Text.Replace("'", "");
            string Time = TimePicker_Time.Text;
            DateTime Date1 = (DateTime)DatePicker_Date.SelectedDate;
            string Date = Date1.Year + "-" + Date1.Month + "-" + Date1.Day;
            
            if (Name != "" && Description != "" && Date != "" && Time != "")
            {
                if (database.Connect())
                {
                    if (database.Insert("INSERT INTO activities(Name, Date, Time, Description) VALUES ('"+Name+"','"+Date+"','"+Time+"','"+Description+"')"))
                    {
                        BadgedAdded.Badge = "Added";
                        Wrap_Activies.Children.Clear();
                        PopulateActiviesPanel("SELECT * FROM activities");
                    } else
                    {
                        BadgedAdded.Badge = "Not added";
                    }
                } else
                {
                    BadgedAdded.Badge = "Failed please try again later";
                }
            } else
            {
                BadgedAdded.Badge = "Not added";
            }
        }

        private void RemoveBadge(object sender, RoutedEventArgs e)
        {
            BadgedAdded.Badge = "";
        }
    }
}
