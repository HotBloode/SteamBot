using System.Windows;

namespace SteamBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller c = new Controller();
        public MainWindow()
        { 
            InitializeComponent();
        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            c.LogIn(tbLog.Text,tbPass.Text, tbCode.Text);
        }
    }
}
