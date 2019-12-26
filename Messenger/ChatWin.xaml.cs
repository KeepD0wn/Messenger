using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using NAudio.Wave;
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
        Serializator serializator = new Serializator();

        static WaveFileWriter waveFile; //TODO: перенести это в класс саунда 
        WaveInEvent waveSource = new WaveInEvent();
        private Timer myTimer = new Timer();
        static SoundPlayer player = new SoundPlayer();

        public ChatWin(User user)
        {
            this.user = user;
            InitializeComponent();                      
            UpdateTableAsync();
            var task = Task.Run(()=> sound.WriteVoiceMsgFromServer());

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
                DisplayMessages(serializator.GetAllMessages());
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

        /// <summary>
        /// постоянно прослушивает сервер на новые сообщения от других юзеров
        /// </summary>
        private void GetMessage()
        {
            while (true)
            {
                string[] words = server.GetServerAnswer();
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
            }
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

        private async void UpdateTableAsync()
        {
            await Task.Run(() => UpdateTable());
        }        

        /// <summary>
        /// опускаем чат вниз
        /// </summary>
        private void ScrollDown() //мотаем вниз
        {
            if (lb.Children.Count != 0)
                scroll.ScrollToEnd();
        }

        /// <summary>
        /// метод останавливает запись и отправляет сообщения всем клиентам
        /// </summary>
        private void OnStopRecording(object sendes, EventArgs e)
        {
            try
            {
                waveSource.StopRecording();
                waveFile.Dispose();
                myTimer.Stop();

                sound.SendVoiceMsgToServer();
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
        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void HideBtn(object sender, EventArgs e)
        {
            Stop.Visibility = Visibility.Hidden;
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

        private void Stop_Click(object sender, RoutedEventArgs e) //пауза при воспроизведении аудио
        {
            player.Stop();
            player.Dispose();
        }
    }
}
