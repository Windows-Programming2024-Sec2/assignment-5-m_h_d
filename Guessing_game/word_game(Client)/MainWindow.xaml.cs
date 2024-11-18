using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;




namespace word_game_Client_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private byte[] buffer = new byte[1024];
        private DispatcherTimer dispatcherTimer;
        private int timeRemaining;


        public MainWindow()
        {
            InitializeComponent();

        }

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                tcpClient = new TcpClient(txtIP.Text, int.Parse(txtPort.Text));
                networkStream = tcpClient.GetStream();

                string serverResponse = ReceiveMessage();
                txtGameInfo.Text = serverResponse;

                if (!int.TryParse(txtTimeLimit.Text, out timeRemaining) || timeRemaining <= 0)
                {
                    MessageBox.Show("Please enter a valid positive number for the time limit.");
                    return;
                }

                StartTimer();

                txtIP.IsReadOnly = true;
                txtName.IsReadOnly = true;
                txtPort.IsReadOnly = true;
                txtTimeLimit.IsReadOnly = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void OnSubmitGuessClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtGuess.Text) || tcpClient == null)
                {
                    MessageBox.Show("Please fill all the fields.");
                    txtGuess.Text = "";
                    return;
                }


                SendMessage(txtGuess.Text);
                string data = ReceiveMessage();
                if (data == "again")
                {
                    txtGuess.Text = "";
                    OnConnectClick(sender, e);
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

        private void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            networkStream.Write(messageBytes, 0, messageBytes.Length);
        }

        private string ReceiveMessage()
        {

            int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to leave ?", "exit", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                if (CheckConnection(txtIP.Text, int.Parse(txtPort.Text)))
                {
                    SendMessage("shut down");
                }

            }

        }


        private void StartTimer()
        {
            // Initialize the DispatcherTimer
            dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Tick every second
            };

            dispatcherTimer.Tick += TimerTick;
            dispatcherTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!CheckConnection(txtIP.Text, int.Parse(txtPort.Text)))
            {

                dispatcherTimer.Stop();
                MessageBox.Show("serever closed");

            }

            // Update the timer display
            if (timeRemaining > 0)
            {
                txtTimer.Text = $"Timer: {timeRemaining--} seconds remaining";
            }
            else
            {
                dispatcherTimer.Stop();
                txtTimer.Text = "Time's up!";
                MessageBox.Show("Time is up! Game over.");


            }
        }


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
    }
}


