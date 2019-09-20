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

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        private SqlConnection connect = null;
        public MainWindow()
        {
            InitializeComponent();
            connect = new SqlConnection("Server=31.31.196.89; Database=u0805163_2iq; User Id=u0805163_user1; Password=1337Elit72;");
            Connect.Open();           
        }

        public SqlConnection Connect { get { return connect; }}

        private void Button_Click(object sender, RoutedEventArgs e)
        {         
            if(Logintb.Text=="test" && Passwtb.Text == "12345")
            {
                ChatWin cw = new ChatWin();
                cw.Show();
                this.Close();
            }
            else
            {
                Logintb.Text = string.Empty;
                Passwtb.Text = string.Empty;
            }
        }

    }
}
