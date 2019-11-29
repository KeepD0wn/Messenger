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
using System.Reflection;
using System.Net.Sockets;
using System.IO;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {               
        static NetworkStream stream = default;
        static NetworkStream streamVoice = default;

        const string ip = "85.192.34.44"; //85.192.34.44
        const int port = 12000;
        const int portVoice = 12001;

        TcpClient client = default;
        TcpClient clientVoice = default;
        
        public static NetworkStream Stream { get { return stream; } }
        public static NetworkStream StreamVoice { get { return streamVoice; } }

        public  MainWindow()
        {
            try
            {
                InitializeComponent();
                Directory.CreateDirectory($@"C:\Users\{Environment.UserName}\Messenger");

                client = new TcpClient();
                clientVoice = new TcpClient();

                client.Connect(ip, port);
                clientVoice.Connect(ip, portVoice);

                stream = client.GetStream(); //получаем сетевой поток для чтения и записи
                streamVoice = clientVoice.GetStream();
            }
            catch
            {
                MessageBox.Show("Невозможно подключиться к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }       
        
        /// <summary>
        /// кнопка ввода
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = $"2:&#:{Logintb.Text}:&#:{Passwtb.Text}"; //запрос подтверждения данных
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                Stream.Write(buffer, 0, buffer.Length);

                byte[] IncomingMessage = new byte[128];  //ответ от сервера сходятся ли логин и пароль по БД
                do
                {
                    int bytes = stream.Read(IncomingMessage, 0, IncomingMessage.Length);
                }
                while (stream.DataAvailable); // пока данные есть в потоке

                string msgWrite = Encoding.ASCII.GetString(IncomingMessage).TrimEnd('\0');
                string[] words = msgWrite.Split(new char[] { ':', '&', '#', ':' }, StringSplitOptions.RemoveEmptyEntries);
            
                if (words[1] == "confirmed")
                {
                    User user = new User
                            (
                              Convert.ToInt32(words[3]),
                              words[4].ToString(),
                              words[5].ToString()
                            );

                    ChatWin cw = new ChatWin();
                    this.Hide();
                    cw.Show();
                }
                else
                {
                    Exep();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }       
        }

        public void Exep()
        {
            MessageBox.Show("Неправильный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Logintb.Text = string.Empty;
            Passwtb.Text = string.Empty;
        }
       
        /// <summary>
        /// открывает окно регистрации
        /// </summary>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Registr reg = new Registr();
            reg.Owner = this;
            // typeof(Registr).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public).Invoke(reg, null);
            reg.Show();
        }

        private void Logintb_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле логина приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Click(sender, e);
            }
        }

        private void Passwtb_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле пароля приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Click(sender, e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //при нажатии на крести все формы закрываются
        {
            Environment.Exit(0); 
        }
    }
}
