using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleServer
{
    class SocketManager
    {
        void Start()
        {
            try
            {
                // Prepare an endpoint for the socket, at port 80
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 80);

                // Create a Socket that will use TCP protocol
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);

                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(10);

                // Wait for a connection to be made - a new socket is created when that happens
                Console.WriteLine("Waiting for a connection...1");
                Socket handler = listener.Accept();

                // Processes the request
                Console.WriteLine("Connection received, processing...");
                ProcessRequest(handler);
            }
            catch (SocketException e)
            {
                // In case of error, just write it
                Console.WriteLine($"SocketException : {e}");
            }
        }
        
        static void ProcessRequest(Socket handler)
        {
            // Prepare space for request
            string incommingRequest = "";
            byte[] bytes = new byte[1024];

            while (true)
            {
                // Read a max. of 1024 bytes
                int bytesRec = handler.Receive(bytes);
                // Convert that to a string
                incommingRequest += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                // If we've read less than 1024 bytes, assume we've already received the
                // whole message
                if (bytesRec < bytes.Length)
                {
                    // No more data to receive, just exit
                    break;
                }
            }

            // Write message received
            Console.WriteLine($"Message received:\n{incommingRequest}");

            // Prepare response
            // payload has the actual HTML we want to send
            string payload = "<html><body>This is the response from my C# server</body></html>";
            // Creates the headers
            // First indicate that the request was accepted
            string response = "HTTP/1.1 200 OK\r\n";
            // Then what kind of payload we're sending - HTML in this case
            response += "content-type: text/html; charset=UTF-8\r\n";
            // How many bytes are we sending back
            response += $"content-length: {payload.Length}\r\n";
            // An empty line
            response += "\r\n";
            // The actual payload
            response += payload;

            // Convert to bytes
            byte[] msg = Encoding.ASCII.GetBytes(response);
            // Send the message to the new socket that belongs to this actual request
            handler.Send(msg);
            // Shutdown the socket, informing the other side
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}

