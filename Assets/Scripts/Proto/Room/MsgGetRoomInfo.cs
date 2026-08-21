using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgGetRoomInfo : MsgBase
{
    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }
    public PlayerInfo[] players;
}

