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
    private Socket        listenerSocket;
    private Socket[]        clientSockets;

    [SerializeField] 
    private Transform       Player1Transform;

    int                     numOfPlayers;
    int                     playerTurn;


    void Start()
    {
        clientSockets = new Socket[4];
        spriteRenderer = GetComponent<SpriteRenderer>();
        numOfPlayers = 0;
        playerTurn = 0;
    }

    void Update()
    {
        Debug.Log("num of players: " + numOfPlayers);
        if (listenerSocket == null)
        {
            Debug.Log("REEEEEE 1: " + numOfPlayers);
            spriteRenderer.color = Color.red;
            OpenConnection();
        }
        else
        {
            Debug.Log("REEEEEE 2: " + numOfPlayers);
            spriteRenderer.color = Color.yellow;
            if (clientSockets[numOfPlayers]  == null)
            {
                Debug.Log("REEEEEE 3: " + numOfPlayers);
                spriteRenderer.color = Color.yellow;

                // Wait for a connection to be made - a new socket is created when that happens
                try
                {
                    // Player 1 is the 0 in the array
                    clientSockets[numOfPlayers] = listenerSocket.Accept();
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

                if (clientSockets[numOfPlayers] != null)
                {
                    Debug.Log("REEEEEE 4: " + numOfPlayers);
                    Debug.Log($"Player {numOfPlayers} Connected!");

                    clientSockets[numOfPlayers].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    numOfPlayers++;
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
            // Normal path - received something
            /////////// IF Player 0.1.2.3... 
                int nBytes = clientSockets[playerTurn].Receive(data);

            if (accountForLittleEndian && (!BitConverter.IsLittleEndian))
                Array.Reverse(data);

            return nBytes;
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
        // Receiving commands from X player
        Debug.Log("Player Turn RECEIVE COMMANDS: " + playerTurn);

        if (clientSockets[playerTurn].Connected)
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
                    else if (command == "end")
                    {
                        if (playerTurn % numOfPlayers == 0)
                            playerTurn = 0;
                        
                        playerTurn++;
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
                    if (clientSockets[0].Poll(1, SelectMode.SelectRead)){}
                    if (clientSockets[1].Poll(1, SelectMode.SelectRead)){}
                    if (clientSockets[2].Poll(1, SelectMode.SelectRead)){}
                    if (clientSockets[3].Poll(1, SelectMode.SelectRead)){}
                }
                catch (SocketException e)
                {
                    Debug.LogError(e);

                    // Close the socket if it's not connected anymore
                    clientSockets[0].Close();
                    clientSockets[1].Close();
                    clientSockets[2].Close();
                    clientSockets[3].Close();
                    
                    clientSockets[0] = null;
                    clientSockets[1] = null;
                    clientSockets[2] = null;
                    clientSockets[3] = null;
                }
            }
        }
        else
        {
            // Close the socket if it's not connected anymore
            clientSockets[0].Close();
            clientSockets[1].Close();
            clientSockets[2].Close();
            clientSockets[3].Close();
            
            clientSockets[0] = null;
            clientSockets[1] = null;
            clientSockets[2] = null;
            clientSockets[3] = null;
        }
    }
}
