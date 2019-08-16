using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using DPUruNet;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Database database;

        public FingerprintReader fingerprintReader;
        
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;
        
        private LinkedList<Fmd> StudenFingerPrints = new LinkedList<Fmd>();

        private LinkedList<Fmd> AdminFingerPrints = new LinkedList<Fmd>();

        private LinkedList<int> StudentId = new LinkedList<int>();
        
        private LinkedList<int> AdminId = new LinkedList<int>();

        private bool isAdminLogin = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeReader()
        {
            fingerprintReader = new FingerprintReader();
            if (!fingerprintReader.GenerateReader())
            {
                ShowError("Fingerprint Device Error", "Error while detecting the fingerprint device. Please Make sure that the device is connected to the computer. Thank you.");
            } else
            {
                if (!fingerprintReader.OpenReader())
                {
                    ErrorHeader1.Text = "Fingerprint device error";
                    ErrorContent1.Text = "Error while opening the fingerprint device.";
                    DialogMessage1.IsOpen = true;
                } else
                {
                    if (!fingerprintReader.StartCaptureAsync(this.OnCaptured))
                    {
                        ErrorHeader1.Text = "Fingerprint device error";
                        ErrorContent1.Text = "Error while capturing the fingerprint.";
                        DialogMessage1.IsOpen = true;
                    }
                }
            }
        }

        private void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                if (!fingerprintReader.CheckCaptureResult(captureResult)) return;
                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    fingerprintReader.CurrentReader.Reset();
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                // See the SDK documentation for an explanation on threshold scores.
                int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                if (isAdminLogin)
                {
                    IdentifyResult identifyResult = Comparison.Identify(resultConversion.Data, 0, AdminFingerPrints.ToArray(), thresholdScore, AdminFingerPrints.Count);
                    if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        throw new Exception(identifyResult.ResultCode.ToString());
                    }

                    if (identifyResult.Indexes.Length > 0)
                    {
                        fingerprintReader.CancelCaptureAndCloseReader(this.OnCaptured);
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            AdminDashboard adminDashboard = new AdminDashboard();
                            adminDashboard.CurrentAdminId = AdminId.ElementAt(identifyResult.Indexes[0][0]);
                            adminDashboard.Show();
                            fingerprintReader.CurrentReader.Dispose();
                            fingerprintReader.CurrentReader = null;
                            this.Close();
                        });
                    }
                } else
                {
                    IdentifyResult identifyResult = Comparison.Identify(resultConversion.Data, 0, StudenFingerPrints.ToArray(), thresholdScore, StudenFingerPrints.Count);
                    if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        throw new Exception(identifyResult.ResultCode.ToString());
                    }

                    if (identifyResult.Indexes.Length > 0)
                    {
                        fingerprintReader.CancelCaptureAndCloseReader(this.OnCaptured);
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StudentDashboard studentDash = new StudentDashboard(StudentId.ElementAt(identifyResult.Indexes[0][0]).ToString(), database);
                            studentDash.Show();
                            fingerprintReader.CurrentReader.Dispose();
                            fingerprintReader.CurrentReader = null;
                            this.Close();
                        });
                    }
                }
            }
            catch (Exception)
            {
                ErrorHeader1.Text = "Fingerprint device error";
                ErrorContent1.Text = "Error while capturing the fingerprint.";
                DialogMessage1.IsOpen = true;
            }
        }

        private void loginAsAdminToggle_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                SnackbarOne.IsActive = true;
                isAdminLogin = true;
            }
            else
            {
                SnackbarOne.IsActive = false;
                isAdminLogin = false;
            }
        }

        private void resetDevice_Click(object sender, RoutedEventArgs e)
        {
            DialogMessage1.IsOpen = false;

            fingerprintReader.CurrentReader.Reset();
            if (!fingerprintReader.OpenReader())
            {
                DialogMessage1.IsOpen = true;
            }
        }

        private void closeAppication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //fingerprintReader.CurrentReader.Dispose();
        }

        private void ShowError(string title, string content)
        {
            ErrorHeader.Text = title;
            ErrorContent.Text = content;
            DialogMessage.IsOpen = true;
        }

        private void LoadAdminFingerprints()
        {
            if (database.Connect())
            {
                try
                {
                    database.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT id, fingerprint FROM admninistrators", database.MyConn);
                    database.MyReader = database.MyComm.ExecuteReader();
                    if (database.MyReader.HasRows)
                    {
                        while (database.MyReader.Read())
                        {
                            AdminFingerPrints.AddFirst(fingerprintReader.DeserializeData(database.MyReader.GetString("fingerprint")));
                            AdminId.AddFirst(database.MyReader.GetInt32("id"));
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                }
                catch (Exception)
                {
                    ShowError("Database error", "Error while fetching data from database. Please try again later. Thank you!");
                }
                database.Close();
            } else
            {
                ShowError("Database error", "Error while connecting to database, please try again later. Thank you.");
            }
        }

        private void LoadStudentFingerprints()
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
                            StudenFingerPrints.AddFirst(fingerprintReader.DeserializeData(database.MyReader.GetString("fingerprint")));
                            StudentId.AddFirst(database.MyReader.GetInt32("student_id"));
                        }
                    }
                    database.MyReader.Close();
                    database.MyReader.Dispose();
                }
                catch (Exception)
                {
                    ShowError("Database error", "Error while fetching data from database. Please try again later. Thank you!");
                }
                database.Close();
            }
            else
            {
                ShowError("Database error", "Error while connecting to database, please try again later. Thank you.");
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            database = new Database();
            InitializeReader();
            LoadAdminFingerprints();
            LoadStudentFingerprints();
        }
    }
}
