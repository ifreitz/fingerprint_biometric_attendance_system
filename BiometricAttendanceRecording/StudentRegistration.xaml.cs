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
    /// Interaction logic for StudentRegistration.xaml
    /// </summary>
    public partial class StudentRegistration : Window
    {
        private Database database;

        private FingerprintReader fingerprint = new FingerprintReader();

        private StudentInfo studentInfo = new StudentInfo();
        
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;

        public StudentRegistration()
        {
            InitializeComponent();
            Expander_FirstStep.IsExpanded = true;
            CBox_Sex.Items.Add(new TextBlock().Text = "M");
            CBox_Sex.Items.Add(new TextBlock().Text = "F");
            CBox_Year.Items.Add(new TextBlock().Text = "1");
            CBox_Year.Items.Add(new TextBlock().Text = "2");
            CBox_Year.Items.Add(new TextBlock().Text = "3");
            CBox_Year.Items.Add(new TextBlock().Text = "4");
        }

        public Database CurrentDatabase
        {
            get { return this.database; }
            set
            {
                this.database = value;
                this.database.Connect();
            }
        }

        public void SetAvailableCourses(ItemCollection items)
        {
            CBox_Course.ItemsSource = items;
        }

        private void GetStudentInfo(int StudentId)
        {
            database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM students WHERE student_id = "+StudentId+"", database.MyConn);
            database.MyReader = database.MyComm.ExecuteReader();
            if (database.MyReader.HasRows)
            {
                while (database.MyReader.Read())
                {
                    studentInfo.SetStudentInformation(
                        StudentId, true,
                        database.MyReader.GetString("fullname"),
                        database.MyReader.GetString("sex"),
                        database.MyReader.GetString("course"),
                        database.MyReader.GetInt32("year").ToString()
                    );
                }
            } else
            {
                studentInfo.SetStudentInformation(StudentId, false, "", "", "", "");
            }
            database.MyReader.Close();
            database.MyReader.Dispose();
        }

        private void NavigateSecondStep(object sender, RoutedEventArgs e)
        {
            if (txtBox_StudentID.Text != "")
            {
                GetStudentInfo(int.Parse(txtBox_StudentID.Text));

                TxtBox_StudentID2.Text = studentInfo.StudentId.ToString();
                TxtBox_Fullname.Text = studentInfo.Name;
                CBox_Course.Text = studentInfo.Course;
                CBox_Sex.Text = studentInfo.Sex;
                CBox_Year.Text = studentInfo.Year;
                studentInfo.Firstfinger = null;
                studentInfo.Lastfinger = null;
                
                Expander_FirstStep.IsEnabled = false;
                Expander_FirstStep.IsExpanded = false;
                Expander_SecondStep.IsExpanded = true;
                Expander_SecondStep.IsEnabled = true;
            }
        }

        private void NavigateThirdStep(object sender, RoutedEventArgs e)
        {
            if (TxtBox_Fullname.Text != "" && CBox_Sex.Text != "" && CBox_Course.Text != "" && CBox_Year.Text != "")
            {
                studentInfo.SetStudentInformation(
                    studentInfo.StudentId,
                    studentInfo.IsExist,
                    TxtBox_Fullname.Text,
                    CBox_Sex.Text,
                    CBox_Course.Text,
                    CBox_Year.Text
                );

                InitializeReader();

                Expander_SecondStep.IsEnabled = false;
                Expander_SecondStep.IsExpanded = false;
                Expander_ThirdStep.IsExpanded = true;
                Expander_ThirdStep.IsEnabled = true;
            }
        }

        private void NavigateFinalStep(object sender, RoutedEventArgs e)
        {
            FinishRegistration();
        }

        private void GoBackSecondStep(object sender, RoutedEventArgs e)
        {
            TxtBlock_FingerprintMessage.Text = "Place any of your finger on the device below.";
            studentInfo.Firstfinger = null;
            studentInfo.Lastfinger = null;
            fingerprint.CancelCaptureAndCloseReader(this.OnCaptured);

            Expander_ThirdStep.IsExpanded = false;
            Expander_ThirdStep.IsEnabled = false;
            Expander_SecondStep.IsEnabled = true;
            Expander_SecondStep.IsExpanded = true;
        }

        private void GoBackFirstStep(object sender, RoutedEventArgs e)
        {
            Expander_SecondStep.IsExpanded = false;
            Expander_SecondStep.IsEnabled = false;
            Expander_FirstStep.IsExpanded = true;
            Expander_FirstStep.IsEnabled = true;   
        }

        private void InitializeReader()
        {
            if (!fingerprint.GenerateReader())
            {
                MessageBox.Show("Error while detecting the fingerprint device\nPlease try again later.", "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else
            {
                if (!fingerprint.OpenReader())
                {
                    MessageBox.Show("Error while opening the fingerprint device.\nPlease try again later.", "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
                else
                {
                    if (!fingerprint.StartCaptureAsync(this.OnCaptured))
                    {
                        this.Close();
                    }
                }
            }
        }

        private void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                if (!fingerprint.CheckCaptureResult(captureResult)) return;

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    fingerprint.CurrentReader.Reset();
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        TxtBlock_FingerprintMessage.Text = "Error. Please try again. Place your finger on the device.";
                    });
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                if (studentInfo.Firstfinger == null)
                {
                    studentInfo.Firstfinger = resultConversion.Data;
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        TxtBlock_FingerprintMessage.Text = "Finger was captured. Now Place the same finger on the device.";
                    });
                } else
                {
                    if (studentInfo.Lastfinger == null)
                    {
                        studentInfo.Lastfinger = resultConversion.Data;
                        Fmd[] fmds = new Fmd[1];
                        fmds[0] = studentInfo.Firstfinger;

                        // See the SDK documentation for an explanation on threshold scores.
                        int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;
                        IdentifyResult identifyResult = Comparison.Identify(studentInfo.Lastfinger, 0, fmds, thresholdScore, 1);
                        if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                        {
                            throw new Exception(identifyResult.ResultCode.ToString());
                        }

                        if (identifyResult.Indexes.Length <= 0)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                TxtBlock_FingerprintMessage.Text = "Failed, first finger and second finger not matched. Please try again, place any finger on the device.";
                                studentInfo.Firstfinger = null;
                                studentInfo.Lastfinger = null;
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                SaveStudentToDatabase();
                                TxtBlock_FingerprintMessage.Text = "Success, your fingerprint data was saved. Please click the next button to finish.";
                                Btn_ProceedLast.IsEnabled = true;
                                fingerprint.CancelCaptureAndCloseReader(this.OnCaptured);
                            });
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error while capturing the fingerprint.\nPlease try again later." + ex, "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    fingerprint.CurrentReader.Dispose();
                    fingerprint.CurrentReader = null;
                    this.Close();
                });
            }
        }

        private void SaveStudentToDatabase()
        {
            if (studentInfo.IsExist)
            {
                // Update student data
                database.Update("UPDATE students SET fullname='"+studentInfo.Name+"',sex='"+studentInfo.Sex+"',course='"+studentInfo.Course+"',year="+studentInfo.Year+",fingerprint='"+fingerprint.SerializeData(studentInfo.Lastfinger)+"',status='Registered' WHERE student_id="+studentInfo.StudentId+"");
            } else
            {
                // Insert student data
                database.Insert("INSERT INTO students(student_id, fullname, sex, course, year, fingerprint, status) VALUES ("+studentInfo.StudentId+",'"+studentInfo.Name+"','"+studentInfo.Sex+"','"+studentInfo.Course+"',"+studentInfo.Year+",'"+fingerprint.SerializeData(studentInfo.Lastfinger)+"','Registered')");
            }
        }

        private void FinishRegistration()
        {
            studentInfo.Clear();
            txtBox_StudentID.Clear();
            Btn_ProceedLast.IsEnabled = false;
            TxtBlock_FingerprintMessage.Text = "Place any of your finger on the device below.";

            Expander_ThirdStep.IsEnabled = false;
            Expander_ThirdStep.IsExpanded = false;
            Expander_FirstStep.IsExpanded = true;
            Expander_FirstStep.IsEnabled = true;
            DialogMessage.IsOpen = true;
        }

        private void CloseMessageDialog(object sender, RoutedEventArgs e)
        {
            DialogMessage.IsOpen = false;
        }

        private void StopRegistration(object sender, System.ComponentModel.CancelEventArgs e)
        {
            database.Close();
        }
    }

    public class StudentInfo
    {
        public int StudentId;
        public bool IsExist;
        public string Name;
        public string Sex;
        public string Course;
        public string Year;

        public Fmd Firstfinger = null;
        public Fmd Lastfinger = null;

        public StudentInfo()
        {
            this.StudentId = 0;
            this.IsExist = false;
            this.Name = "";
            this.Sex = "";
            this.Course = "";
            this.Year = "";
        }
        public void SetStudentInformation(int StudentId, bool IsExist, string Name, string Sex, string Course, string Year)
        {
            this.StudentId = StudentId;
            this.IsExist = IsExist;
            this.Name = Name;
            this.Sex = Sex;
            this.Course = Course;
            this.Year = Year;
        }

        public void Clear()
        {
            this.StudentId = 0;
            this.IsExist = false;
            this.Name = "";
            this.Sex = "";
            this.Course = "";
            this.Year = "";
        }
    }
}
