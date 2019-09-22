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

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для Registr.xaml
    /// </summary>
    public partial class Registr : Window
    {
        MainWindow main = new MainWindow();
        public Registr()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RLog.Text != "" && RPas.Text != "")
                {
                    string qu = $"Insert into MessengerUsers (UserName,UserPassword) values ('{RLog.Text}','{RPas.Text}');";
                    using (SqlCommand com = new SqlCommand(qu, main.Connect))
                    {

                        com.ExecuteNonQuery();

                    }
                    MessageBox.Show("Регистрация успешно подтверждена", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.None);
                    RLog.Text = string.Empty;
                    RPas.Text = string.Empty;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Введите данные корректно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Data.SqlClient.SqlException)
            {
                MessageBox.Show("Пользователь с токим логином уже имеется", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RLog.Text = string.Empty;
                RPas.Text = string.Empty;
            }
            
        }

        private void RLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e);
            }
        }

        private void RPas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e);
            }
        }
    }
}
