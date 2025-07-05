using System;

namespace TTB.Assets.Scripts
{
    [Serializable] //for json utility
    public class PlayerCommandSend
    {
        // send server
        public int playerid;
        public string action;
        public string target;
        public string[] extra;
    }
}