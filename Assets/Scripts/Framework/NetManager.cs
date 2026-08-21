using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public static class NetManager
{
    /// <summary>
    /// 客户端Socket
    /// </summary>
    private static Socket socket;
    /// <summary>
    /// 字节数组
    /// </summary>
    private static ByteArray byteArray;
    /// <summary>
    /// 写入队列
    /// </summary>
    private static Queue<ByteArray> writeQueue;
    /// <summary>
    /// 是否正在连接
    /// </summary>
    private static bool isConnecting;
    /// <summary>
    /// 是否正在关闭
    /// </summary>
    private static bool isClosing;
    /// <summary>
    /// 消息列表
    /// </summary>
    private static List<MsgBase> msgList = new List<MsgBase>();
    /// <summary>
    /// 消息列表长度
    /// </summary>
    private static int msgCount = 0;
    /// <summary>
    /// 一帧处理消息的最大长度
    /// </summary>
    private static int processMsgCount = 10;

    /// <summary>
    /// 是否启用心跳机制
    /// </summary>
    public static bool isUsePing = true;
    /// <summary>
    /// 心跳间隔
    /// </summary>
    public static int pingInterval = 30;
    /// <summary>
    /// 上一次发送Ping协议的时间
    /// </summary>
    private static float lastPingTime = 0;
    /// <summary>
    /// 上一次收到Pong协议的时间
    /// </summary>
    private static float lastPongTime = 0;


    /// <summary>
    /// 连接状态
    /// </summary>
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }
    /// <summary>
    /// 事件委托
    /// </summary>
    /// <param name="err"></param>
    public delegate void EventListener(string err);
    /// <summary>
    /// 事件监听列表
    /// </summary>
    public static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }
    /// <summary>
    /// 删除事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }
    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="err"></param>
    private static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }
    /// <summary>
    /// 事件委托
    /// </summary>
    /// <param name="msgBase"></param>
    public delegate void MsgListener(MsgBase msgBase);
    /// <summary>
    /// 消息事件列表
    /// </summary>
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    /// <summary>
    /// 添加消息事件
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }
    /// <summary>
    /// 删除消息事件
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }
    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="msgBase"></param>
    private static void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }
    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="ip">ip地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("连接失败，已经连接了");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("正在连接");
            return;
        }
        Init();
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }
    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="ar"></param>
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("连接成功");
            isConnecting = false;
            FireEvent(NetEvent.ConnectSucc, "");
            //接收
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("连接失败" + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    private static void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;

        msgList = new List<MsgBase>();
        msgCount = 0;

        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }
    /// <summary>
    /// 关闭
    /// </summary>
    public static void Close()
    {
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        //还有剩余消息
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        //没有剩余消息，直接关闭
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msg">消息</param>
    public static void Send(MsgBase msg)
    {
        //检测
        if (socket == null || !socket.Connected)
            return;
        if (isConnecting)
            return;
        if (isClosing)
            return;
        //编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            //发送
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }
    /// <summary>
    /// 发送回调
    /// </summary>
    /// <param name="ar"></param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if (socket == null || !socket.Connected)
            return;
        int count = socket.EndSend(ar);

        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        ba.readIndex += count;
        if (ba.Length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.First();
            }
        }
        //继续发送
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, 0, SendCallback, socket);
        }
        else if (isClosing)
        {
            socket.Close();
        }
    }
    /// <summary>
    /// 接收回调
    /// </summary>
    /// <param name="ar"></param>
    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            int count = socket.EndSend(ar);
            //断开
            if (count == 0)
            {
                Close();
                return;
            }
            //成功接收
            byteArray.writeIndex += count;
            OnReceiveData();
            //ByteArray长度过小，扩容
            if (byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(byteArray.Length * 2);
            }
            //继续接收消息
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("接收失败" + ex.ToString());
        }
    }
    /// <summary>
    /// 处理接收来的消息
    /// </summary>
    public static void OnReceiveData()
    {
        if (byteArray.Length <= 2)
            return;
        int readIndex = byteArray.readIndex;
        byte[] bytes = byteArray.bytes;
        //bytes[readIndex]%256  bytes[readIndex+1]/256
        short bodyLength = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        if (byteArray.Length < bodyLength + 2)
            return;

        byteArray.readIndex += 2;
        //解析消息名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);

        if (protoName == "")
        {
            Debug.Log("解析失败");
            return;
        }
        byteArray.readIndex += nameCount;
        //解析消息体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyCount);
        byteArray.readIndex += bodyCount;
        byteArray.MoveBytes();
        lock (msgList)
        {
            msgList.Add(msgBase);
        }
        msgCount++;
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }
    /// <summary>
    /// 消息处理，把消息放到msgList当中处理，后续放到Update里面执行
    /// </summary>
    public static void MsgUpdate()
    {
        if (msgCount == 0)
            return;
        for (int i = 0; i < processMsgCount; i++)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgCount > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }
    /// <summary>
    /// 发送Ping协议
    /// </summary>
    private static void PingUpdate()
    {
        if (!isUsePing)
            return;
        //发送消息
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msg = new MsgPing();
            Send(msg);
            lastPingTime = Time.time;
        }

        //pingInterval*4时间内接收不到Pong断开
        if(Time.time-lastPongTime>pingInterval*4)
        {
            Close();
        }
    }
    /// <summary>
    /// 这个Update需要在脚本中调用执行
    /// </summary>
    public static void Update()
    {
        MsgUpdate();
        PingUpdate();
    }
    /// <summary>
    /// 接收到Pong消息后
    /// </summary>
    /// <param name="msgBase"></param>
    private static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }
}
