using System;

namespace TTB.Assets.Scripts
{
    [Serializable]
    public class ServerMessage
    {
        // Receive server
        public int playerid;
        public string action;
        public string message;
        public string[] extra;
    }
}