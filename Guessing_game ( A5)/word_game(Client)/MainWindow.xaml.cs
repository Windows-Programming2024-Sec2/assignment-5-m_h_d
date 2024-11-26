/*
* FILE : MainWindow.xaml.cs
* PROJECT : PROG2121 -  A5
* PROGRAMMER : Mohamad Aldabea
* FIRST VERSION : 2024 / 11  / 19 
* DESCRIPTION :
* The functions in this file are used to show and pristin the client side of the game 
*/


using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;




namespace word_game_Client_
{
    /// <summary>
    /// this class will deal with the components of the
    /// UI and send and receive the status to the server and make 
    /// the decisions based on that status
    /// </summary>
    public partial class MainWindow : Window
    {
        // create a data that will be used later
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private byte[] _buffer = new byte[256];
        private DispatcherTimer _timer;
        private int _timeRemaining;

        //
        // Method :MainWindow
        // DESCRIPTION :  initialize the the components of the app
        // PARAMETERS : none
        // RETURNS : Window
        //
        public MainWindow()
        {
            InitializeComponent();

        }

        //
        // Method :OnConnectClick
        // DESCRIPTION :  this method will make a connection between the server and the client based on the user input
        // PARAMETERS : (object sender, RoutedEventArgs e)
        // RETURNS : void
        //
        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _tcpClient = new TcpClient(txtIP.Text, int.Parse(txtPort.Text));
                _stream = _tcpClient.GetStream();

                string serverResponse = ReceiveMessage();
                chars_game.Text = serverResponse;
                chars_game.Visibility = Visibility.Visible;


                if (!int.TryParse(txtTimeLimit.Text, out _timeRemaining) || _timeRemaining <= 0)
                {
                    MessageBox.Show("Please enter a valid positive number for the time limit.");
                    return;
                }

                StartTimer();
                DataAccess(false);

            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        //
        // Method :OnSubmitGuessClick
        // DESCRIPTION :  the logic of the game status will be here , this method will start
        // sending and receiving the status to the server and make the a decision based on that
        // status and display it for the user .
        // PARAMETERS : (object sender, RoutedEventArgs e)
        // RETURNS : void
        //
        private void OnSubmitGuessClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtGuess.Text) || _tcpClient == null)
                {
                    MessageBox.Show("Please fill all the fields.");
                    txtGuess.Text = "";
                    return;
                }


                SendMessage(txtGuess.Text);
                string data = ReceiveMessage();

                if (data == "all_found")
                {
                    MessageBox.Show(data);
                    _timer.Stop();
                    txtGuess.Text = "";
                    MessageBoxResult result = MessageBox.Show(ReceiveMessage(), "exit", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        MessageBox.Show("please reconnect to the server and change the connection inputs ( if you want )");
                        DataAccess(true);
                        chars_game.Text = "";
                        txtGameInfo.Text = "";
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else
                {
                    txtGameInfo.Text += "\n" + data;
                    txtGuess.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending / receiving data" + ex);
            }


        }

        //
        // Method :SendMessage
        // DESCRIPTION :  this method will be used every time the client want to send a message to the server
        // PARAMETERS :string message
        // RETURNS : void
        //
        private void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            _stream.Write(messageBytes, 0, messageBytes.Length);
        }

        //
        // Method :ReceiveMessage
        // DESCRIPTION :  this method will be used every time the client want to receive a message from the server
        // PARAMETERS : none
        // RETURNS : string 
        //
        private string ReceiveMessage()
        {
            int bytesRead = _stream.Read(_buffer, 0, _buffer.Length);
            return Encoding.ASCII.GetString(_buffer, 0, bytesRead);

        }

        //
        // Method :Window_Closing
        // DESCRIPTION :  this method will handle the closing app , the user will ask by the server to
        // conform the closing and check if the client connect to server or no ( for safely closing )
        // PARAMETERS :(object sender, System.ComponentModel.CancelEventArgs e)
        // RETURNS : void
        //
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
            MessageBoxResult result = MessageBox.Show("Do you want to leave ?", "exit", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                _timer.Start();
            }
            else
            {
                if (CheckConnection(txtIP.Text, int.Parse(txtPort.Text)))
                {
                    SendMessage("shut down");
                }

            }

        }

        //
        // Method :StartTimer
        // DESCRIPTION :  this method will present the timer on the app that will count down in sec depending on user input
        // PARAMETERS : none
        // RETURNS : void
        //
        private void StartTimer()
        {
            // Initialize the DispatcherTimer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Tick every second
            };

            _timer.Tick += TimerTick;
            _timer.Start();
        }

        //
        // Method :TimerTick
        // DESCRIPTION :  this method will keep the timer updating every 1sec and check
        // of the connection between the server and the clint , if the connection lost , the client will notified
        // PARAMETERS : (object sender, EventArgs e)
        // RETURNS : void
        //
        private void TimerTick(object sender, EventArgs e)
        {
            if (!CheckConnection(txtIP.Text, int.Parse(txtPort.Text)))
            {

                _timer.Stop();
                MessageBox.Show("serever closed");
                Content.IsEnabled = false;
                submit.IsEnabled = false;
            }

            // Update the timer display
            if (_timeRemaining > 0)
            {
                txtTimer.Text = $"Timer: {_timeRemaining--} seconds remaining";
            }
            else
            {
                _timer.Stop();
                txtTimer.Text = "Time's up!";
                MessageBox.Show("Time is up! Game over.");


            }
        }


        //
        // Method :CheckConnection
        // DESCRIPTION : this method will check if the connection between
        // the client and server ( the client data will depending on the parameters
        // {which client to check} ) , and return the status 
        // PARAMETERS :(string ip, int portal)
        // RETURNS : bool
        //
        private bool CheckConnection(string ip, int portal)
        {
            try
            {
                TcpClient tmpC = new TcpClient(ip, portal);
                NetworkStream tmpS = tmpC.GetStream();
                byte[] messageBytes = Encoding.ASCII.GetBytes("shut down");
                tmpS.Write(messageBytes, 0, messageBytes.Length);
                return true;
            }
            catch
            {
                return false;
            }


        }

        //
        // Method :DataAccess
        // DESCRIPTION :  this method will change the access of the
        // client data ( ip , timer , etc ) depending on the status
        // PARAMETERS : (bool status)
        // RETURNS : void
        //
        private void DataAccess(bool status)
        {
            if (status)
            {
                txtIP.IsReadOnly = false;
                txtName.IsReadOnly = false;
                txtPort.IsReadOnly = false;
                txtTimeLimit.IsReadOnly = false;
            }
            if (!status)
            {
                txtIP.IsReadOnly = true;
                txtName.IsReadOnly = true;
                txtPort.IsReadOnly = true;
                txtTimeLimit.IsReadOnly = true;
            }
        }
    }
}


