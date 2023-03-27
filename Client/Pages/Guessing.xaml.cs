/*
* File : Guessing.xaml.cs
* Project : A05 - Client
* Programmer : Jacob Psutka
* First Version : 11/20/2022
* Description : contains the logic for guessing page
*/

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client.Pages
{
    /*
    Name:       Guessing : Page
    Purpose:    handles the logic for getting user input, 
    *           client side validation, and sending it to the server.
    */
    public partial class Guessing : Page
    {
        public Guessing()
        {
            bool connectionSucc = true; // connection status

            InitializeComponent();

            // send ip, port, and users name to the server
            MainWindow.instance.client = new ClientCommunication(MainWindow.instance.ip, MainWindow.instance.port, MainWindow.instance.userName);
            
            // Connect to server
            Task connectingTask = Task.Run(() => MainWindow.instance.client.ConnectClient());
            connectionSucc = connectingTask.Wait(100);

            // displays the updated range to the user
            updatedRange.Content = "Range: " + MainWindow.instance.client.Min + " - " + MainWindow.instance.client.Max;

            try { connectingTask.Dispose(); } catch { } // do nothing if it cant connect


            // Check to make sure Client connected to server successfully
            if (connectionSucc)
            {
                ; ; // Do nothing
            }
            if (!connectionSucc)
            {
                if (MessageBox.Show("Connection to server failed\nWould you like to retry?", "Hi-Lo Game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MainWindow.instance.View.Navigate(new Start()); // Go to start page
                }
                else
                {
                    // End game before sutting down application
                    MainWindow.instance.client.EndGame();

                    Application.Current.Shutdown();
                }
            }
            range.Text = "No Guess"; // error msg
        }

        /*
        Method:     Make_Guess
        Purpose:    After the user clicks the make guess button the method will do 
                    any nessisarry client side data input validation.
                    It will also tell the user weather their guess was too high or too low
                    and show the updated guessing range.
        */
        private void Make_Guess(object sender, RoutedEventArgs e)
        {
            string ErrorMsg = ""; // error msg
            int guess;

            // check to make sure the guess field is not left empty
            if (guessing.Text == "")
            {
                ErrorMsg = ErrorMsg + "A guess is mandatory\n";
            }
            else if (!int.TryParse(guessing.Text.ToString(), out guess)) // check to see if the guess is all numeric
            {
                ErrorMsg = ErrorMsg + "Guess field cannot contain alphanumeric characters"; // display error msg
            }
            else if (guess > int.Parse(MainWindow.instance.client.Max)) // check to make sure the user didnt guess a number which is outside the bounds of the range
            {
                ErrorMsg = ErrorMsg + "Guess field cannot exceed max guess value";
            }
            else if (guess < int.Parse(MainWindow.instance.client.Min)) // check to make sure the user didnt guess a number which is outside the bounds of the range
            {
                ErrorMsg = ErrorMsg + "Guess field cannot be below min guess value";
            }
            else // if no errors
            {
                MainWindow.instance.guess = guess.ToString();

                // return servers response to the users guess to the responce string
                string response = MainWindow.instance.client.MakeGuess(MainWindow.instance.guess);

                // update range
                updatedRange.Content = "Range: " + MainWindow.instance.client.Min + " - " + MainWindow.instance.client.Max;

                switch (response)
                {
                    case "hi":
                        range.Text = "Too High";
                        break;
                    case "lo":
                        range.Text = "Too Low";
                        break;
                    case "won":
                        // if the user wins, send them to the win page
                        MainWindow.instance.View.Navigate(new Win());
                        break;
                }
            }
            error.Content = ErrorMsg; // error msg
        }
    }
}
