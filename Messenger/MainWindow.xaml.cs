using System;
using System.Windows;
using System.Windows.Input;
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
        static NetworkStream stream;
        static NetworkStream streamVoice;
        Server server = new Server();

        readonly string ip = "127.0.0.1"; //85.192.34.44
        readonly int port = 12000;
        readonly int portVoice = 12001;
        
        public static NetworkStream Stream { get { return stream; } }
        public static NetworkStream StreamVoice { get { return streamVoice; } }

        public  MainWindow()
        {
            try
            {
                InitializeComponent();
                Directory.CreateDirectory($@"C:\Users\{Environment.UserName}\Messenger");
                ConnectToServer();
            }
            catch
            {
                MessageBox.Show("Невозможно подключиться к серверу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        private void ConnectToServer()
        {
            TcpClient client = new TcpClient();
            TcpClient clientVoice = new TcpClient();

            client.Connect(ip, port);
            clientVoice.Connect(ip, portVoice);

            stream = client.GetStream(); //получаем сетевой поток для чтения и записи
            streamVoice = clientVoice.GetStream();
        }

        private void CompareData(string [] words)
        {
            if (words[1] == "confirmed")
            {
                User user = new User
                        (
                          Convert.ToInt32(words[2]),
                          words[3].ToString(),
                          words[4].ToString()
                        );

                ChatWin cw = new ChatWin(user);
                this.Hide();
                cw.Show();
            }
            else
            {
                Exep();
                return;
            }
        }

        private void SendPack()
        {
            server.Send("2", Logintb.Text, Passwtb.Text); //отправляет на сервер данные для входа в аккаунт для проверки логина и пароля
        }

        private void Exep()
        {
            MessageBox.Show("Неправильный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Logintb.Text = string.Empty;
            Passwtb.Text = string.Empty;
        }

        /// <summary>
        /// кнопка ввода
        /// </summary>
        private void Button_Enter(object sender, RoutedEventArgs e)
        {
            try
            {
                //проверяем логин и пароль через бд на сервере
                string[] words = server.GetServerAnswer(SendPack);
                CompareData(words);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// открывает окно регистрации
        /// </summary>
        private void Button_CheckIn(object sender, RoutedEventArgs e)
        {
            Registr reg = new Registr();
            reg.Owner = this;
            // typeof(Registr).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public).Invoke(reg, null);
            reg.Show();
        }

        private void Login_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле логина приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Enter(sender, e);
            }
        }

        private void Password_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр в поле пароля приравнивается к кнопке ввода
        {
            if (e.Key == Key.Enter) 
            {
                Button_Enter(sender, e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //при нажатии на крести все формы закрываются
        {
            Environment.Exit(0); 
        }
    }
}
