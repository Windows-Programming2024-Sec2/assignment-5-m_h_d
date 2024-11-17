using System;
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
        private int timeRemaining; // Time in seconds


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

        private void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            networkStream.Write(messageBytes, 0, messageBytes.Length);
        }

        private string ReceiveMessage()
        {

            int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to exit the game ?", "Exit", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                SendMessage("shut down");
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

    }
}