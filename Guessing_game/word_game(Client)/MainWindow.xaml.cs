using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Xml.Linq;

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
                SendMessage("the user" + txtName.Text+ "is Connect" );

                string serverResponse = ReceiveMessage();
                txtGameInfo.Text = serverResponse;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private void OnSubmitGuessClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtGuess.Text))
            {
                MessageBox.Show("Please enter a guess.");
                return;
            }

            SendMessage(txtGuess.Text);
            string serverResponse = ReceiveMessage();
            txtGameInfo.Text += "\n" + serverResponse;
        }

        private void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            networkStream.Write(messageBytes, 0, messageBytes.Length);
        }

        private string ReceiveMessage()
        {
            int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}