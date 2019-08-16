using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for AdministratorsPage.xaml
    /// </summary>
    public partial class AdministratorsPage : Page
    {
        Database myDatabase;
        public AdministratorsPage(Database database)
        {
            InitializeComponent();

            this.myDatabase = database;
            PopulateDataGrid("SELECT firstname AS FirstName, middlename AS MiddleName, lastname AS LastName, username AS Username, position AS Role, createdOn AS CreateOn FROM admninistrators");
        }

        private void AddAdminAccount(object sender, RoutedEventArgs e)
        {
            AddAdminUser addAdminUserPage = new AddAdminUser();
            addAdminUserPage.ShowDialog();
            addAdminUserPage = null;
        }

        private void PopulateDataGrid(string query)
        {
            if (myDatabase.Connect())
            {
                try
                {
                    myDatabase.MyComm = new MySql.Data.MySqlClient.MySqlCommand(query, myDatabase.MyConn);
                    myDatabase.MyAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter(myDatabase.MyComm);

                    DataSet ds = new DataSet();
                    myDatabase.MyAdapter.Fill(ds, "LoadData");
                    dataGrid.DataContext = ds;
                    txtBlock_NumberOfAdmins.Text = "Number of Administrators: " + ds.Tables[0].Rows.Count;

                    myDatabase.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot fetch data from database!", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Cannot fetch data from database.\nPlease try again.", "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGridByName(object sender, RoutedEventArgs e)
        {
            if (TxtBox_Search.Text != "")
            {
                PopulateDataGrid("SELECT firstname AS FirstName, middlename AS MiddleName, lastname AS LastName, username AS Username, position AS Role, createdOn AS CreateOn FROM admninistrators WHERE firstname LIKE '%"+TxtBox_Search.Text+"%' OR middlename LIKE '%"+TxtBox_Search.Text+"%' OR lastname LIKE '%"+TxtBox_Search.Text+"%'");
            }
        }

        private void loadDataGrid(object sender, RoutedEventArgs e)
        {
            TxtBox_Search.Clear();
            PopulateDataGrid("SELECT firstname AS FirstName, middlename AS MiddleName, lastname AS LastName, username AS Username, position AS Role, createdOn AS CreateOn FROM admninistrators");
        }
    }
}
