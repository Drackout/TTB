using UnityEngine;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;

public class StartConsoleServer : MonoBehaviour
{
    public Socket listenerSocket = null;
    string server = "localhost";
    string msg;
    public IPEndPoint remoteEP;
    public Socket socket;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        msg = "";
        // Pop up the server (w/ head for testing)

        //Process firstProc = new Process();
        //firstProc.StartInfo.FileName = "C:\\Users\\BlankyCat\\Desktop\\LP1ConsoleTemplate\\RedesSocketServer\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
        //firstProc.EnableRaisingEvents = true;
        //firstProc.StartInfo.CreateNoWindow = false;

        //firstProc.Start();

        //// wait 2 secs then connects the "host"
        Thread.Sleep(2000);

        // Connects and Joins as player 1
        OpenConnection();

        while (true)
        {
            msg = Console.ReadLine();
            if (msg == "exit")
            {
                sendMessages("closeClient");
                socket.Close();
            }
        }
    }

    // JOIN AS A CLIENT
    public void OpenConnection()
    {
        try
        {
            // Create listener socket
            // Prepare an endpoint for the socket, at port 80
            IPHostEntry ipHost = Dns.GetHostEntry(server);
            IPAddress ipAddress = null;

            for (int i = 0; i < ipHost.AddressList.Length; i++)
            {
                if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipHost.AddressList[i];
                }
            }
            remoteEP = new IPEndPoint(ipAddress, 2100);

            // Create and connect the socket to the remote endpoint (TCP)
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // SEND TO SERVER
            sendMessages(msg);

            // RECEIVE FROM SERVER
            //receiveMessages(socket);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Socket Exception: " + e);
        }
    }

    public void sendMessages(string message)
    {
        try
        {
            // Connect to the endpoint
            socket.Connect(remoteEP);
            message = "Client To Server!";
            // Convert the message to a byte array
            byte[] byteData = Encoding.UTF8.GetBytes(message);
            socket.Send(byteData);
            //Console.WriteLine("Message sent to server: " + message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        finally
        {
            // Close the socket 
            //socket.Close();
        }
    }

    public void receiveMessages(Socket epSocket)
    {
        // Buffer to receive the server's response
        byte[] buffer = new byte[1024];
        int bytesReceived = epSocket.Receive(buffer);

        // Convert the received data into a string
        // bytesReceived = how many bytes contain data
        string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

        // Output the response from the server
        Console.WriteLine("Response from server: " + response);

    }


}