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
using System.Diagnostics;

public class CliConnect1 : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;
    string server = "localhost";
    public IPEndPoint remoteEP;
    public Socket socket;
    private GameObject playerPosition;
    public GameObject startGameScreen;
    public Transform PlayerList;
    public GameObject PlayerListPrefab;
    public TextMeshProUGUI infoBox;
    public TextMeshProUGUI DisplayID;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;
    private static int playerId = 1; //IT OVERWRITES everytime for no apparent reason? 
    public GameObject gameContols;
    public Button[] gameButtons;
    private int tryConn = 0;
    

    // Run things in main thread instead of the Clients
    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();
    
    void Start()
    {
        //Launch headless Server (with head to test)
        Process firstProc = new Process();
        firstProc.StartInfo.FileName = ".\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
        firstProc.EnableRaisingEvents = true;
        firstProc.StartInfo.CreateNoWindow = false;
        firstProc.Start();

        // wait 2 secs then connects the "host/player1"
        Thread.Sleep(2000);

        while (!isConnected && tryConn < 10)
        {
            try
            {
                ConnectToServer();
            }
            catch
            {
                tryConn++;
                Thread.Sleep(1000);
            }
        }
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
            
            // Start the thread
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

        }
        catch (Exception ex)
        {
            infoBox.text += "\nError connecting to server: " + ex.Message;
        }
    }

    // Receives data from the server and updates the UI
    void ReceiveData()
    {
        byte[] buffer = new byte[64];
        int bytesReceived;

        while (isConnected)
        {
            try
            {
                bytesReceived = stream.Read(buffer, 0, buffer.Length);
                if (bytesReceived == 0) break;  // Connection closed

                string msg = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                infoBox.text += "\nServer: " + msg;


                // Handle server messages and update game and UI
                if (msg.Contains("PlayerID"))
                {
                    int incomeID = Convert.ToInt16(msg.Split(':')[1]);
                    RunOnMainThread(() =>
                    {
                        playerId = incomeID;
                        DisplayID.text = playerId.ToString();
                        infoBox.text += $"\nIam player {playerId}";

                        for (int i = 0; i < playerId - 1; i++)
                        {
                            Instantiate(PlayerListPrefab, PlayerList);
                        }
                    });
                }
                else if (msg.Contains("updatePlayerList"))
                {
                    infoBox.text += "MY ID: " + playerId;
                    RunOnMainThread(() => Instantiate(PlayerListPrefab, PlayerList));
                }
                else if (msg.Contains("Game started"))
                {
                    startGameScreen.SetActive(false);
                    gameContols.SetActive(true);
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
                    infoBox.text += "\n" + msg;
                }
                else if (msg.Contains("No Energy"))
                {
                    //Already writes the message
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

                }
                else if (msg.Contains("Next turn"))
                {
                    infoBox.text += "\n----- NEW TURN -----";
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
        //foreach (Button btn in gameButtons)
        //{
        //    btn.interactable = false;
        //}
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