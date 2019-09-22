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
            int x = 0;
            while (true)
            {                
                string qu = "select * from MessengerMessege";
                data.Clear();
                using (SqlCommand com = new SqlCommand(qu, main.Connect))
                {
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(new string[2]);
                        data[data.Count - 1][0] = reader[1].ToString();
                        data[data.Count - 1][1] = reader[2].ToString();
                    }
                    reader.Close();
                }

                ClearTable();
                foreach (string[] s in data)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.Items.Add(s[0]+":  "+s[1]); }));                       
                }

                x = +1;
                if (x == 1)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.ScrollIntoView(lb.Items[lb.Items.Count - 1]); }));
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
            if (UserTxt.Text.Trim()!=string.Empty)
            {
                try
                {
                    string sql = string.Format($"Insert into MessengerMessege (MessegeUserName,MessegeText,MessegeDate) values ('{User.Login}','{UserTxt.Text}',GETDATE())");
                    using (SqlCommand cmd = new SqlCommand(sql, main.Connect))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    UserTxt.Text = string.Empty;
                }
                catch
                {
                    MessageBox.Show("Ой-ой-ой, что-то пошло не так", "Ой-ой-ой", MessageBoxButton.OK, MessageBoxImage.None);
                }                
                lb.ScrollIntoView(lb.Items[lb.Items.Count-1]); //мотаем вниз
            }            
        }

        private void UserTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TxtPost_Click(sender, e);
            }            
        }
    }
}