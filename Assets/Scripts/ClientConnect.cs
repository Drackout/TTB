using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ClientConnect : MonoBehaviour
{
    //// What do we want to get: http://en.wikipedia.org/wiki/HTTP
    //string target = "en.wikipedia.org";
    //string targetRequest = "/wiki/HTTP";
    //// Use DNS to get IP address
    //IPHostEntry ipHost = Dns.GetHostEntry(target);
    //IPAddress ipAddr = ipHost.AddressList[0];

    //// Use the target IP address to decide IPv4 or IPv6
    //// Create a socket for TCP/IP (connection-oriented)
    //Socket sender = new Socket(ipAddr.AddressFamily,
    //                            SocketType.Stream,
    //                            ProtocolType.Tcp);
    //// Create the definition of the endpoint (IP address + port)
    //// Port 80 = HTTP
    //IPEndPoint endPoint = new IPEndPoint(ipAddr, 80);

    //// Need to use exception handling to work with sockets
    //try
    //{
    //    // Connect to remote server
    //    sender.Connect(endPoint);
    //                    // Convert a string to bytes (ASCII)
    //    // Sockets only work with bytes
    //    string request = $"GET {targetRequest} HTTP/1.1\r\nHost: {target}\r\n\r\n";
    //    byte[] msg = Encoding.ASCII.GetBytes(request);
    //    Console.WriteLine($"Sending:\n{request}");

    //    // Send message
    //    int bytesSent = sender.Send(msg);

    //    // Retrieve response
    //    // Maximum allowed is 8 Mb. In real code, there should be a header in the message with the size
    //    // But HTTP is based on text, so we'd have to scan the header for a length, and then get the rest
    //    // of the data
    //    byte[] bytes = new byte[8 * 1024 * 1024];
    //    int bytesRec = sender.Receive(bytes);

    //    // Convert response from bytes to a string
    //    string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);
    //    Console.WriteLine($"Response:\n{response}");

    //    // Shutdown the socket (sends a termination signal, so the
    //    // other side knows we're terminating the connection)
    //    sender.Shutdown(SocketShutdown.Both);
    //    // Closes the socket
    //    sender.Close();
    //}
    //catch (SocketException e)
    //{
    //    // In case of error, just write it
    //    Console.WriteLine($"SocketException : {e}");
    //}

}
