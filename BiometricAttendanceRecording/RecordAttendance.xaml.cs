using DPUruNet;
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
using System.Windows.Shapes;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for RecordAttendance.xaml
    /// </summary>
    public partial class RecordAttendance : Window
    {
        private Database database;
        
        FingerprintReader fingerprintReader = new FingerprintReader();
        
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;
        
        LinkedList<Fmd> studentFmds = new LinkedList<Fmd>();

        LinkedList<string> studentIds = new LinkedList<string>();

        LinkedList<string> AttendedIds = new LinkedList<string>();
        
        private bool isAttendanceIn;

        private int activityId;

        private string activityName;

        public RecordAttendance(Database database)
        {
            InitializeComponent();
            this.database = database;
            GetStudentFingerprints();
            InitializeReader();
        }

        public bool IsAttendanceIn
        {
            get { return this.isAttendanceIn; }
            set { this.isAttendanceIn = value; }
        }

        public void SetActivityInformation(int activityId, string activityName)
        {
            this.activityId = activityId;
            this.activityName = activityName;
            if (IsAttendanceIn)
            {
                TxtBlock_ActivityType.Text = "In";
            } else
            {
                TxtBlock_ActivityType.Text = "Out";
            }
        }

        private void InitializeReader()
        {
            if (!fingerprintReader.GenerateReader())
            {
                MessageBox.Show("Error while detecting the fingerprint device\nPlease try again later.", "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else
            {
                if (!fingerprintReader.OpenReader())
                {
                    MessageBox.Show("Error while opening the fingerprint device.\nPlease try again later.", "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
                else
                {
                    if (!fingerprintReader.StartCaptureAsync(this.OnCaptured))
                    {
                        this.Close();
                    }
                }
            }
        }

        private void OnCaptured(CaptureResult captureResult)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Snackbar_Recorded.IsActive = false;
            });
            try
            {
                if (!fingerprintReader.CheckCaptureResult(captureResult)) return;

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    fingerprintReader.CurrentReader.Reset();
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Snackbar_Recorded.IsActive = true;
                        SnackBar_Message.Text = "Sorry, please try again.";
                    });
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                // See the SDK documentation for an explanation on threshold scores.
                int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;
                IdentifyResult identifyResult = Comparison.Identify(resultConversion.Data, 0, studentFmds.ToArray(), thresholdScore, studentFmds.Count);
                if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    throw new Exception(identifyResult.ResultCode.ToString());
                }

                if (identifyResult.Indexes.Length > 0)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        string findStudentId = studentIds.ElementAt(identifyResult.Indexes[0][0]);
                        UpdateStudentInformation(findStudentId);
                        if (IsAttendedAlready(findStudentId))
                        {
                            SnackBar_Message.Text = "Your attendance is already recorded.";
                            Snackbar_Recorded.IsActive = true;
                        } else
                        {
                            if (SaveAttendancetoDatabase(findStudentId))
                            {
                                SnackBar_Message.Text = "Successful, your attendance is recorded.";
                                Snackbar_Recorded.IsActive = true;
                                AttendedIds.AddFirst(findStudentId);
                            } else
                            {
                                SnackBar_Message.Text = "Sorry, that doesn't work. Please try again.";
                                Snackbar_Recorded.IsActive = true;
                            }
                        }
                    });
                } else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        TxtBox_StudentID.Clear();
                        TxtBox_Name.Clear();
                        TxtBox_Course.Clear();
                        TxtBox_Year.Clear();
                        SnackBar_Message.Text = "Sorry, that doesn't work. Please try again.";
                        Snackbar_Recorded.IsActive = true;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while capturing the fingerprint.\nPlease try again later." + ex, "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    fingerprintReader.CurrentReader.Reset();
                });
            }
        }


        private bool IsAttendedAlready(string studentId)
        {
            for (int i = 0; i < AttendedIds.Count; i++)
            {
                if (AttendedIds.ElementAt(i) == studentId)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SaveAttendancetoDatabase(string studentId)
        {
            bool returnValue = true;
            if (database.Connect())
            {
                try
                {
                    bool isExist = false;
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT student_id FROM attendance WHERE student_id = '"+studentId+"' AND activity_id = "+this.activityId+"", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        isExist = true;
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();

                    if (isExist)
                    {
                        if (isAttendanceIn)
                        {
                            if (!database.Update("UPDATE attendance SET attendance_in=1,in_time='"+System.DateTime.Now.ToShortTimeString()+"' WHERE student_id='"+studentId+"' AND activity_id = "+this.activityId+""))
                            {
                                returnValue = false;
                            }
                        } else
                        {
                            if (!database.Update("UPDATE attendance SET attendance_out=1,out_time='"+System.DateTime.Now.ToShortTimeString()+"' WHERE student_id='"+studentId+"' AND activity_id = "+this.activityId+""))
                            {
                                returnValue = false;
                            }
                        }
                    } else
                    {
                        if (isAttendanceIn)
                        {
                            if (!database.Insert("INSERT INTO attendance(activity_id, student_id, attendance_in, in_time) VALUES ("+this.activityId+", '"+studentId+"', 1, '"+System.DateTime.Now.ToShortTimeString()+"')"))
                            {
                                returnValue = false;
                            }
                        }
                        else
                        {
                            if (!database.Insert("INSERT INTO attendance(activity_id, student_id, attendance_out, out_time) VALUES ("+this.activityId+", '"+studentId+"', 1, '"+System.DateTime.Now.ToShortTimeString()+"')"))
                            {
                                returnValue = false;
                            }
                        }
                    }
                } catch (Exception e)
                {
                    MessageBox.Show("Sorry Cannot fetch data from database.\nPleese try again\n"+e,"Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    returnValue = false;
                }
            } else
            {
                MessageBox.Show("Sorry Cannot connect to database.\nPlease try again.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                returnValue = false;
            }
            
            return returnValue;
        }

        private void GetStudentFingerprints()
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT student_id, fingerprint FROM students WHERE fingerprint != ''", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            studentFmds.AddFirst(fingerprintReader.DeserializeData(database.MyReader.GetString("fingerprint")));
                            studentIds.AddFirst(database.MyReader.GetString("student_id"));
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                } catch (Exception e)
                {
                    MessageBox.Show("Sorry cannot fetch student data from database.\nPlease try again later.\n" + e, "Databse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
                database.Close();
            } else
            {
                MessageBox.Show("Sorry cannot connect to the database.\nPlease try again later.", "Databse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
        
        private void UpdateStudentInformation(string studentId)
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT student_id, fullname, course, year FROM students WHERE student_id = '" + studentId+"'", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            TxtBox_StudentID.Text = database.MyReader.GetString("student_id");
                            TxtBox_Name.Text = database.MyReader.GetString("fullname");
                            TxtBox_Course.Text = database.MyReader.GetString("course");
                            TxtBox_Year.Text = database.MyReader.GetInt32("year").ToString();
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                } catch (Exception e)
                {
                    MessageBox.Show("Sorry cannot fetch student data from database.\nPlease try again later.\n" + e, "Databse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                MessageBox.Show("Sorry cannot connect to the database.\nPlease try again later.", "Databse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void InitializeAll()
        {
            TxtBlock_ActivityName.Text = this.activityName;
        }
    }
}
