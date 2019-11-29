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
using NAudio.Wave;
using Microsoft.Speech.Recognition;
using System.Windows.Navigation;
using System.IO;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {
        public List<string[]> listData = new List<string[]>();
        static WaveFileWriter waveFile;
        WaveInEvent waveSource = new WaveInEvent();
        private System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        static System.Media.SoundPlayer player = new System.Media.SoundPlayer();

        public ChatWin()
        {
            InitializeComponent();
            UpdateTableAsync();

            waveSource.DataAvailable += WaveSource_DataAvailable;
            myTimer.Tick += OnStopRecording;
            player.Disposed +=HideAllBtn;
        }    

        /// <summary>
        /// выводит все сообщения на стекпанел и принимает их с сервера
        /// </summary>
        public void UpdateTable() 
        {
            try 
            {
                string message = $"3:&#:K"; //просим отправить все сообщения с сервера
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                MainWindow.Stream.Write(buffer, 0, buffer.Length);

                int bufferSize = 1024;
                int bytesRead = 0;
                int allBytesRead = 0;

                byte[] length = new byte[4];
                bytesRead = MainWindow.Stream.Read(length, 0, 4); //записываем размер файла в первые 4 байта
                int fileLength = BitConverter.ToInt32(length, 0);

                int bytesLeft = fileLength;
                byte[] dataByte = new byte[fileLength];

                while (bytesLeft > 0)
                {

                    int PacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

                    bytesRead = MainWindow.Stream.Read(dataByte, allBytesRead, PacketSize);
                    allBytesRead += bytesRead;
                    bytesLeft -= bytesRead;

                }

                string path = $@"C:\Users\{Environment.UserName}\Messenger\txtMes.txt";
                File.WriteAllBytes(path, dataByte);
                XmlSerializer ser = new XmlSerializer(typeof(List<string[]>));                
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                listData = (List<string[]>)ser.Deserialize(file);
                file.Close();

                foreach (string[] s in listData) //перекидывает данные из листа в блоки
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        TextBlock newBlock = new TextBlock();
                        newBlock.Text = s[0] + ":  " + s[1];
                        lb.Children.Add(newBlock);
                    }));
                }

                Dispatcher.BeginInvoke(new ThreadStart(ScrollDown));
                GetMessage();
            }
            catch
            {
                Dispatcher.BeginInvoke(new ThreadStart(this.Hide));
                MessageBox.Show("Скорее всего сервер разовал соединение", "Messenger", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        public async void UpdateTableAsync()
        {
            await Task.Run(()=>UpdateTable());
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

                switch (words[0])
                {
                    case "4":
                        Dispatcher.BeginInvoke(new ThreadStart(delegate
                        {
                            TextBlock newBlock = new TextBlock();
                            newBlock.Text = words[1] + ":  " + words[2];
                            lb.Children.Add(newBlock);
                        }));
                        break;
                    case "5":
                        GetVoiceMsg();
                        break;
                }
            }
        }

        /// <summary>
        /// отправляем сообщение
        /// </summary>
        private void TxtPost_Click(object sender, RoutedEventArgs e) 
        {
            if (UserTxt.Text.Trim() != string.Empty) //проверка на пустую строку
            {
                string message = $"1:&#:{User.Login}:&#:{UserTxt.Text}";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                MainWindow.Stream.Write(buffer, 0, buffer.Length); //отправляем сообщение

                UserTxt.Text = string.Empty;
                ScrollDown();
            }
        }

        void GetVoiceMsg()
        {
            int bufferSize = 1024;
            int allBytesRead = 0;

            try
            {
                byte[] length = new byte[4];
                int bytesRead = MainWindow.StreamVoice.Read(length, 0, 4);
                int fileLength = BitConverter.ToInt32(length, 0);

                int bytesLeft = fileLength;
                byte[] data = new byte[fileLength];

                while (bytesLeft > 0)
                {

                    int PacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

                    bytesRead = MainWindow.StreamVoice.Read(data, allBytesRead, PacketSize);
                    allBytesRead += bytesRead;
                    bytesLeft -= bytesRead;

                }

                File.WriteAllBytes($@"C:\Users\{Environment.UserName}\Messenger\ClientSoundMes.wav", data);
            }
            catch
            {
                MessageBox.Show("Не удалось принять голосовое сообщение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void UserTxt_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр отправляет сообщение
        {
            if(e.Key == Key.Enter) 
            {
                TxtPost_Click(sender, e);
            }            
        }
        
        private void UserTxt_LostFocus(object sender, RoutedEventArgs e) //при потере фокуса на окне ввода, появляется картинка
        {
            SetPicture(); 
        }

        private void UserTxt_GotFocus(object sender, RoutedEventArgs e) //при фокусировке на окне ввода, исчезает картинка
        {
            UserTxt.Background = null; 
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
        /// кнопка записи звука
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VoiceWarning.Visibility = Visibility.Visible;
                waveSource.WaveFormat = new WaveFormat(44100, 1);
                string tempFile = ($@"C:\Users\{Environment.UserName}\Messenger\SoundMessage.wav");
                waveFile = new WaveFileWriter(tempFile, waveSource.WaveFormat); 
                waveSource.StartRecording();

                myTimer.Interval = 20000;
                myTimer.Start();
            }
            catch
            {
                OnStopRecording(this,null);                
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //при нажатии на крести все формы закрываются
        {
            Environment.Exit(0); 
        }

        /// <summary>
        /// кнопка для прослушивания голосового файла
        /// </summary>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                player.SoundLocation = $@"C:\Users\{Environment.UserName}\Messenger\ClientSoundMes.wav";
                player.Play();
                Stop.Visibility = Visibility.Visible;
            }
            catch
            {
                MessageBox.Show("Не удалось найти путь к файлу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }     
       
        /// <summary>
        /// метод останавливает запись и отправляет сообщения всем клиентам
        /// </summary>
        void OnStopRecording(object sendes,EventArgs e)
        {
            try
            {
                waveSource.StopRecording();
                waveFile.Dispose();
                myTimer.Stop();

                SendSoundToServer();
                VoiceWarning.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }
                    
        /// <summary>
        /// запись звука
        /// </summary>
        static void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
        }

        void SendSoundToServer() 
        {                        
            byte[] file = File.ReadAllBytes($@"C:\Users\{Environment.UserName}\Messenger\SoundMessage.wav");
            byte[] fileLength = BitConverter.GetBytes(file.Length); //4 байта
            byte[] package = new byte[4 + file.Length];
            fileLength.CopyTo(package, 0); 
            file.CopyTo(package, 4); //начиная с 4 байта пишем файл

            int bufferSize = 1024;
            int bytesSent = 0; //отталкиваемся с какого байта отправлять
            int bytesLeft = package.Length; //смотрим сколько осталось

            while (bytesLeft > 0)
            {

                int packetSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft; //если больше отправляем 1024, если меньше то остаток

                MainWindow.StreamVoice.Write(package, bytesSent, packetSize);
                bytesSent += packetSize;
                bytesLeft -= packetSize;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e) //пауза при воспроизведении аудио
        {
            player.Stop();
            player.Dispose();
        }

        private void HideAllBtn(object sender, EventArgs e)
        {
            Stop.Visibility = Visibility.Hidden;
        }
    }
}
