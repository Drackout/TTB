using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HostServerStart : MonoBehaviour
{
    [SerializeField] private int port = 1234;

    private SpriteRenderer  spriteRenderer;
    private Socket          listenerSocket;
    private Socket[]        clientSockets;

    [SerializeField] 
    private Transform       Player1Transform;

    int                     numOfPlayers;
    int                     playerTurn;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        numOfPlayers = 0;
        playerTurn = 0;
    }

    void Update()
    {
        if (listenerSocket == null)
        {
            spriteRenderer.color = Color.red;
            OpenConnection();
        }
        else
        {
            spriteRenderer.color = Color.yellow;
            if (clientSockets == null)
            {
                spriteRenderer.color = Color.yellow;

                // Wait for a connection to be made - a new socket is created when that happens
                try
                {
                    if (numOfPlayers == 0)
                        clientSockets[0] = listenerSocket.Accept();
                    if (numOfPlayers == 1)
                        clientSockets[1] = listenerSocket.Accept();
                    if (numOfPlayers == 2)
                        clientSockets[2] = listenerSocket.Accept();
                    if (numOfPlayers == 3)
                        clientSockets[3] = listenerSocket.Accept();
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.WouldBlock)
                    {
                        // The error was that this operation would block
                        // That's to be expected in our case while we don't have a a connection
                        return;
                    }
                    else
                    {
                        Debug.LogError(e);
                    }
                }

                if (clientSockets != null)
                {
                    Debug.Log($"Player {numOfPlayers} Connected!");

                    if (numOfPlayers == 0)
                        clientSockets[0].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    if (numOfPlayers == 1)
                        clientSockets[1].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    if (numOfPlayers == 2)
                        clientSockets[2].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    if (numOfPlayers == 3)
                        clientSockets[3].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                }
            }
            else 
            {
                spriteRenderer.color = Color.green;

                ReceiveCommands();
            }
        }
    }

    void OpenConnection()
    {
        try
        {
            // Create listener socket
            // Prepare an endpoint for the socket, at port 80
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // Create a Socket that will use TCP protocol
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // A Socket must be associated with an endpoint using the Bind method
            listenerSocket.Bind(localEndPoint);

            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 1 request at a time
            listenerSocket.Listen(1);

            listenerSocket.Blocking = false;
            Debug.Log("Connection Open!");
        }
        catch (SocketException e)
        {
            Debug.LogError(e);
        }
    }

    int Receive(byte[] data, bool accountForLittleEndian = true)
    {
        try
        {
            int nByteFinal = 0;
            // Normal path - received something
            /////////// IF Player 0... 
            if (playerTurn == 0)
            {
                int nBytes0 = clientSockets[0].Receive(data);
                nByteFinal = nBytes0;
            }
            /////////// IF Player 1... 
            if (playerTurn == 1)
            {
                int nBytes1 = clientSockets[1].Receive(data);
                nByteFinal = nBytes1;
            }
            /////////// IF Player 2... 
            if (playerTurn == 2)
            {
                int nBytes2 = clientSockets[2].Receive(data);
                nByteFinal = nBytes2;
            }
            /////////// IF Player 3... 
            if (playerTurn == 3)
            {
                int nBytes3 = clientSockets[3].Receive(data);
                nByteFinal = nBytes3;
            }


            //nByteFinal 

            if (accountForLittleEndian && (!BitConverter.IsLittleEndian))
                Array.Reverse(data);

            return nByteFinal;
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.WouldBlock)
            {
                // Didn't receive any data, just return 0
                return 0;
            }
            else
            {
                // Error => log it
                Debug.LogError(e);
            }
        }
        // Return -1 if there's an error
        return -1;
    }

    void ReceiveCommands()
    {
        if (clientSocket.Connected)
        {
            var lenBytes = new byte[4];

            int nBytes = Receive(lenBytes, true);

            if (nBytes == 4)
            {
                // Convert lenBytes from 4 bytes to a Uint32
                UInt32 commandLen = BitConverter.ToUInt32(lenBytes);

                var commandBytes = new byte[commandLen];
                nBytes = Receive(commandBytes, false);

                if (nBytes == commandLen)
                {
                    string command = Encoding.ASCII.GetString(commandBytes);
                    if (command == "up")
                    {
                        transform.position += Vector3.up * 0.25f;
                        Player1Transform.position += new Vector3(0, 0, 1);
                    }
                    else if (command == "down")
                    {
                        transform.position += Vector3.down * 0.25f;
                        Player1Transform.position += new Vector3(0, 0, -1);
                    }
                    else if (command == "right")
                    {
                        transform.position += Vector3.right * 0.25f;
                        Player1Transform.position += new Vector3(1, 0, 0);
                    }
                    else if (command == "left")
                    {
                        transform.position += Vector3.left * 0.25f;
                        Player1Transform.position += new Vector3(-1, 0, 0);
                    }
                    else
                    {
                        Debug.LogError($"Unknown command {command}!");
                    }
                }
            }
            else
            {
                try
                {
                    if (clientSocket.Poll(1, SelectMode.SelectRead))
                    {
                    }
                }
                catch (SocketException e)
                {
                    Debug.LogError(e);

                    // Close the socket if it's not connected anymore
                    clientSocket.Close();
                    clientSocket = null;
                }
            }
        }
        else
        {
            // Close the socket if it's not connected anymore
            clientSocket.Close();
            clientSocket = null;
        }
    }
}
