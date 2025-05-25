using UnityEngine;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class StartConsoleServer : MonoBehaviour
{
    public Socket listenerSocket = null;
    string server = "localhost";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Pop up the server (w/ head for testing)

         Process firstProc = new Process();
         firstProc.StartInfo.FileName = "C:\\Users\\BlankyCat\\Desktop\\LP1ConsoleTemplate\\RedesSocketServer\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
         firstProc.EnableRaisingEvents = true;
         firstProc.StartInfo.CreateNoWindow = false;

         firstProc.Start();

        Thread.Sleep(2000);

        // var HostConnect = new SocketClient("127.0.0.1", 9000);
        // await client.ConnectAndCommunicateAsync();
        OpenConnection(listenerSocket);
    }

    public Socket OpenConnection(Socket listenerSocket)
    {
        try
        {
            // Create listener socket
            // Prepare an endpoint for the socket, at port 80
            IPHostEntry ipHost = Dns.GetHostEntry(server);
            IPAddress   ipAddress = null;
            for (int i = 0; i < ipHost.AddressList.Length; i++)
            {
                if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipHost.AddressList[i];
                }
            }
            IPEndPoint  remoteEP = new IPEndPoint(ipAddress, 2100);

            // Create and connect the socket to the remote endpoint (TCP)
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);                
            socket.Connect(remoteEP);
            // Debug.log($"Connected to {server}!");

                
            return listenerSocket;
        }
        catch (SocketException e)
        {
            // Console.WriteLine("Socket Exception: " + e);
            return null;
        }
    }


}
