using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for AttendancePage.xaml
    /// </summary>
    public partial class AttendancePage : Page
    {
        Database database;
        public AttendancePage(Database database)
        {
            InitializeComponent();
            this.database = database;
        }

        private void PopulateDataGrid(string query)
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand(query, database.MyConn);
                    database.MyAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter(database.MyComm);

                    DataSet ds = new DataSet();
                    database.MyAdapter.Fill(ds, "LoadData");
                    dataGrid.DataContext = ds;

                    if (ds.Tables[0].Rows.Count <= 0)
                    {
                        MessageBox.Show("This student does not have any attendance record.", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Cannot fetch data from database! " + e.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                database.Close();
            }
            else
            {
                MessageBox.Show("Cannot fetch data from database.\nPlease try again.", "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindStudent(object sender, RoutedEventArgs e)
        {
            if (TxtBox_Search.Text != "")
                PopulateDataGrid("SELECT c.fullname AS 'Name', c.course AS 'Course', b.Name AS 'Activity', DATE_FORMAT(Date, '%M %d, %Y') AS 'Date', b.Time, IF(a.attendance_in = 1, 'Present', 'Absent') AS 'In', IF(a.attendance_out = 1, 'Present', 'Absent') AS 'Out' FROM attendance AS a INNER JOIN activities AS b ON a.activity_id = b.id INNER JOIN students AS c ON a.student_id = c.student_id WHERE c.student_id = '" + TxtBox_Search.Text+"'");
        }
    }
}
