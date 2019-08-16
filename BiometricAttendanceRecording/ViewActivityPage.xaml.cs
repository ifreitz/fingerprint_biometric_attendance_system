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
    /// Interaction logic for ViewActivityPage.xaml
    /// </summary>
    public partial class ViewActivityPage : Page
    {
        private Database database;

        private ActivityControl activityControl;

        private DataSet ds;
        
        private int Id;

        private string ActivityName;

        private string Description;

        private string Date;

        private string Time;

        public ViewActivityPage(Database database)
        {
            InitializeComponent();
            this.database = database;
            PopulateAvailabelCourses();

            CBox_FilterAttendance.Items.Add(new TextBlock().Text = "Attendance In");
            CBox_FilterAttendance.Items.Add(new TextBlock().Text = "Attendance Out");
            CBox_FilterAttendance.SelectedIndex = 0;
        }

        public ActivityControl CurrentActivityControl
        {
            get { return this.activityControl; }
            set { this.activityControl = value; }
        }

        private void PopulateAvailabelCourses()
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM courses", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            CBox_FilterCourse.Items.Add(new TextBlock().Text = database.MyReader.GetString("name"));
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot fetch data from database!", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                database.Close();
            }
        }

        private void GetInAttendance()
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.in_time AS 'Time In' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_in != 0 AND act.id = "+this.Id+"");
        }

        private void GetOutAttendance()
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.out_time AS 'Time Out' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_out != 0 AND act.id = "+this.Id+"");
        }

        private void GetInAttendanceSearchId(string id)
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.in_time AS 'Time In' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_in != 0 AND act.id = "+this.Id+" AND (stu.fullname like '%"+id+"%' OR stu.student_id like '%"+id+"%')");
        }

        private void GetOutAttendanceSearchId(string id)
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.out_time AS 'Time Out' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_out != 0 AND act.id = "+this.Id+" AND (stu.fullname like '%" + id + "%' OR stu.student_id like '%" + id + "%')");
        }

        private void GetInAttendanceByCourse(string course)
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.in_time AS 'Time In' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_in != 0 AND act.id = "+this.Id+" AND stu.course = '"+course+"'");
        }

        private void GetOutAttendanceByCourse(string course)
        {
            PopulateAttendanceGrid("SELECT stu.student_id AS 'Student ID', stu.fullname AS 'Name', stu.course AS 'Course', stu.year AS 'Year', att.out_time AS 'Time Out' FROM attendance AS att INNER JOIN activities as act ON att.activity_id = act.id INNER JOIN students AS stu ON att.student_id = stu.student_id WHERE att.attendance_out != 0 AND act.id = "+this.Id+" AND stu.course = '" + course + "'");
        }

        private void PopulateAttendanceGrid(string query)
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand(query, database.MyConn);
                    database.MyAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter(database.MyComm);

                    ds = new DataSet();
                    database.MyAdapter.Fill(ds, "LoadData");
                    dataGrid.DataContext = ds;
                    txtBlock_NumberOfStudents.Text = "Number of Students: " + ds.Tables[0].Rows.Count;
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

        public void PopulateInformation(int Id, string ActivityName, string Description, string Date, string Time)
        {
            this.Id = Id;
            this.ActivityName = ActivityName;
            this.Description = Description;
            this.Date = Date;
            this.Time = Time;

            this.TxtBlock_Activity.Text = ActivityName + " > Attendance";
            this.txtBox_Name.Text = ActivityName;
            this.txtBox_Description.Text = Description;
            this.DatePicker_Date.Text = Date;
            this.TimePicker_Time.Text = Time;

            this.CBox_FilterCourse.Text = "";
            this.TxtBox_Search.Clear();
            this.CBox_FilterAttendance.SelectedIndex = 0;

            GetInAttendance();
        }

        private void UpdateInformation(object sender, RoutedEventArgs e)
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
                    if (database.Insert("UPDATE activities SET Name='"+Name+"',Date='"+Date+"',Time='"+Time+"',Description='"+Description+"' WHERE id = "+this.Id+""))
                    {
                        BadgedAdded.Badge = "Updated";
                        PopulateInformation(this.Id, Name, Description, DatePicker_Date.Text, Time);

                        activityControl.TxtBlock_Name.Text = ActivityName;
                        activityControl.TxtBlock_Description.Text = Description;
                        activityControl.TxtBlock_Date.Text = DatePicker_Date.Text + " " + Time;
                    }
                    else
                    {
                        BadgedAdded.Badge = "Failed to update";
                    }
                }
                else
                {
                    BadgedAdded.Badge = "Failed please try again later";
                }
            }
            else
            {
                BadgedAdded.Badge = "Failed to update";
            }
        }

        private void FilterAttendance(object sender, EventArgs e)
        {
            CBox_FilterCourse.Text = "";
            TxtBox_Search.Clear();
            if (CBox_FilterAttendance.SelectedIndex == 0)
            {
                GetInAttendance();
            } else
            {
                GetOutAttendance();
            }
        }

        private void FilterAttendanceByCourse(object sender, EventArgs e)
        {
            if (CBox_FilterCourse.Text != "")
            {
                if (CBox_FilterAttendance.SelectedIndex == 0)
                {
                    GetInAttendanceByCourse(CBox_FilterCourse.Text);
                } else
                {
                    GetOutAttendanceByCourse(CBox_FilterCourse.Text);
                }
            }
        }

        private void ViewAllAttendance(object sender, RoutedEventArgs e)
        {
            CBox_FilterCourse.Text = "";
            if (CBox_FilterAttendance.SelectedIndex == 0)
            {
                GetInAttendance();
            } else
            {
                GetOutAttendance();
            }
        }

        private void SearchStudentAttendance(object sender, RoutedEventArgs e)
        {
            CBox_FilterCourse.Text = "";
            if (CBox_FilterAttendance.SelectedIndex == 0)
            {
                GetInAttendanceSearchId(TxtBox_Search.Text);
            } else
            {
                GetOutAttendanceSearchId(TxtBox_Search.Text);
            }
        }

        private void StartAttendanceIn(object sender, RoutedEventArgs e)
        {
            OpenAttendancePage(true);
        }

        private void StartAttendanceOut(object sender, RoutedEventArgs e)
        {
            OpenAttendancePage(false);
        }

        private void OpenAttendancePage(bool isAttendanceIn)
        {
            RecordAttendance recordAttendance = new RecordAttendance(database);
            recordAttendance.IsAttendanceIn = isAttendanceIn;
            recordAttendance.SetActivityInformation(this.Id, this.ActivityName);
            recordAttendance.InitializeAll();
            recordAttendance.ShowDialog();
            recordAttendance = null;
        }

        private void ExportData(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
                worksheet = workbook.Sheets["Sheet1"];
                worksheet = workbook.ActiveSheet;

                for (int i = 1; i <= ds.Tables[0].Columns.Count; i++)
                {
                    worksheet.Cells[1, i] = ds.Tables[0].Columns[i - 1].ToString();
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        if (ds.Tables[0].Rows[i][j].ToString() != null)
                        {
                            worksheet.Cells[i + 2, j + 1] = ds.Tables[0].Rows[i][j].ToString();
                        }
                        else
                        {
                            worksheet.Cells[i + 2, j + 1] = "wax";
                        }
                    }
                }

                app.Columns.AutoFit();
                app.StandardFontSize = 8;
                app.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Microsoft Excel Application not found!", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
