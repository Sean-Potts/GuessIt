/*
*   FILE            : ClientCommunication.cs
*   PROJECT         : PROG2121 - Assignment #5
*   PROGRAMMER      : Sean Potts
*   FIRST VERSION   : 2022-11-17
*   DESCRIPTION     : Contains logic for the client to communicate with the server
* 
*/

using Client.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Client
{
    /*
    *  NAME    : ClientCommunication
    *  
    *  PURPOSE : The purpose of this class is handle communication between the client and server for the hi-lo game, and return values to the 
    *            code behind the UI to be processed correctly. This class 3 methods. The first, ConnectClient(), starts the hi-lo game. The second, 
    *            MakeGuess(), allows the client to guess a value. The third, EndGame(), ends the hi-lo game. 
    * 
    */
    public class ClientCommunication
    {
        private string min;
        private string max;
        private string userName;
        private string result;
        private string ip;
        private int port;

        public string Result
        { get { return result; } set { result = value; } }
        public string Min
        { get { return min; } set { min = value; } }
        public string Max
        { get { return max; } set { max = value; } }
        public string UserName
        { get { return userName; } set { userName = value; } }
        public string Ip
        { get { return ip; } set { ip = value; } }
        public int Port
        { get { return port; } set { port = value; } }

        /*
        * CONSTRUCTOR   : ClientCommunication
        *
        * DESCRIPTION   : Sets port, userName, ipAddress to the values stored in the UI
        *
        * PARAMETERS    : string ipAddress : The ip address that will be used to connect to the server
        *               : int portNumber   : The specific port that will be used to connec to the server 
        *               : string name      : The name the user will send to the server when the game is being played
        * 
        * RETURNS       : VOID : No return value
        *	    
        */
        public ClientCommunication(string ipAddress, int portNumber, string name)
        {
            port = portNumber;
            userName = name;
            ip = ipAddress;
        }


        /*
        * METHOD        : ConnectClient
        *
        * DESCRIPTION   : This method connects to the server using TcpClient with the stored ip address and port. Once connected 
        *                 the client sends their userName to the server to start the game. The server should respond with a max and min value 
        *                 that will be stored in the fields, max & min respectively. 
        *
        * PARAMETERS    : VOID : No parameters
        * 
        * RETURNS       : VOID : No return value
        *	    
        */
        public void ConnectClient()
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(ip, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(userName);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Buffer to store the response bytes.  
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                // Server recieved will be in form result|min|max store this information in a variable
                string[] inputs = responseData.Split('|');

                // Store the information from the server in apporiate variables
                result = inputs[0];
                min = inputs[1];
                max = inputs[2];

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }


        /*
        * METHOD        : MakeGuess
        *
        * DESCRIPTION   : This method connects to the server using TcpClient with the stored ip address and port. Once connected 
        *                 the client sends their userName to the server and a guess value for the hi-lo game. The client then waits for 
        *                 the response which will contain the gameState specifying if the user won or not. The method will store the new max and min
        *                 in the apporiate fields and move on.
        *
        * PARAMETERS    : string Guess     : The guess the user has entered, for the hi-lo game that will be sent to the server to process
        * 
        * RETURNS       : string gameState : The state of the game, if the user has won, the guess was to high or low, or load if the game had a pervious state.
        *                                    The gameState variable has a default value of error, incase commincation between the client and server failed.
        *	    
        */
        public string MakeGuess(string Guess)
        {

            string gameState = "error";
           
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(ip, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(userName + "|" + Guess);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Buffer to store the response bytes.  
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                // Server recieved will be in form result|min|max store this information in a variable
                string[] inputs = responseData.Split('|');

                // The first part of the message seperated by | is the result which can be stored in both result, and gameState
                result = inputs[0]; 
                gameState = inputs[0];
          
                // Only if the user did not win will the max and min be store
                if (result != "won")
                {
                    min = inputs[1];
                    max = inputs[2];
                }

                // Close everything.
                stream.Close();

                // dont close connection?
                client.Close();
               
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return gameState;
        }


        /*
        * METHOD        : EndGame
        *
        * DESCRIPTION   : This method connects to the server using TcpClient with the stored ip address and port. Once connected 
        *                 the client sends their userName and the word "end" to tell the server they would like to quit the game. 
        * 
        * PARAMETERS    : VOID             : No parameters
        * 
        * RETURNS       : string endStatus : endStatus is defaulted to failed, if the client successfully communicates to the server, the server will 
        *                                    respond changing endStatus.
        */
        public string EndGame()
        {
            
            string endStatus = "failed";
            
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(ip, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(userName + "|" + "end");

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Buffer to store the response bytes.  
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                // Server recieved will be in form result|min|max store this information in a variable
                string[] inputs = responseData.Split('|');

                result = inputs[0];
                endStatus = inputs[0];
               
                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            
            return endStatus;
        }



        /*
        * METHOD        : ConfirmEndGame
        *
        * DESCRIPTION   : This method connects to the server using TcpClient with the stored ip address and port. Once connected 
        *                 the client sends their userName and the word "exit" to confirm that they would like to disconnect. 
        * 
        * PARAMETERS    : VOID             : No parameters
        * 
        * RETURNS       : string endStatus : endStatus is defaulted to failed, if the client successfully communicates to the server, the server will 
        *                                    respond changing endStatus.
        */
        public string ConfirmEndGame()
        {

            string endStatus = "failed";

            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                TcpClient client = new TcpClient(ip, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(userName + "|" + "exit");

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Buffer to store the response bytes.  
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                // Server recieved will be in form result|min|max store this information in a variable
                string[] inputs = responseData.Split('|');

                result = inputs[0];
                endStatus = inputs[0];

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            return endStatus;
        }



        /*
         * Method: LifelineMessage
         * description: starts the lifeline for the server to message the client
         * parameter: Dispatcher dispatcher - we need this in order to have access to change UI elements
         */
        public void LifelineMessage(Dispatcher dispatcher)
        {
            TcpClient client = new TcpClient(ip, port);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(userName + "|lifeline");
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            data = new Byte[256];
            stream.Read(data, 0, data.Length);
            stream.Close();
            client.Close();
            MainWindow.instance.client = null;

            if (MessageBox.Show("Server closed, connect to a different server?", "Hi-Lo Game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                dispatcher.Invoke(() => MainWindow.instance.View.Navigate(new Start()));
            }
            else
            {
                dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }
    }
}
