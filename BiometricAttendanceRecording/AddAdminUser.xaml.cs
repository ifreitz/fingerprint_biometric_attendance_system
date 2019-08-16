using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DPUruNet;

namespace BiometricAttendanceRecording
{
    /// <summary>
    /// Interaction logic for AddAdminUser.xaml
    /// </summary>
    public partial class AddAdminUser : Window
    {
        FingerprintReader fingerprintReader;

        Database myDatabase;

        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;

        LinkedList<Fmd> adminsFingerprintData = new LinkedList<Fmd>();

        private Fmd firstFinger = null;

        private Fmd secondFinger = null;
        
        public AddAdminUser()
        {
            InitializeComponent();
            fingerprintReader = new FingerprintReader();
            InitializeReader();
            initializeAllAdminFingerprintData();
        }

        public void initializeAllAdminFingerprintData()
        {
            myDatabase = new Database();
            if (myDatabase.Connect())
            {
                try
                {
                    myDatabase.MyComm = new MySql.Data.MySqlClient.MySqlCommand("SELECT fingerprint FROM admninistrators", myDatabase.MyConn);
                    myDatabase.MyReader = myDatabase.MyComm.ExecuteReader();
                    if (myDatabase.MyReader.HasRows)
                    {
                        while (myDatabase.MyReader.Read())
                        {
                            adminsFingerprintData.AddFirst(fingerprintReader.DeserializeData(myDatabase.MyReader.GetString("fingerprint")));
                        }
                    }
                    myDatabase.MyReader.Close();
                    myDatabase.MyReader.Dispose();
                } catch (Exception e)
                {
                    MessageBox.Show("Cannot fetch to database, please try again later. " + e, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                    myDatabase.Close();
                    this.Close();
                }
                myDatabase.Close();
            } else
            {
                MessageBox.Show("Cannot connect to database, please try again later.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void CancelRegistration(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            try
            {
                if (!fingerprintReader.CheckCaptureResult(captureResult)) return;

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    fingerprintReader.CurrentReader.Reset();
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        richTextBox.Document.Blocks.Clear();
                        richTextBox.AppendText("\nError\nPlease try again.\nPlace your finger on the device.");
                    });
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                if (isFingerprintDataExist(resultConversion.Data))
                {
                    firstFinger = null;
                    secondFinger = null;
                    MessageBox.Show("It seems that there's slightly the same fingerprint data as yours.\n\nPlease use your other fingers.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        richTextBox.Document.Blocks.Clear();
                        richTextBox.AppendText("\nPlace any finger on the device.");
                    });
                } else
                {
                    if (firstFinger == null)
                    {
                        firstFinger = resultConversion.Data;
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            richTextBox.Document.Blocks.Clear();
                            richTextBox.AppendText("\nFinger was captured\nNow Place the same finger on the device.");
                        });
                    } else
                    {
                        if (secondFinger == null)
                        {
                            secondFinger = resultConversion.Data;
                            Fmd[] fmds = new Fmd[1];
                            fmds[0] = firstFinger;

                            // See the SDK documentation for an explanation on threshold scores.
                            int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;
                            IdentifyResult identifyResult = Comparison.Identify(secondFinger, 0, fmds, thresholdScore, 1);
                            if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                            {
                                throw new Exception(identifyResult.ResultCode.ToString());
                            }

                            if (identifyResult.Indexes.Length <= 0)
                            {
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    richTextBox.Document.Blocks.Clear();
                                    richTextBox.AppendText("\nFailed, first finger and second finger not matched.\nPlease try again, place any finger on the device.");
                                    firstFinger = null;
                                    secondFinger = null;
                                });
                            } else
                            {
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    richTextBox.Document.Blocks.Clear();
                                    richTextBox.AppendText("\nSuccess, please submit your registration now.");
                                    btn_submit.IsEnabled = true;
                                });
                                fingerprintReader.CancelCaptureAndCloseReader(this.OnCaptured);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error while capturing the fingerprint.\nPlease try again later." + ex, "Error Fingerprint", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    fingerprintReader.CurrentReader.Dispose();
                    fingerprintReader.CurrentReader = null;
                    this.Close();
                });
            }
        }
       
        private bool isFingerprintDataExist(Fmd fingerprint)
        {
            // See the SDK documentation for an explanation on threshold scores.
            int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;
            IdentifyResult identifyResult = Comparison.Identify(fingerprint, 0, adminsFingerprintData.ToArray(), thresholdScore, adminsFingerprintData.Count);
            if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                throw new Exception(identifyResult.ResultCode.ToString());
            }
            
            if (identifyResult.Indexes.Length > 0)
            {
                return true;
            }

            return false;
        }

        private void SubmitForm(String firstname, String lastname, String middlename, String position, String username, String fingerprint)
        {
            if (myDatabase.Connect())
            {
                if (myDatabase.Insert("INSERT INTO `admninistrators`(`firstname`, `middlename`, `lastname`, `username`, `position`, `fingerprint`, `createdOn`) VALUES ('"+firstname+"','"+middlename+"','"+lastname+"','"+username+"','"+position+"','"+fingerprint+"','"+DateTime.Now.ToShortDateString()+"')"))
                {
                    MessageBox.Show("Registration Successful","Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    myDatabase.Close();
                    this.Close();
                } else
                {
                    MessageBox.Show("Cannot insert to database, please try again later.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                    myDatabase.Close();
                    this.Close();
                }
            } else
            {
                MessageBox.Show("Cannot connect to database, please try again later.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btn_submit_Click(object sender, RoutedEventArgs e)
        {
            if (txtBox_fname.Text == "" || txtBox_lname.Text == "" || txtBox_position.Text == "" || txtBox_username.Text == "" || firstFinger == null || secondFinger == null)
            {
                MessageBox.Show("Please complete the registration.", "Registration Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } else
            {
                SubmitForm(txtBox_fname.Text, txtBox_lname.Text, txtBox_mname.Text, txtBox_position.Text, txtBox_username.Text, fingerprintReader.SerializeData(secondFinger));
            }
        }
    }
}
