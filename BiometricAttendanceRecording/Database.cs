using System;
using MySql.Data.MySqlClient;

namespace BiometricAttendanceRecording
{
    public class Database
    {
        public MySqlConnection MyConn;
        public MySqlCommand MyComm;
        public MySqlDataAdapter MyAdapter;
        public MySqlDataReader MyReader;

        public bool Connect()
        {
            try
            {
                MyConn = new MySqlConnection("server=127.0.0.1; username=root; database='biometric_attendance'");
                MyConn.Open();
            }
            catch (MySqlException e)
            {
                return false;
                //MessageBox.Show("Cannot connect to the database!\n\n" + e.Message, "OSS Kiosk", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return true;
        }

        public void Close()
        {
            MyConn.Close();
        }

        public bool Insert(String Query)
        {
            try
            {
                MyComm = new MySqlCommand(Query, MyConn);
                MyComm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                System.Windows.MessageBox.Show("Cannot insert data to the database!\n\n" + e.Message, "OSS Kiosk", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public bool Delete(String Query)
        {
            try
            {
                MyComm = new MySqlCommand(Query, MyConn);
                MyComm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                //MessageBox.Show("Cannot delete data to the database!\n\n" + e.Message, "OSS Kiosk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool Update(String Query)
        {
            try
            {
                MyComm = new MySqlCommand(Query, MyConn);
                MyComm.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                //MessageBox.Show("Cannot update data to the database!\n\n" + e.Message, "OSS Kiosk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
