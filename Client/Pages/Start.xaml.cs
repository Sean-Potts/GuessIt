/*
* File : Start.xaml.cs
* Project : A05 - Client
* Programmer : Jacob Psutka
* First Version : 11/20/2022
* Description : contains the logic for start page
*/

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net;

namespace Client.Pages
{
    /*
    Name:       Start : Page
    Purpose:    handles the logic for getting the information needed 
    *           to connect to the server from the user
    */
    public partial class Start : Page
    {
        public Start()
        {
            InitializeComponent();
        }

        /*
        Method:     Start_Click
        Purpose:    After the user clicks the start button, the method will 
                    do data input validation and attempt to send information/connect to the server
        */
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string ErrorMsg = ""; // error msg
            bool connectionSucc = false;

            Int32 portCheck;
            IPAddress ipCheck;


            if (ip.Text == "" || port.Text == "" || name.Text == "") // checks if any text boxes are left empty
            {
                ErrorMsg += "All input fields are mandatory\n";
            }
            else if (name.Text.Contains("|")) // checks name field for the pipe char
            {
                ErrorMsg += "Incorrect Name Format\n";
            }
            else if (name.Text.ToString() == "stop") // checks name field for the string "stop" (causes application to shutdown)
            {
                ErrorMsg += "Invalid Name\n";
            }
            else if (!Int32.TryParse(port.Text.ToString(), out portCheck)) // checks port field to make sure the user doesnt put in a port too large
            {
                ErrorMsg += "Port must be an integer";
            }
            else if (portCheck >= 65535)
            {
                ErrorMsg += "Port number exceeds max port";
            }
            else if (portCheck < 0)
            {
                ErrorMsg += "Port number must be a positive number";
            }
            else if (!IPAddress.TryParse(ip.Text.ToString(), out ipCheck))
            {
                ErrorMsg += "IP fields is invalid";
            }
            else 
            {
                MainWindow.instance.ip = ip.Text.ToString();
                MainWindow.instance.port = int.Parse(port.Text);
                MainWindow.instance.userName = name.Text.ToString();
               

                // Check to see if client has connected to the server
                MainWindow.instance.client = new ClientCommunication(MainWindow.instance.ip, MainWindow.instance.port, MainWindow.instance.userName);

                // Attempt to connect to the server
                Task connectingTask = Task.Run(() => MainWindow.instance.client.ConnectClient());
                connectionSucc = connectingTask.Wait(100);


                try { connectingTask.Dispose(); } catch { } // do nothing if it cant connect


                // Check to make sure Client connected to server successfully
                if (connectionSucc)
                {
                    // start lifeline
                    Task.Run(() => MainWindow.instance.client.LifelineMessage(Dispatcher));
                    MainWindow.instance.View.Navigate(new Guessing());
                }
                if (!connectionSucc)
                {
                    // Show popup window
                    if (MessageBox.Show("Connection to server failed\nWould you like to retry?", "Hi-Lo Game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ; ; // Do nothing...
                    }
                    else
                    {
                        // End game before sutting down application
                        MainWindow.instance.client.EndGame();

                        Application.Current.Shutdown();
                    }
                }

            }
            error.Content = ErrorMsg; // update error msg
        }
    }
}
