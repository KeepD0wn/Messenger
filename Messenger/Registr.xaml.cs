using System;
using System.Windows;
using System.Windows.Input;


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
                    string[] words = server.GetServerAnswer(SendPack);
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

        private void SendPack()
        {
            server.Send("0", RLog.Text, RPas.Text); //отправляет на сервер запрос на регистрацию юзера
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
