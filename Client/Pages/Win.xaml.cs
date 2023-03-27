/*
* File : Win.xaml.cs
* Project : A05 - Client
* Programmer : Jacob Psutka
* First Version : 11/20/2022
* Description : contains the logic for Win page
*/

using System.Windows;
using System.Windows.Controls;

namespace Client.Pages
{
    /*
    Name:       Win : Page
    Purpose:    handles the logic for showing the user that they had won the game
    *           it also allows them to either exit the game or play again
    */
    public partial class Win : Page
    {
        public Win()
        {
            InitializeComponent();
            
            MainWindow.instance.client = new ClientCommunication(MainWindow.instance.ip, MainWindow.instance.port, MainWindow.instance.userName);
        }

        /*
        Method:     PlayAgain_Click
        Purpose:    After the user wins the game, if the play again button is clicked
                    this method sends the user back to the guessing page.
        */
        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.instance.View.Navigate(new Guessing()); // Go back to guess page
        }


        /*
        Method:     Exit_Click
        Purpose:    After the user wins the game, if the exit button is clicked
                    this method calls the EndGame() method and then shuts down the application.
        */
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.instance.client.EndGame();

            Application.Current.Shutdown(); // close application
        }
    }
}
