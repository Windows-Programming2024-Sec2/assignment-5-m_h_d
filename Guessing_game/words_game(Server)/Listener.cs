using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Linq;


namespace word_game_Server_
{
    internal class Listener
    {


        internal void StartListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 5000.
                Int32 port = 5000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
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


        private void Worker(Object o)
        {
            TcpClient client = (TcpClient)o;
            Random r = new Random();
            try
            {

                String data = null;
                int wordFound = 0;

                // Load the XML file
                XDocument xmlDoc = XDocument.Load("words_game(Server).exe.config");

                // Get all the 'data' elements under 'game'
                var dataElements = xmlDoc.Descendants("data").ToList();

                // Generate a random index to select a 'data' element
                int randomIndex = r.Next(dataElements.Count);

                var selectedData = dataElements[randomIndex];


                int numOfWords = int.Parse(selectedData.Element("WordCount")?.Value ?? "0");
                List<string> validWords = selectedData.Descendants("Word").Select(w => w.Value).ToList();
                string gameString = selectedData.Element("CharacterString")?.Value ?? "Not_found";


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


        private void SendMsg(string msg, NetworkStream network)
        {
            //NetworkStream stream = client.GetStream();

            Byte[] bytes = Encoding.ASCII.GetBytes(msg);
            network.Write(bytes, 0, bytes.Length);


        }

        private string ReadMsg(NetworkStream network)
        {
            int i;
            string input;
            Byte[] bytes = new Byte[256];

            // Translate data bytes to a ASCII string.
            i = network.Read(bytes, 0, bytes.Length);
            input = System.Text.Encoding.ASCII.GetString(bytes, 0, i);


            return input;
        }


    }


}
