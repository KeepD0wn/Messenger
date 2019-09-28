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

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatWin.xaml
    /// </summary>
    public partial class ChatWin : Window
    {       
        public List<string[]> data = new List<string[]>();
        MainWindow main = new MainWindow();
        int x = 0;

        public ChatWin()
        {
            InitializeComponent();
            Thread th = new Thread(UpdateTable);
            th.Start();            
        }        

        public void UpdateTable() 
        {            
            while (true)
            {                //замутить оптимизейшн
                string qu = "select * from MessengerMessege";  
                data.Clear();
                using (SqlCommand com = new SqlCommand(qu, main.Connect))
                {
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(new string[2]);
                        data[data.Count - 1][0] = reader[1].ToString();
                        data[data.Count - 1][1] = reader[2].ToString();
                    }                    
                    reader.Close();
                }

                ClearTable();
                foreach (string[] s in data)
                {                                        
                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    { TextBlock newBlock = new TextBlock();
                      newBlock.Text = s[0] + ":  " + s[1];
                      lb.Children.Add(newBlock);
                    }));                       
                }
                
                if (x == 0)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { ScrollDown();}));
                    x += 1;
                }
                Thread.Sleep(500);
            }
        }

        public void ClearTable()
        {
            foreach (string[] s in data)
            {                                
                Dispatcher.BeginInvoke(new ThreadStart(delegate { lb.Children.Clear(); }));
            }
        }
        
        private void TxtPost_Click(object sender, RoutedEventArgs e)
        {            
            if (UserTxt.Text.Trim()!=string.Empty)
            {               
                    string sql = string.Format("Insert into MessengerMessege (MessegeUserName,MessegeText,MessegeDate) values (@login , @txt , GETDATE());");
                    using (SqlCommand cmd = new SqlCommand(sql, main.Connect))
                    {
                        cmd.Parameters.AddWithValue("@login",User.Login);
                        cmd.Parameters.AddWithValue("@txt",UserTxt.Text);
                        cmd.ExecuteNonQuery();
                    }
                    UserTxt.Text = string.Empty;
                    ScrollDown();
            }            
        }

        public void ScrollDown() //мотаем вниз
        {
            if (lb.Children.Count != 0)
                scroll.ScrollToEnd();
        }

        private void UserTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TxtPost_Click(sender, e);
            }            
        }
        
        private void UserTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPicture();
        }

        private void UserTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            UserTxt.Background = null;
        }

        void SetPicture()
        {
            if (UserTxt.Text == string.Empty)
            {
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri("Resources/Writt.png", UriKind.Relative)
                    );
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.AlignmentY = AlignmentY.Top;
                textImageBrush.Stretch = Stretch.None;
                UserTxt.Background = textImageBrush;
            }
        }

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
    }
}