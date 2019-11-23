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
        public Registr()
        {
            InitializeComponent();
        }

        /// <summary>
        /// кнопка ввода
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                if (RLog.Text != "" && RPas.Text != "")
                {
                    string message = $"0:&#:{RLog.Text}:&#:{RPas.Text}"; //говорим что хотим добавить
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    MainWindow.Stream.Write(buffer, 0, buffer.Length);

                    byte[] IncomingMessage = new byte[128]; //ответ сервера всё ок или нет (есть такой логин или нет)
                    do
                    {
                        int bytes = MainWindow.Stream.Read(IncomingMessage, 0, IncomingMessage.Length);
                    }
                    while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке

                    string msgWrite = Encoding.ASCII.GetString(IncomingMessage).TrimEnd('\0'); //декодируем и разделяем на команды
                    string[] words = msgWrite.Split(new char[] { ':', '&', '#', ':' }, StringSplitOptions.RemoveEmptyEntries);

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

        private void RLog_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле логина приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Click(sender, e);
            }
        }

        private void RPas_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле пароля приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Click(sender, e);
            }
        }
    }
}
