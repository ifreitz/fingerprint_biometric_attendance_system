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
    /// Interaction logic for ManageStudentsPage.xaml
    /// </summary>
    public partial class ManageStudentsPage : Page
    {
        Database database;

        DataSet ds;

        public ManageStudentsPage(Database database)
        {
            InitializeComponent();
            this.database = database;
            loadDataGrid("SELECT student_id AS Student_ID, fullname AS FullName, sex AS Sex, course AS Course, year AS Year, status AS Status FROM students");
            PopulateAvailabelCourses();
        }

        private void loadDataGrid(string query)
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
                            CourseList.Items.Add(new TextBlock().Text = database.MyReader.GetString("name"));
                            CBox_FilterCourse.Items.Add(new TextBlock().Text = database.MyReader.GetString("name"));
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                } catch (Exception)
                {
                    MessageBox.Show("Cannot fetch data from database!", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                database.Close();
            }
        }

        private void AddNewCourse(object sender, RoutedEventArgs e)
        {
            if (txtBox_Course.Text == "")
            {
                BadgedAdded.Badge = "Not Added";
            } else
            {
                if (database.Connect())
                {
                    if (database.Insert("INSERT INTO courses(name) VALUES('"+(txtBox_Course.Text).Replace("'", "")+"')"))
                    {
                        BadgedAdded.Badge = "Added";
                        CourseList.Items.Add(new TextBlock().Text = txtBox_Course.Text);
                        txtBox_Course.Clear();
                        database.Close();
                    } else
                    {
                        BadgedAdded.Badge = "Not Added";
                    }
                } else
                {
                    BadgedAdded.Badge = "Not Added";
                }
            }
        }

        private void FocusCourseField(object sender, RoutedEventArgs e)
        {
            BadgedAdded.Badge = "";
        }

        private void SearchStudent(object sender, RoutedEventArgs e)
        {
            string searchTxt = TxtBox_Search.Text;
            CBox_FilterCourse.SelectedIndex = -1;
            loadDataGrid("SELECT student_id AS Student_ID, fullname AS FullName, sex AS Sex, course AS Course, year AS Year, status AS Status FROM students WHERE fullname LIKE '%"+searchTxt+"%' OR student_id LIKE '%"+searchTxt+"%'");
        }

        private void ViewAllStudents(object sender, RoutedEventArgs e)
        {
            CBox_FilterCourse.SelectedIndex = -1;
            TxtBox_Search.Clear();
            loadDataGrid("SELECT student_id AS Student_ID, fullname AS FullName, sex AS Sex, course AS Course, year AS Year, status AS Status FROM students");
        }

        private void FilterDataByCourse(object sender, EventArgs e)
        {
            if (CBox_FilterCourse.SelectedIndex > -1)
                loadDataGrid("SELECT student_id AS Student_ID, fullname AS FullName, sex AS Sex, course AS Course, year AS Year, status AS Status FROM students WHERE course = '"+CBox_FilterCourse.Text+"'");
        }

        private void StartRegistration(object sender, RoutedEventArgs e)
        {
            StudentRegistration studentRegistration = new StudentRegistration();
            studentRegistration.CurrentDatabase = database;
            studentRegistration.SetAvailableCourses(CBox_FilterCourse.Items);
            studentRegistration.ShowDialog();
            studentRegistration = null;
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
                        } else
                        {
                            worksheet.Cells[i + 2, j + 1] = "wax";
                        }
                    }
                }
                app.Columns.AutoFit();
                app.StandardFontSize = 8;
                app.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Microsoft Excel Application not found!\n" + ex, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
