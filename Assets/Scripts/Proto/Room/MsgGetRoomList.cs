using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgGetRoomList:MsgBase
{
    public MsgGetRoomList()
    {
        protoName = "MsgGetRoomList";
    }
    public RoomInfo[] rooms;
}
