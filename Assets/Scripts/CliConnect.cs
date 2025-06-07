using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;

public class CliConnect : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;
    string server = "localhost";
    public IPEndPoint remoteEP;
    public Socket socket;
    public GameObject plane;
    private GameObject playerPosition;
    public GameObject startGameScreen;
    public Transform PlayerList;
    public GameObject PlayerListPrefab;
    public TextMeshProUGUI infoBox;
    public TextMeshProUGUI DisplayID;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;
    private int playerId;
    public GameObject gameContols;
    public Button[] gameButtons;
    
    private LineRenderer LR;

    // Run things in main thread instead of the Clients
    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();
    
    void Start()
    {
        // DOESNT WORK AS INTENDED
        //LR = GetComponent<LineRenderer>();
        //LR.positionCount = 2;

        ConnectToServer();
    }

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 30;
        Screen.SetResolution(1280, 720, false);
        Screen.fullScreen = false;
        gameContols.SetActive(false);
        foreach (Button btn in gameButtons)
        {
            btn.interactable = false;
        }
    }


    ///// had help with this to run outside Client thread
    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue().Invoke();
            }
        }
    }
    public static void RunOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }
    /////


    // Establishes the TCP connection with the server
    void ConnectToServer()
    {
        try
        {
            IPHostEntry ipHost = Dns.GetHostEntry(server);
            IPAddress ipAddress = null;

            // Use IPv4 - same as teacher
            for (int i = 0; i < ipHost.AddressList.Length; i++)
            {
                if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ipHost.AddressList[i];
                }
            }
            remoteEP = new IPEndPoint(ipAddress, 2100);

            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //socket.Blocking = false;
            socket.Connect(remoteEP);

            // Initialize the NetworkStream to send/receive data over the socket
            // Makes it easy to send/Receive data
            stream = new NetworkStream(socket);
            isConnected = true;
            
            // Start a background thread to receive data
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

        }
        catch (Exception ex)
        {
            //Debug.LogError("Error connecting to server: " + ex.Message);
            infoBox.text += "\nError connecting to server: " + ex.Message;
        }
    }

    // Receives data from the server and updates the UI
    void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytesReceived;

        while (isConnected)
        {
            try
            {
                bytesReceived = stream.Read(buffer, 0, buffer.Length);
                if (bytesReceived == 0) break;  // Connection closed

                string msg = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                Debug.Log("Server: " + msg);
                infoBox.text += "\nServer: " + msg;

                if (msg.Contains("PlayerID"))
                {
                    playerId = Convert.ToInt16(msg.Split(':')[1]);
                    DisplayID.text = playerId.ToString();
                    //Debug.Log($"Iam player {playerId}");
                    //infoBox.text += $"\nIam player {playerId}";

                    for (int i = 0; i < playerId - 1; i++)
                    {
                        RunOnMainThread(() => Instantiate(PlayerListPrefab, PlayerList));
                    }
                }
                else if (msg.Contains("updatePlayerList"))
                {
                    RunOnMainThread(() => Instantiate(PlayerListPrefab, PlayerList));
                }
                else if (msg.Contains("Game started"))
                {
                    //should be 1 ? 
                    startGameScreen.SetActive(false);
                    gameContols.SetActive(true);
                    //Debug.Log("Game started! It's player " + message.Split(':')[2] + "'s turn.");
                    //infoBox.text += "------------------------------------------";
                    infoBox.text += "\nGame started! Player " + msg.Split(':')[1] + "'s turn.";

                    if (playerId == Convert.ToInt16(msg.Split(':')[1]))
                    {
                        infoBox.text += "\nIt's My turn :)";
                        foreach (Button btn in gameButtons)
                        {
                            btn.interactable = true;
                        }
                    }

                    playerPosition.transform.position = transform.position;
                }
                else if (msg.Contains("Not your turn"))
                {
                    //Debug.Log(message);
                    infoBox.text += "\n" + msg;
                }
                else if (msg.Contains("No Energy"))
                {
                    //Debug.Log(message);
                    //infoBox.text += "\n"+msg;
                }
                else if (msg.Contains("Waiting for Host"))
                {
                    //Debug.Log(message);
                    infoBox.text += "\n" + msg;
                }
                else if (msg.Contains("CanMove"))
                {
                    infoBox.text += "\n" + msg;
                    if (msg.Split(':')[1] == "up")
                    {
                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position = new Vector3(
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.x,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.y,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.z + Convert.ToInt16(msg.Split(':')[2]));
                    }
                    else if (msg.Split(':')[1] == "down")
                    {
                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position = new Vector3(
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.x,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.y,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.z - Convert.ToInt16(msg.Split(':')[2]));
                    }
                    else if (msg.Split(':')[1] == "left")
                    {
                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position = new Vector3(
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.x - Convert.ToInt16(msg.Split(':')[2]),
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.y,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.z);
                    }
                    else if (msg.Split(':')[1] == "right")
                    {
                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position = new Vector3(
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.x + Convert.ToInt16(msg.Split(':')[2]),
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.y,
                                                                        playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.z);
                    }

                    //infoBox.text += "\n" + msg;
                    // MOVE PLAYER TRANSFORM TO NEW POSITIONS
                    // NEED TESTING
                    //playerPosition.transform.Translate(Vector3.forward * float.Parse(msg.Split(':')[1]) * Time.deltaTime);
                }
                else if (msg.Contains("Next turn"))
                {
                    if (playerId == Convert.ToInt16(msg.Split(':')[1]))
                    {
                        gameContols.SetActive(true);
                        infoBox.text += "\nIt's My turn :)";
                        foreach (Button btn in gameButtons)
                        {
                            btn.interactable = true;
                        }
                    }
                }
                else
                {
                    //Debug.Log(message);
                }





                // Update game by server messages
                // NOT WORKING BUT WORKS IF OUTSIDE ^
                HandleServerMessage(msg);
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
                Console.WriteLine("Error getting data: " + e.Message);
            }
        }
        
    }

    // Handle server messages and update the UI accordingly
    void HandleServerMessage(string message)
    {
        //Debug.Log("REEEEEE5: " + message + ":"+Convert.ToInt16(message.Split(':')[2]));
        //if (message.Contains("PlayerID"))
        //{
        //    playerId = Convert.ToInt16(message.Split(':')[2]);
        //    Debug.Log($"Iam player {playerId}");
        //}
        //else
        //if (message.Contains("Game started"))
        //{
        //    //should be 1 ? 
        //    startGameScreen.SetActive(false);
        //    //Debug.Log("Game started! It's player " + message.Split(':')[2] + "'s turn.");
        //    infoBox.text += "\nGame started! It's player " + message.Split(':')[2] + "'s turn.";
        //    playerPosition.transform.position = transform.position;
        //}
        //else if (message.Contains("Not your turn"))
        //{
        //    //Debug.Log(message);
        //    infoBox.text += "\n"+message;
        //}
        //else if (message.Contains("Waiting for Host"))
        //{
        //    //Debug.Log(message);
        //    infoBox.text += "\n"+message;
        //}
        //else if (message.Contains("CanMove"))
        //{
        //    // MOVE PLAYER TRANSFORM TO NEW POSITIONS
        //    // NEED TESTING
        //    playerPosition.transform.Translate(Vector3.forward * float.Parse(message.Split(':')[1]) * Time.deltaTime);
        //}
        //else
        //{
        //    //Debug.Log(message);
        //}
    }





    // Sends action to server (move, attack, etc.)
    public void SendAction(string action, string target = "")
    {
        if (!isConnected) return;

        string message = $"{playerId}:{action}:{target}";
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
        //Debug.Log("Sent: " + message);
        infoBox.text += "\nSent: " + message;
    }


    // Button for Launch (only Player 1 can launch)
    public void OnLaunchClicked()
    {
        if (playerId == 1)
        {
            SendAction("launch");
        }
        else
        {
            infoBox.text += "\nOnly Player 1 can launch the game.";
            //Debug.Log("Only Player 1 can launch the game.");
        }
    }

    //Movement
    public void goUp()
    {
        SendAction("move", "up");
    }
    
    public void goDown()
    {
        SendAction("move", "down");
    }
    
    public void goLeft()
    {
        SendAction("move", "left");
    }
    
    public void goRight()
    {
        SendAction("move", "right");
    }
    
    // Attack
    public void OnAttackClicked()
    {
        // IMPROVE
        // Just attacks enemy1
        SendAction("attack");
    }
    
    // End turn
    public void endTurn()
    {
        foreach (Button btn in gameButtons)
        {
            btn.interactable = false;
        }
        SendAction("endTurn");
    }

    // Clean up the connection b4 exit
    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }

        if (stream != null)
        {
            stream.Close();
        }

        if (client != null)
        {
            client.Close();
        }
    }
}