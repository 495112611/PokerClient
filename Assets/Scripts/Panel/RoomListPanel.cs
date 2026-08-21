using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    private Text idText;
    private Text beanText;
    private Button createButton;
    private Button refreshButton;
    private Transform content;
    private GameObject roomObj;

    private Text roomIdText;
    public override void OnInit()
    {
        skinPath = "RoomListPanel";
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        //Ѱ�����
        idText = skin.transform.Find("Head/IdText").GetComponent<Text>();
        beanText = skin.transform.Find("Head/BeanText").GetComponent<Text>();
        createButton = skin.transform.Find("Create/CreateButton").GetComponent<Button>();
        refreshButton = skin.transform.Find("Create/RefreshButton").GetComponent<Button>();
        content = skin.transform.Find("RoomList/Scroll View/Viewport/Content");
        roomObj = skin.transform.Find("Room").gameObject;


        roomObj.SetActive(false);
        idText.text = GameManager.id;

        createButton.onClick.AddListener(OnCreateClick);
        refreshButton.onClick.AddListener(OnRefreshClick);


        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);
    }
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
    }
    public void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }
    public void OnRefreshClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }
    public void OnMsgGetAchieve(MsgBase msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        beanText.text = msg.bean.ToString();
    }
    public void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if (msg.result)
        {
            PanelManager.Open<TipPanel>("创建成功");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("创建失败");
        }
    }
    public void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        if (msg.rooms == null)
            return;
        for (int i = 0; i < msg.rooms.Length; i++)
        {
            GenerateRoom(msg.rooms[i]);
        }

    }
    public void OnMsgEnterRoom(MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        if (msg.result)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("进入房间失败");
        }
    }

    public void GenerateRoom(RoomInfo roomInfo)
    {
        GameObject o = Instantiate(roomObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;


        Transform trans = o.transform;
        roomIdText=trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button joinButton = trans.Find("JoinButton").GetComponent<Button>();

        roomIdText.text = roomInfo.id.ToString();
        countText.text = roomInfo.count.ToString();
        if (roomInfo.isPrepare)
        {
            statusText.text = "准备中";
        }
        else
        {
            statusText.text = "未准备";
        }
        joinButton.onClick.AddListener(OnJoinClick);
    }
    public void OnJoinClick()
    {
        MsgEnterRoom msgEnterRoom = new MsgEnterRoom();
        msgEnterRoom.id = int.Parse(roomIdText.text);
        NetManager.Send(msgEnterRoom);
    }
}
