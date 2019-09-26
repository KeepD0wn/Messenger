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
        int x = 0;

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
                        data.Add(new string[2]);
                        data[data.Count - 1][0] = reader[1].ToString();
                        data[data.Count - 1][1] = reader[2].ToString();
                    }
                    reader.Close();
                }

                ClearTable();
                foreach (string[] s in data)
                {                                        
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    { TextBlock newBlock = new TextBlock();
                      newBlock.Text = s[0] + ":  " + s[1];
                      lb.Children.Add(newBlock);
                    }));                       
                }
                
                if (x == 0)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { ScrollDown();}));
                    x += 1;
                }
                Thread.Sleep(500);
            }
        }

        public void ClearTable()
        {
            foreach (string[] s in data)
            {                                
                Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.Children.Clear(); }));
            }
        }
        
        private void TxtPost_Click(object sender, RoutedEventArgs e)
        {            
            if (UserTxt.Text.Trim()!=string.Empty)
            {               
                    string sql = string.Format("Insert into MessengerMessege (MessegeUserName,MessegeText,MessegeDate) values (@login , @txt , GETDATE());");
                    using (SqlCommand cmd = new SqlCommand(sql, main.Connect))
                    {
                        cmd.Parameters.AddWithValue("@login",User.Login);
                        cmd.Parameters.AddWithValue("@txt",UserTxt.Text);
                        cmd.ExecuteNonQuery();
                    }
                    UserTxt.Text = string.Empty;
                    ScrollDown();
            }            
        }

        public void ScrollDown() //мотаем вниз
        {
            if (lb.Children.Count != 0)
                scroll.ScrollToEnd();
        }

        private void UserTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TxtPost_Click(sender, e);
            }            
        }

        private void UserTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserTxt.Text == "")
            {
                // Create an ImageBrush.
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri("Resources/Writt.png", UriKind.Relative)
                    );
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.AlignmentY = AlignmentY.Top;
                textImageBrush.Stretch = Stretch.None;
                // Use the brush to paint the button's background.
                UserTxt.Background = textImageBrush;

            }
            else
            {
                UserTxt.Background = null;
            }
        }
    }
}