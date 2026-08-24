using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgPlayCards : MsgBase
{
    public MsgPlayCards()
    {
        protoName = "MsgPlayCards";
    }
    public string id = "";
    public bool play;
    public CardInfo[] cards = new CardInfo[20];
    public int cardType;
     public bool result;
    public bool canNotPlay = true;
}
