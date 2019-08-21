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
    /// Interaction logic for StudentAttendancePage.xaml
    /// </summary>
    public partial class StudentAttendancePage : Page
    {
        StudentDashboard studentDashboard;
        public StudentAttendancePage(StudentDashboard studentDashboard)
        {
            InitializeComponent();
            this.studentDashboard = studentDashboard;
            PopulateDataGrid("SELECT b.Name AS 'Activity', DATE_FORMAT(Date, '%M %d, %Y') AS 'Date', b.Time, IF(a.attendance_in = 1, 'Present', 'Absent') AS 'In', IF(a.attendance_out = 1, 'Present', 'Absent') AS 'Out' FROM attendance AS a INNER JOIN activities AS b ON a.activity_id = b.id INNER JOIN students AS c ON a.student_id = c.student_id WHERE c.student_id = '" + studentDashboard.StudentId+ "'");
        }

        private void PopulateDataGrid(string query)
        {
            if (studentDashboard.database.Connect())
            {
                try
                {
                    studentDashboard.database.MyComm = new MySql.Data.MySqlClient.MySqlCommand(query, studentDashboard.database.MyConn);
                    studentDashboard.database.MyAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter(studentDashboard.database.MyComm);

                    DataSet ds = new DataSet();
                    studentDashboard.database.MyAdapter.Fill(ds, "LoadData");
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
                studentDashboard.database.Close();
            }
            else
            {
                MessageBox.Show("Cannot fetch data from database.\nPlease try again.", "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
