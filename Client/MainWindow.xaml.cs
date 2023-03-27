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
using Client.Pages;
using Client;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // These will all be deleted as I am going to interact with the client program directly
        public string ip;
        public int port;
        public string userName;
        public int min = 1;
        public int max = 50;
        public string guess;
        public ClientCommunication client;
        
        public static MainWindow instance;

        public MainWindow()
        {
            InitializeComponent();
            
            instance = this;

            View.Content = new Start();
        }

        // Allows window to be moved
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // 7. "the server asks for confirmation" - it makes more sense to me that the client should ask the user for confirmation but the requirements say otherwise
            if (client != null)
            {
                client.EndGame(); // send inital message to end the game with the server
            }

            if (MessageBox.Show("Confirm Exit?", "Hi-Lo Game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Stop Connection to server gracefully before sutting down application
                if (client != null)
                {
                    client.ConfirmEndGame();
                }

                Application.Current.Shutdown();
            }
            else
            {
                ; ; // Do nothing...
            }
        }
    }
}
