using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using NAudio.Wave;
using System.IO;
using System.Xml.Serialization;
using System.Media;
using Timer = System.Windows.Forms.Timer;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {
        User user;
        Server server = new Server();
        SoundClass sound = new SoundClass();

        static WaveFileWriter waveFile;
        WaveInEvent waveSource = new WaveInEvent();
        private Timer myTimer = new Timer();
        static SoundPlayer player = new SoundPlayer();

        public ChatWin(User user)
        {
            InitializeComponent();
            this.user = user;            
            UpdateTableAsync();

            waveSource.DataAvailable += WaveSource_DataAvailable;
            myTimer.Tick += OnStopRecording;
            player.Disposed +=HideBtn;
        }    

        /// <summary>
        /// выводит все сообщения на стекпанел и принимает их с сервера
        /// </summary>
        public void UpdateTable() 
        {
            try
            {
                byte[] dataByte = GetSerializedMessages();
                var messages= DeserializeMessages(dataByte);
                DisplayMessages(messages);

                Dispatcher.BeginInvoke(new ThreadStart(ScrollDown));
            }
            catch
            {
                Dispatcher.BeginInvoke(new ThreadStart(this.Hide));
                MessageBox.Show("Потеряно соединение", "Messenger", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            GetMessage();
        }

        private void DisplayMessages(List<string[]> list)
        {
            foreach (string[] s in list) //перекидывает данные из листа в блоки
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    TextBlock newBlock = new TextBlock();
                    newBlock.Text = s[0] + ":  " + s[1];
                    lb.Children.Add(newBlock);
                }));
            }
        }

        private List<string[]> DeserializeMessages(byte [] data)
        {
            string path = $@"C:\Users\{Environment.UserName}\Messenger\txtMes.txt";
            File.WriteAllBytes(path, data);
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<string[]>));
                return (List<string[]>)ser.Deserialize(file);
            }
        }

        private static byte[] GetSerializedMessages()
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
            return dataByte;
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
                 string[] words = server.GetConfirmLine();
                //byte[] IncomingMessage = new byte[256];
                //do
                //{
                //    int bytes = MainWindow.Stream.Read(IncomingMessage, 0, IncomingMessage.Length); //ждём сообщения
                //}
                //while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке
                //string[] words = GetConfirmLine();
                CompareData(words);
            }
        }

        private void CompareData(string[] words)
        {
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
                    sound.GetVoiceMsg();
                    break;
            }
        }

        public void GetVoiceMsg()
        {
            try
            {
                byte[] length = new byte[4];
                int bytesRead = MainWindow.StreamVoice.Read(length, 0, 4);
                int fileLength = BitConverter.ToInt32(length, 0);

                int bufferSize = 1024;
                int allBytesRead = 0;
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
        /// отправляем сообщение
        /// </summary>
        private void TxtPost_Click(object sender, RoutedEventArgs e) 
        {
            if (UserTxt.Text.Trim() != string.Empty) //проверка на пустую строку
            {
                server.Send("1", user.Login, UserTxt.Text);
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

        private void UserTxt_KeyDown(object sender, KeyEventArgs e) //нажатие на энтр отправляет сообщение
        {
            if(e.Key == Key.Enter) 
            {
                TxtPost_Click(sender, e);
            }            
        }
        
        private void UserTxt_LostFocus(object sender, RoutedEventArgs e) //при потере фокуса на окне ввода, появляется картинка
        {
            new PictureClass().SetPicture(UserTxt);
        }

        private void UserTxt_GotFocus(object sender, RoutedEventArgs e) //при фокусировке на окне ввода, исчезает картинка
        {
            UserTxt.Background = null; 
        }        

        /// <summary>
        /// кнопка записи звука
        /// </summary>
        private void Button_StartRecording(object sender, RoutedEventArgs e)
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
        private void Button_ListenVoiceMes(object sender, RoutedEventArgs e)
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

                sound.SendSoundToServer();
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

        private void Stop_Click(object sender, RoutedEventArgs e) //пауза при воспроизведении аудио
        {
            player.Stop();
            player.Dispose();
        }

        private void HideBtn(object sender, EventArgs e)
        {
            Stop.Visibility = Visibility.Hidden;
        }
    }
}
