/*
* File : HiLoServer.cs
* Project : PROG2121 - A05 - Server
* Programmer : Sean Potts
* First Version : 11/18/2022
* Description : handles running the server to play the game of Hi Lo
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace Server
{
  
    /*
    *  NAME    : HiLoServer
    *  
    *  PURPOSE : Handles the running of the server: listening to new messages, sending new messages to a task to be processed,
    *             sending messages back to the client, and disconnecting the client and the server. 
    * 
    */
    internal class HiLoServer
    {
        private TcpListener server = null;
        private bool running = true;

        private int min;
        private int max;
        private Random random;
        private IPAddress serverIP;
        private Int32 port;

        private Dictionary<string, ClientData> clientDic = new Dictionary<string, ClientData>();


        // Client Data: stores info about a client and their current game
        struct ClientData
        {
            public bool playing;
            public int target;
            public int min;
            public int max;
            public TcpClient lifeline;

            public ClientData(int target, int min, int max)
            {
                this.target = target;
                this.min = min;
                this.max = max;
                lifeline = null;
                playing = true;
            }
        }

        /*
        * CONSTRUCTOR   : HiLoServer
        *
        * DESCRIPTION   : Sets the min, max, ip, and port of the newly created HiLoServer
        *
        * PARAMETERS    : int min             : The lowest number that the server can generate
        *               : int max             : The highest number that the server can generate 
        *               : IPAddress serverIP  : The IP of the server - should be set to the IP of the computer or it probably wont work
        *               : Int32 port          : The port of the server
        *    
        * RETURNS       : VOID : No return value
        *	    
        */
        public HiLoServer(int min, int max, IPAddress serverIP, Int32 port)
        {
            this.min = min;
            this.max = max;
            this.serverIP = serverIP;
            this.port = port;
            random = new Random();
        }


        /*
        * METHOD        : Start
        *
        * DESCRIPTION   : Listens for clients and passes them off to a Worker tas
        *
        * PARAMETERS    : VOID : No parameters
        * 
        * RETURNS       : VOID : No return value
        *	    
        */
        public void Start()
        {
            try
            {
                server = new TcpListener(serverIP, port);
                server.Start(); // Start listening for client requests.

                
                // Enter the listening loop.
                while (running)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Task clientTask = new Task(() => Worker(client));
                    clientTask.Start();
                }
            }
            catch (SocketException e)
            {
                if (running)
                { 
                    Console.WriteLine("SocketException: {0}", e);
                }
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }



        /*
        * METHOD        : Worker
        *
        * DESCRIPTION   : Processes an incoming message and send the appropriate response. this method should be passed to a task.
        *
        * PARAMETERS    : TcpClient client : the connecting to the client that is sending the message
        * 
        *  RETURNS      : VOID : No return value
        *	    
        */
        public void Worker(TcpClient client)
        {
            // list variables
            string data = null;
            string msg;
            byte[] msgBytes;
            ClientData clientData;
            bool sendMsg = true;


            // read data
            Byte[] bytes = new Byte[256];
            NetworkStream stream = client.GetStream();
            int size = stream.Read(bytes, 0, bytes.Length);
            data = System.Text.Encoding.ASCII.GetString(bytes, 0, size);


            // parse data
            string[] inputs = data.Split('|');


            // load game data
            bool doesClientExist = clientDic.TryGetValue(inputs[0], out clientData);
            if (doesClientExist)
            {
                msg = "load|" + clientData.min.ToString() + "|" + clientData.max.ToString(); // in case of error, repeat last message
            }
            else
            {
                msg = ""; // or send nothing if there is no last message
            }


            // calculate message
            if (inputs.Length == 1) // only one input: could be starting a new game or shutting down the server 
            {
                if (inputs[0] == "stop")
                {
                    ShutDown();
                }
                else if (!doesClientExist) // if the client does not exist, create new client data
                {
                    
                    int target = random.Next(min, max + 1); // random max is exclusive, so we add 1
                    msg = "new|" + min.ToString() + "|" + max.ToString(); // create message

                    clientDic.Add(inputs[0], new ClientData(target, min, max)); // save the client's info
                }
                else if (clientDic.TryGetValue(inputs[0], out clientData) && clientData.playing == false) // if the client is messaging us after finnishing their last game, start a new one
                {
                    clientData.target = random.Next(min, max + 1); // random max is exclusive, so we add 1
                    clientData.min = min;
                    clientData.max = max;

                    msg = "new|" + min.ToString() + "|" + max.ToString(); // create message

                    clientData.playing = true;
                    clientDic[inputs[0]] = clientData; // save the new game

                }
            }
            else // if more than one input, we are playing the game
            {
                if (clientDic.TryGetValue(inputs[0], out clientData)) // get client data for calculation
                {
                    int guess;
                    if (int.TryParse(inputs[1], out guess))
                    {
                        if (guess == clientData.target) // check if they won the game
                        {
                            msg = "won";
                            clientData.playing = false; // set the clients game state to be finished (false)
                        }
                        else // if they didnt win, calculate new min/max
                        {
                            if (guess > clientData.target)
                            {
                                clientData.max = guess - 1;
                                msg = "hi";
                            }
                            else if (guess < clientData.target) // probably not needed as we already know guess is not >= target 
                            {
                                clientData.min = guess + 1;
                                msg = "lo";
                            }
                            msg += "|" + clientData.min.ToString() + "|" + clientData.max.ToString();
                        }

                        clientDic[inputs[0]] = clientData; // save changes to clientData in clientDic
                    }
                    else if (inputs[1] == "lifeline")
                    {
                        sendMsg = false; // if it is the lifeline dont respond. we will respond when the server disconnects
                        clientData.lifeline = client;
                        clientDic[inputs[0]] = clientData;

                    }
                    else if (inputs[1] == "end") // response to initial client disconnect message
                    {
                        msg = "ok";
                    }
                    else if (inputs[1] == "exit") // response to confirm client disconnect message
                    {
                        msg = "bye";
                        RemoveClient(inputs[0]);

                    }
                }
            }


            // write response
            if (sendMsg)
            {
                msgBytes = System.Text.Encoding.ASCII.GetBytes(msg); // translate the message into bytes
                stream.Write(msgBytes, 0, msgBytes.Length); // send it to the stream

                // send response/end
                client.Close();
            }
        }


        /*
        * METHOD        : ShutDown
        *
        * DESCRIPTION   : Shuts down the server and messages any clients still playing
        *
        * PARAMETERS    : VOID : No parameters
        * 
        *  RETURNS      : VOID : No return value
        *	    
        */
        internal void ShutDown()
        {
            running = false;
            try { server.Stop(); } catch { }


            foreach (string key in clientDic.Keys.ToList<string>()) // we need to make a copy or else we are reading and writing to the same place, which will cause errors
            {
                Byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes("end"); // translate the message into bytes
                clientDic[key].lifeline.GetStream().Write(msgBytes, 0, msgBytes.Length);
                clientDic[key].lifeline.Close();
                clientDic.Remove(key);
            }
        }

        /*
        * METHOD        : RemoveClient
        *
        * DESCRIPTION   : Diconnects a single client from the server
        *
        * PARAMETERS    : string key : The name of the client who is wanting to disconnect
        * 
        *  RETURNS      : VOID : No return value
        *	    
        */
        internal void RemoveClient(string key)
        {
            Byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes("end"); // translate the message into bytes
            clientDic[key].lifeline.GetStream().Write(msgBytes, 0, msgBytes.Length);
            clientDic[key].lifeline.Close();
            clientDic.Remove(key);
        }
    }
}
