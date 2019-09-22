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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection connect = null;
        User user = new User(); // сделать доступ из других классов
        public MainWindow()
        {
            InitializeComponent();
            connect = new SqlConnection("Server=31.31.196.89; Database=u0805163_2iq; User Id=u0805163_user1; Password=!123qwe;");
            Connect.Open();            
        }       

        public SqlConnection Connect { get { return connect; } }

        private void Button_Click(object sender, RoutedEventArgs e)
        {           
            SqlCommand sql = new SqlCommand($"select * from MessengerUsers where UserName = '{Logintb.Text}'", Connect);

            using (SqlDataReader reader = sql.ExecuteReader())
            {
                int viewed = 0;   //если 0, то пользователя с таким логином нет и ридер не запустится, если 1, то есть             
                while (reader.Read())
                {
                    viewed += 1;
                    if (Passwtb.Text == reader[2].ToString())
                    {
                       user = new User
                        (
                          Convert.ToInt32(reader[0]),
                          reader[1].ToString(),
                          reader[2].ToString()
                        );

                        ChatWin cw = new ChatWin();
                        this.Close();
                        cw.Show();
                    }
                    else
                    {
                        Exep();
                        return;
                    }
                }

                if (viewed == 0)
                {
                    Exep();
                    return;
                }
            }
        }

        void Exep()
        {
            MessageBox.Show("Неправильный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Logintb.Text = string.Empty;
            Passwtb.Text = string.Empty;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Registr reg = new Registr();
            reg.Owner = this;
            reg.Show();
        }               
    }
}
