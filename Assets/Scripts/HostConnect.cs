using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Diagnostics;
using TTB.Assets.Scripts;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;


public class HostConnect : MonoBehaviour
{
    private TcpClient client;
    private Thread receiveThread;
    private bool isConnected = false;
    string server = "localhost";
    public IPEndPoint remoteEP;
    public Socket socket;
    //private GameObject playerPosition;
    public GameObject startGameScreen;
    public Transform PlayerList;
    public GameObject PlayerListPrefab;
    public TextMeshProUGUI infoBox;
    public TextMeshProUGUI DisplayID;
    public GameObject[] playerPrefabs;
    public GameObject[] enemyPrefabs;
    private static int playerId; //= 1;
    public GameObject gameContols;
    public Button[] gameButtons;
    private int tryConn = 0;
    Process firstProc = new Process();
    public Slider energySlider;
    public Slider healthSlider;
    public Image myIcon;
    public TextMeshProUGUI enemyName;
    public LayerMask clickLayerMask;
    public Image turnIcon;


    // Run things in main thread instead of the Clients
    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private static readonly Queue<string> uiMessages = new Queue<string>();
    readonly object queueLock = new object();

    void Start()
    {
        StartCoroutine(LaunchConnect());
    }

    IEnumerator LaunchConnect()
    {
        //Launch headless Server (with head to test)
        //firstProc.StartInfo.FileName = "C:\\Users\\BlankyCat\\Desktop\\LP1ConsoleTemplate\\RedesSocketServer\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
        firstProc.StartInfo.FileName = ".\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
        firstProc.EnableRaisingEvents = true;
        firstProc.StartInfo.CreateNoWindow = false;
        firstProc.Start();
        yield return new WaitForSeconds(2f);

        while (!isConnected && tryConn < 10)
        {
            try
            {
                ConnectToServer();
            }
            catch
            {
                tryConn++;
            }
            yield return new WaitForSeconds(1f);
        }
    }


    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 30;
        Screen.SetResolution(1280, 720, false);
        Screen.fullScreen = false;
        gameContols.SetActive(false);
        foreach (Button btn in gameButtons)
        {
            btn.interactable = false;
        }
    }

    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue().Invoke();
            }
        }
        lock (queueLock)
        {
            while (uiMessages.Count > 0)
            {
                string message = uiMessages.Dequeue();
                infoBox.text += "\n" + message;
            }
        }

        // Get enemy clicked name
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickLayerMask))
            {
                Transform enemyClick = hit.transform;

                // Go up the hierarchy to get the root GameObject (e.g. Player1)
                while (enemyClick.parent != null)
                {
                    enemyClick = enemyClick.parent;
                }

                string enemyClickName = enemyClick.name;
                ServerMessage("Unit selected: " + enemyClickName);

                if (enemyName != null)
                    enemyName.text = enemyClickName;
            }
        }

        if (enemyName.text == "Enemy Name" || enemyName.text == "")
            gameButtons[4].interactable = false;
        else
            gameButtons[4].interactable = true;
    }
    public static void RunOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }
    void ServerMessage(string message)
    {
        lock (queueLock)
        {
            uiMessages.Enqueue(message);
        }
    }


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

            isConnected = true;

            // Start the thread
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            //infoBox.text += "\nError connecting to server: " + ex.Message;
            ServerMessage("Error connecting to server: " + e.Message);
        }
    }

    // Receives data from the server and updates the UI
    void ReceiveData()
    {
        //infoBox.text += "\nConnected: "+isConnected+"\n";
        //infoBox.text += "aaaaaaConnected, thread starting\n";
        //byte[] buffer = new byte[64];
        //int bytesReceived;


        while (isConnected)
        {
            try
            {
                // VALIDATION OF DATA RECEIVED, if more than should come
                /////////////////////////APLICAR ISTO AO SERVIDOOOOOOOOOOOOOOOOOOR
                byte[] lenBytes = new byte[4];
                int bytesReceived = Receive(lenBytes);
                //string msg1;

                if (bytesReceived == 4)
                {
                    UInt32 commandLen = BitConverter.ToUInt32(lenBytes, 0);

                    var commandBytes = new byte[commandLen];
                    bytesReceived = Receive(commandBytes);


                    //msg1 = Encoding.ASCII.GetString(commandBytes, 0, bytesReceived);
                    //infoBox.text += "\n444Server: " + msg1;
                    //ServerMessage("444Server: " + msg1);
                    //ServerMessage("1234: " + bytesReceived+ " _ " + commandLen);

                    // check if received data equals expected length
                    if (bytesReceived == commandLen)
                    {
                        string msg = Encoding.UTF8.GetString(commandBytes);

                        UnityEngine.Debug.Log("Received Raw: " + msg);

                        try
                        {
                            ServerMessage serverMsg = null;

                            try
                            {
                                serverMsg = JsonUtility.FromJson<ServerMessage>(msg);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Invalid json: " + e);
                                continue;
                            }

                            if (serverMsg != null)
                            {
                                int SrvID = serverMsg.playerid;
                                string SrvAct = serverMsg.action;
                                string SrvMsg = serverMsg.message;
                                string[] SrvExtra = serverMsg.extra;
                                // ServerMessage($"Server Message - Player: {serverMsg.playerId} Action: {serverMsg.action}, Message: {serverMsg.message}");

                                // Handle Server messages
                                if (SrvAct != "")
                                {
                                    switch (SrvAct)
                                    {
                                        case "getid":
                                            AGetID(SrvID);
                                            continue;

                                        case "updateplayerlist":
                                            RunOnMainThread(() => Instantiate(PlayerListPrefab, PlayerList));
                                            continue;

                                        case "gamestart":
                                            AGameStart(SrvID);
                                            continue;

                                        case "canmove":
                                            ACanMove(SrvID, SrvMsg, SrvExtra);
                                            continue;

                                        case "attack":
                                            AAttack(SrvMsg, SrvID, SrvExtra);
                                            continue;

                                        case "killed":
                                            AKilled(SrvMsg, SrvID, SrvExtra);
                                            continue;

                                        case "nextturn":
                                            ANextTurn(SrvID, SrvMsg);
                                            continue;

                                    }
                                }
                                else
                                {
                                    ServerMessage(serverMsg.message);
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            ServerMessage("Error from json received: " + e);
                            throw;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    //infoBox.text += "CATCH: "+e;
                    ServerMessage("CATCH" + e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                //infoBox.text += "Error getting data: " + e.Message;
                ServerMessage("Error getting data: " + e.Message);
            }
        }

    }


    int Receive(byte[] data, bool accountForLittleEndian = true)
    {
        try
        {
            // Normal path - received something
            int nBytes = socket.Receive(data);

            if (accountForLittleEndian && (!BitConverter.IsLittleEndian))
                Array.Reverse(data);

            return nBytes;
        }

        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.WouldBlock)
            {
                return 0; // no receive data, return 0
            }
            else
            {

                // Error => log it
                //Debug.LogError(e);
            }
        }
        return -1; // Return -1 if there's an error
    }




    // Sends action to server (move, attack, etc.)
    public void SendAction(string action = "", string target = "", int playerid = 0, string[] extra = null)
    {
        if (extra == null)
            extra = new string[0];

        if (!isConnected) return;

        PlayerCommandSend msg = new PlayerCommandSend()
        {
            playerid = playerid,
            action = action.ToLower(),
            target = target.ToLower(),
            extra = extra
        };
        UnityEngine.Debug.Log($"SA_ playerId: {msg.playerid}, action: '{msg.action}', target: '{msg.target}', extra: '{msg.extra}'");

        // converter em json
        string json = JsonUtility.ToJson(msg); // using System.Text.Json
        byte[] data = Encoding.UTF8.GetBytes(json);
        byte[] dataLen = BitConverter.GetBytes((UInt32)data.Length);

        UnityEngine.Debug.Log("SA JSON: " + json);

        socket.Send(dataLen);
        socket.Send(data);
        //ServerMessage("Sent: " + json);

        //SEEEEEEEEEEEEEEEND TWICE.. 4 bytes.. then the message.. like receiving
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
            //infoBox.text += "\nOnly Player 1 can launch the game.";
            ServerMessage("Only Player 1 can launch the game.");
        }
    }

    //Movement
    public void goUp()
    {
        SendAction("move", "up", playerId);
    }

    public void goDown()
    {
        SendAction("move", "down", playerId);
    }

    public void goLeft()
    {
        SendAction("move", "left", playerId);
    }

    public void goRight()
    {
        SendAction("move", "right", playerId);
    }

    public void OnAttackClicked()
    {
        SendAction("attack", enemyName.text, playerId);
    }

    public void endTurn()
    {
        SendAction("endturn");
    }

    // Clean up the connection b4 exit
    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();

        if (client != null)
            client.Close();
    }

    public void CleanTextBox()
    {
        infoBox.text = "";
    }

    public void StopServer()
    {
        firstProc.Kill();
    }

    public void AGetID(int newID)
    {
        RunOnMainThread(() =>
        {
            playerId = newID;
            if (playerId == 1)
                myIcon.color = new Color32(56, 56, 136, 255);
            else
                myIcon.color = new Color32(54, 156, 54, 255);

            DisplayID.text = playerId.ToString();
            ServerMessage($"Iam player {playerId}");

            for (int i = 0; i < playerId - 1; i++)
            {
                Instantiate(PlayerListPrefab, PlayerList);
            }
        });
    }

    public void AGameStart(int playerTurn)
    {
        RunOnMainThread(() =>
        {
            CleanTextBox();
            ServerMessage($"Game started! Player {playerTurn} turn.");
            RunOnMainThread(() => {turnIcon.color = new Color32(56, 56, 136, 255);});
            startGameScreen.SetActive(false);
            gameContols.SetActive(true);

            if (playerId == playerTurn)
            {
                ServerMessage("It's My turn :)");
                foreach (Button btn in gameButtons)
                {
                    btn.interactable = true;
                }
            }
        });
    }

    public void ACanMove(int player, string direction, string[] amount)
    {
        int amount0 = Convert.ToInt16(amount[0]);
        Vector3 movement = Vector3.zero;

        switch (direction)
        {
            case "up":
                movement = Vector3.forward * amount0;
                break;

            case "down":
                movement = Vector3.back * amount0;
                break;

            case "left":
                movement = Vector3.left * amount0;
                break;

            case "right":
                movement = Vector3.right * amount0;
                break;

            default:
                movement = Vector3.zero;
                break;
        }
        ChangePlayerEnergyBar(player, Convert.ToInt16(amount[1]));
        RunOnMainThread(() => playerPrefabs[player].transform.position += movement);
    }

    public void AAttack(string unit, int enemID, string[] amount)
    {
        ChangeUnitHPBar(unit, enemID, Convert.ToInt16(amount[0]));
        ChangePlayerEnergyBar(Convert.ToInt16(amount[2]), Convert.ToInt16(amount[1]));
    }

    public void AKilled(string unit, int enemID, string[] amount)
    {
        // Remove HP till 0
        // Animation (fall or just rotate)
        // Remove from Enemy or Player List ...TODO
        ChangeUnitHPBar(unit, enemID, Convert.ToInt16(amount[0]));
        ChangePlayerEnergyBar(Convert.ToInt16(amount[2]), Convert.ToInt16(amount[1]));
        //For now will just rotate
        RunOnMainThread(() =>
        {
            Transform unitTrans = null;

            if (unit == "player")
                unitTrans = playerPrefabs[enemID].transform;
            else if (unit == "enemy")
                unitTrans = enemyPrefabs[enemID].transform;

            unitTrans.rotation = quaternion.Euler(90f, 0f, 0f);
        });
    }


    public void ANextTurn(int nextPlayerID, string playerRefillEnergy)
    {
        //infoBox.text += "\n----- NEW TURN -----";
        //ServerMessage("----- NEW TURN -----");
        ServerMessage($"\n----- Player {nextPlayerID} Turn! -----");
        RunOnMainThread(() =>
        {
            if (nextPlayerID == 1)
                turnIcon.color = new Color32(56, 56, 136, 255);
            else
                turnIcon.color = new Color32(54, 156, 54, 255);
        });

        ChangePlayerEnergyBar(nextPlayerID, Convert.ToInt16(playerRefillEnergy));
        if (playerId == nextPlayerID)
        {
            ServerMessage("It's My turn :)");
            RunOnMainThread(() =>
            {
                gameContols.SetActive(true);
                foreach (Button btn in gameButtons)
                {
                    btn.interactable = true;
                }
            });
        }
    }

    public void ChangePlayerEnergyBar(int playerID, int energyLeft)
    {
        RunOnMainThread(() =>
        {
            
            Slider[] sliders = playerPrefabs[playerID].GetComponentsInChildren<Slider>();
            foreach (Slider s in sliders)
            {
                if (s.name == "Energybar")
                {
                    s.value = energyLeft;
                    if (playerID == playerId)
                    {
                        energySlider.value = energyLeft;
                    }
                }
            }
        });
    }
    
    public void ChangeUnitHPBar(string unit, int playerID, int hpLeft)
    {
        RunOnMainThread(() =>
        {
            Slider[] sliders = null;
            if (unit == "player")
                sliders = playerPrefabs[playerID].GetComponentsInChildren<Slider>();
            else if (unit == "enemy")
                sliders = enemyPrefabs[playerID].GetComponentsInChildren<Slider>();

            foreach (Slider s in sliders)
            {
                if (s.name == "HPbar")
                {
                    s.value = hpLeft;
                    if (playerID == playerId)
                    {
                        healthSlider.value = hpLeft;
                    }
                }
            }
        });
    }
}