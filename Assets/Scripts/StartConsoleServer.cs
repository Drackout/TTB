using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;

public class StartConsoleServer : MonoBehaviour
{
    //public Socket listenerSocket = null;
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
        //Thread.Sleep(2000);

        // Connects and Joins as player 1
        ConnectToServer();

        //while (true)
        //{
        //    msg = Console.ReadLine();
        //    if (msg == "exit")
        //    {
        //        sendMessages("closeClient");
        //        socket.Close();
        //    }
        //}
    }

    void Update()
    {
        Debug.Log("Awaiting for my turn: Receiving messages!");
        //receiveMessages();
    }

    public void LaunchGame()
    {
        Debug.Log("StartGame!");
        sendMessages("launch");
    }

    // JOIN AS A CLIENT
    public void ConnectToServer()
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
            socket.Blocking = false;
            // SEND TO SERVER
            sendMessages(msg);

            // RECEIVE FROM SERVER
            receiveMessages();
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
            Console.WriteLine("Send Error: " + e.Message);
        }
        finally
        {
            // Close the socket 
            //socket.Close();
        }
    }

    public void receiveMessages()
    {
        Debug.Log("Receiving messages!");
        try
        {
            // Buffer to receive the server's response
            byte[] buffer = new byte[1024];
            int bytesReceived = socket.Receive(buffer);

            // Convert the received data into a string
            // bytesReceived = how many bytes contain data
            string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
            // Output the response from the server
            Debug.Log("Response from server: " + response);
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock)
            {
                throw e;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Receive Error: " + e.Message);
        }


    }
    










/*
            public static async Task Main(string[] args)
            {
                string serverIp = "127.0.0.1";  // Server IP address
                int port = 5000;  // Server port

                try
                {
                    using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        await Task.Factory.StartNew(() => clientSocket.Connect(serverIp, port));
                        byte[] buffer = new byte[1024];

                        for (int round = 1; round <= 10; round++)
                        {
                            // Read server prompt
                            int bytesRead = await Task.Factory.StartNew(() => clientSocket.Receive(buffer));
                            string prompt = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine(prompt);

                            // Get user input and send it to the server
                            Console.Write("Your input: ");
                            string userInput = Console.ReadLine();
                            byte[] userInputBytes = Encoding.ASCII.GetBytes(userInput);
                            await Task.Factory.StartNew(() => clientSocket.Send(userInputBytes));

                            // Receive server response
                            bytesRead = await Task.Factory.StartNew(() => clientSocket.Receive(buffer));
                            string serverResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine($"Server response: {serverResponse}");
                        }

                        Console.WriteLine("Finished all rounds. Closing connection.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }



        */











}