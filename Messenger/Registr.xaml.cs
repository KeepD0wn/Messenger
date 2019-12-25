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
using System.Net.Sockets;


namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для Registr.xaml
    /// </summary>
    public partial class Registr : Window
    {
        Server server = new Server();

        public Registr()
        {
            InitializeComponent();
        }

        /// <summary>
        /// кнопка ввода
        /// </summary>
        private void Button_RegEnter(object sender, RoutedEventArgs e) 
        {
            try
            {
                if (RLog.Text != "" && RPas.Text != "")
                {
                    string[] words = server.GetConfirmLine(SendPack);
                    CompareData(words); //проверка нет ли уже такого юзера
                }
                else
                {
                    MessageBox.Show("Введите данные корректно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void CompareData(string[] words)
        {
            if (words[1] == "confirmed")
            {
                MessageBox.Show("Регистрация успешно подтверждена", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.None);
                RLog.Text = string.Empty;
                RPas.Text = string.Empty;
                this.Close();
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже имеется. Введите логин английскими буквами a-z, логин должен содержать не менее 4 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RLog.Text = string.Empty;
                RPas.Text = string.Empty;
            }
        }
        
        public void SendPack()
        {
            server.Send("0", RLog.Text, RPas.Text);
        }

        private void Log_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле логина приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_RegEnter(sender, e);
            }
        }

        private void Pas_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле пароля приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_RegEnter(sender, e);
            }
        }
    }
}
