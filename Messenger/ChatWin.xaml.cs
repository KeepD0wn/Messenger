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
using Microsoft.Speech.Recognition;
using System.Windows.Navigation;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {       
        public List<string[]> data = new List<string[]>();

        public ChatWin()
        {
            InitializeComponent();
            Thread th = new Thread(UpdateTable);
            th.Start();
        }        
         
        /// <summary>
        /// постоянно прослушивает сервер на новые сообщения от других юзеров
        /// </summary>
        public void GetMessage()
        {            
            while (true)
            {
                byte[] IncomingMessage = new byte[256]; 
                do
                {
                    int bytes = MainWindow.Stream.Read(IncomingMessage, 0, IncomingMessage.Length); //ждём сообщения
                }
                while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке

                string msgWrite = Encoding.UTF8.GetString(IncomingMessage).TrimEnd('\0');
                string[] words = msgWrite.Split(new char[] { ':', '&', '#', ':' }, StringSplitOptions.RemoveEmptyEntries); //разделяем пришедшую команду
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    TextBlock newBlock = new TextBlock();
                    newBlock.Text = words[1] + ":  " + words[2];
                    lb.Children.Add(newBlock);
                }));
            }            
        }

        /// <summary>
        /// выводит все сообщения на стекпанел и принимает их с сервера
        /// </summary>
        public void UpdateTable() 
        {
            string message = $"3:&#:K"; //просим отправить все сообщения с сервера
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            MainWindow.Stream.Write(buffer, 0, buffer.Length);

            byte[] IncomingMessage = new byte[256]; //принимаем сколько сообщений надо прочитать 
            do
            {
                int bytes = MainWindow.Stream.Read(IncomingMessage, 0, IncomingMessage.Length); //ждём сообщения
            }
            while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке

            string msgWrite = Encoding.UTF8.GetString(IncomingMessage).TrimEnd('\0');
            int count = Convert.ToInt32(msgWrite);

            for (int i=0;i<count;i++)
            {
                byte[] IncomingMessage2 = new byte[256];
                do
                {
                    int bytes = MainWindow.Stream.Read(IncomingMessage2, 0, IncomingMessage2.Length); //ждём сообщения
                    string msgWrite2 = Encoding.UTF8.GetString(IncomingMessage2).TrimEnd('\0'); //переводим в строку без лишних символов
                    string[] words = msgWrite2.Split(':');
                    data.Add(new string[2]); //записываем сообщения в лист 
                    data[data.Count - 1][0] = words[1]; //пишем ник
                    data[data.Count - 1][1] = words[2]; //пишем сообщение
                }
                while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке
            }
           
            foreach (string[] s in data)
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    TextBlock newBlock = new TextBlock();
                    newBlock.Text = s[0] + ":  " + s[1];
                    lb.Children.Add(newBlock);
                }));
            }

            Dispatcher.BeginInvoke(new ThreadStart(ScrollDown));
            Thread.Sleep(5);
            GetMessage();
        }

        /// <summary>
        /// отправляем сообщение
        /// </summary>
        private void TxtPost_Click(object sender, RoutedEventArgs e)
        {            
            if (UserTxt.Text.Trim()!=string.Empty) //проверка на пустую строку
            {
                string message = $"1:&#:{User.Login}:&#:{UserTxt.Text}";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                MainWindow.Stream.Write(buffer, 0, buffer.Length); //отправляем сообщение

                UserTxt.Text = string.Empty;
                ScrollDown();
            }            
        }

        /// <summary>
        /// опускаем чат вниз
        /// </summary>
        public void ScrollDown() //мотаем вниз
        {
            if (lb.Children.Count != 0)
                scroll.ScrollToEnd();
        }

        private void UserTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) //нажатие на энтр отправляет сообщение
            {
                TxtPost_Click(sender, e);
            }            
        }
        
        private void UserTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPicture(); //при нажатии на окно ввода картинка появляется
        }

        private void UserTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            UserTxt.Background = null; //при нажатии на окно ввода картинка исчезает
        }

        /// <summary>
        /// устанавливаем картинку "введите сообщение" в левый верхний угол
        /// </summary>
        void SetPicture()
        {
            if (UserTxt.Text == string.Empty)
            {
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri("pack://application:,,,/Resources/Writt.png", UriKind.Absolute)
                    );

                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.AlignmentY = AlignmentY.Top;
                textImageBrush.Stretch = Stretch.None;
                UserTxt.Background = textImageBrush;
            }
        }

        /// <summary>
        /// кнопка микрофона по распознаванию голосовых команд
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MicButton.Background = Brushes.DarkSlateGray;
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("ru-ru");
            SpeechRecognitionEngine speech = new SpeechRecognitionEngine(culture);
            speech.SetInputToDefaultAudioDevice();

            speech.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(OnSpeechRecognized);

            Choices num = new Choices();
            num.Add(new string[] { "закрой приложение" });

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(num);

            Grammar grammar = new Grammar(grammarBuilder);
            speech.LoadGrammar(grammar);

            speech.RecognizeAsync(RecognizeMode.Single);
        }

        void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.6)
            {
                Environment.Exit(0);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0); //при нажатии на крести все формы закрываются
        }
    }
}