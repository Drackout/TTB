using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class SocketClient
{
    private readonly string _serverIp;
    private readonly int _serverPort;

    public SocketClient(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
    }

    public async Task ConnectAndCommunicateAsync()
    {
        using TcpClient client = new TcpClient();

        try
        {
            Console.WriteLine("Connecting to server...");
            await client.ConnectAsync(_serverIp, _serverPort);
            Console.WriteLine("Connected!");

            using NetworkStream stream = client.GetStream();

            // Example: send a message
            string message = "Hello Server!";
            byte[] dataToSend = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            Console.WriteLine("Sent: " + message);

            // Example: receive a response
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}