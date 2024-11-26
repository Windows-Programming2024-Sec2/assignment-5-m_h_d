using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace words_game_Server_
{
    public class Listener
    {
        // constants to use in the program
        private int _port = 5000;
        private string _host = "127.0.0.2";
        private int _byteSize = 256;


        //
        // Method : StartListener
        // DESCRIPTION :this method will start lestern to a new client connection
        // and start the game when the connection success , its multi thread method ,
        // its can receive more than one client . 
        // PARAMETERS : none
        // RETURNS : void
        //
        public void StartListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 5000.
                Int32 port = _port;
                IPAddress localAddr = IPAddress.Parse(_host);


                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();


                // Enter the listening loop.
                while (true)
                {

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    ParameterizedThreadStart ts = new ParameterizedThreadStart(Worker);
                    Thread clientThread = new Thread(ts);
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        //
        // Method : Worker
        // DESCRIPTION : this is the start point of the game in server , it will start
        // sending and receiving the status of the game and processing it depending on the
        // game data in xml file and the user inputs
        // PARAMETERS :(Object o)
        // RETURNS : void
        //
        private void Worker(Object o)
        {
            TcpClient client = (TcpClient)o;
            Random r = new Random();
            try
            {

                String data = null;
                int wordFound = 0;


                string path = "C:/tmp/game";

                string[] files = Directory.GetFiles(path, "*.txt");

                string selectedFile = files[r.Next(files.Length)];

                string[] fileContent = File.ReadAllLines(selectedFile);


                string gameString = fileContent[0];

                int numOfWords = int.Parse(fileContent[1]);

                List<string> validWords = fileContent.Skip(2).ToList();


                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                SendMsg("your chars : (" + gameString + " ) \n # of words : " + numOfWords, stream);

                // Loop to receive all the data sent by the client.
                while (true)
                {
                    // this for testing the connection


                    data = ReadMsg(stream);

                    // if the user choice to close the client side app
                    if (data == "shut down")
                    {
                        break;

                    }

                    // Process the data sent by the client to see if it in the list
                    data = CheckWord(data, validWords);

                    // if nothing found in the list 
                    if (data == "nothing")
                    {
                        SendMsg("Sorry!! " + data + "found there", stream);
                    }

                    // if the input is exist in the list
                    else
                    {

                        wordFound++;

                        // if the user found all the words 
                        if (wordFound == numOfWords)
                        {
                            // send a flag to the client that all words founded , and ask if he wnants play again
                            SendMsg("all_found", stream);
                            SendMsg("do you want to play again ?", stream);


                            break;
                        }

                        // if the user entered an exist word  in the list  ,but did not find all of them yet.
                        else
                        {
                            //notify him with the new counter
                            SendMsg("Correct!! \n  you found ( " + wordFound + " ) out of  " + numOfWords, stream);
                        }
                    }


                }
            }

            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Shutdown and end connection
                client.Close();
            }
        }

        //
        // Method : CheckWord
        // DESCRIPTION :this method will take a list and a word to check if
        // the word in the list or not , and remove it if it in the list , and return the status
        // PARAMETERS :(string word, List<string> lists)
        // RETURNS :string
        //
        private string CheckWord(string word, List<string> lists)
        {


            if (lists.Contains(word))
            {
                lists.Remove(word);
                return word;
            }
            else
            {
                return "nothing";
            }

        }

        //
        // Method :  SendMsg
        // DESCRIPTION :this method will called every time the server need to send
        // a data to the client , by taking the message and the network stream
        // PARAMETERS : (string msg, NetworkStream network)
        // RETURNS : void
        //
        private void SendMsg(string msg, NetworkStream network)
        {

            Byte[] bytes = Encoding.ASCII.GetBytes(msg);
            network.Write(bytes, 0, bytes.Length);


        }

        //
        // Method :ReadMsg
        // DESCRIPTION : this method will called every time the server need to
        // read a data from the client , by taking  the network stream
        // PARAMETERS :(NetworkStream network)
        // RETURNS : string 
        //
        private string ReadMsg(NetworkStream network)
        {
            int i;
            string input;
            Byte[] bytes = new Byte[_byteSize];

            // Translate data bytes to a ASCII string.
            i = network.Read(bytes, 0, bytes.Length);
            input = System.Text.Encoding.ASCII.GetString(bytes, 0, i);


            return input;
        }

    }
}


