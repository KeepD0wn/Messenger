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
using System.Data.SqlClient;
using System.Threading;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {       
        public List<string[]> data = new List<string[]>();
        MainWindow main = new MainWindow();

        public ChatWin()
        {
            InitializeComponent();
            Thread th = new Thread(UpdateTable);
            th.Start();
        }

        public void UpdateTable() 
        {
            while (true)
            {                
                string qu = "select * from MessengerMessege";
                data.Clear();
                using (SqlCommand com = new SqlCommand(qu, main.Connect))
                {
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(new string[1]);
                        data[data.Count - 1][0] = reader[2].ToString();
                    }
                    reader.Close();
                }

                ClearTable();
                foreach (string[] s in data)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.Items.Add(s[0]); }));                       
                }                    
                
                Thread.Sleep(500);
            }
        }

        public void ClearTable()
        {
            foreach (string[] s in data)
            {                
                Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.Items.Clear(); }));
            }
        }
        
        private void TxtPost_Click(object sender, RoutedEventArgs e)
        {
            string sql = string.Format($"Insert into MessengerMessege (MessegeUserID,MessegeText,MessegeDate) values ({User.Id},'{UserTxt.Text}',GETDATE())");
            using (SqlCommand cmd = new SqlCommand(sql, main.Connect))
            {                
                    cmd.ExecuteNonQuery();     
            }
            UserTxt.Text = string.Empty;
        }               
    }
}