using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerStatus
{
    call,
    rob,
    play,
}
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 当前玩家id
    /// </summary>
    public static string id = "";
    /// <summary>
    /// 当前玩家时候房主
    /// </summary>
    public static bool isHost;
    /// <summary>
    /// 层级
    /// </summary>
    private Transform root;
    /// <summary>
    /// 玩家手牌集合
    /// </summary>
    public static List<Card> cards = new List<Card>();
    /// <summary>
    /// 底牌集合
    /// </summary>
    public static List<Card> threeCards = new List<Card>();
    /// <summary>
    /// 玩家状态
    /// </summary>
    public static PlayerStatus status = PlayerStatus.call;
    /// <summary>
    /// 左玩家id
    /// </summary>
    public static string leftId = "";
    /// <summary>
    /// 右玩家id
    /// </summary>
    public static string rightId = "";
    /// <summary>
    /// 左边玩家生成的游戏物体
    /// </summary>
    public static GameObject leftObj;
    /// <summary>
    /// 右边玩家生成的游戏物体
    /// </summary>
    public static GameObject rightObj;
    /// <summary>
    /// 自己玩家生成的游戏物体
    /// </summary>
    public static GameObject playerObj;
    /// <summary>
    /// 是不是地主
    /// </summary>
    public static bool isLandLord = false;
    /// <summary>
    /// 底牌
    /// </summary>
    public static GameObject threeCardsObj;
    /// <summary>
    /// 是否按下鼠标选牌
    /// </summary>
    public static bool isPressing;
    /// <summary>
    /// 选择的手牌
    /// </summary>
    public static List<Card> selectCard = new List<Card>();
    /// <summary>
    /// 允许不出 如果为true那么不出按钮显示 为false不出按钮不显示
    /// </summary>
    public static bool canNotPlay;
    private void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);
        PanelManager.Init();
        PanelManager.Open<LoginPanel>();

        root = GameObject.Find("Root").transform;

        CardManager.Init();

    }
    private void Update()
    {
        NetManager.Update();
    }
    public void OnConnectClose(string err)
    {
        PanelManager.Open<TipPanel>("断开连接");
    }
    public void OnMsgKick(MsgBase msgBase)
    {
        root.GetComponent<BasePanel>().Close();
        PanelManager.Open<TipPanel>("被踢下线");
        PanelManager.Open<LoginPanel>();
    }

    public static void SyncDestroy(string id)
    {
        if (leftId == id)
        {
            for (int i = leftObj.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(leftObj.transform.GetChild(i).gameObject);
            }
        }
        if (rightId == id)
        {
            for (int i = rightObj.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(rightObj.transform.GetChild(i).gameObject);
            }
        }
        if (GameManager.id == id)
        {
            for (int i = playerObj.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(playerObj.transform.GetChild(i).gameObject);
            }
        }
    }
    public static void SyncGenerate(string id, string name)
    {
        GameObject resource = Resources.Load<GameObject>(name);
        if (leftId == id)
        {
            GameObject go = Instantiate(resource, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(leftObj.transform, false);
        }
        if (rightId == id)
        {
            GameObject go = Instantiate(resource, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(rightObj.transform, false);
        }
        if (GameManager.id == id)
        {
            GameObject go = Instantiate(resource, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(playerObj.transform, false);
        }
    }
    public static void SyncGenerateCard(string id, string name)
    {
        name = "Card/" + name;
        Sprite sprite=Resources.Load<Sprite>(name);
        if(leftId == id)
        {
            GameObject go = new GameObject(name);
            Image image = go.AddComponent<Image>();
            image.SetNativeSize();
            go.transform.localScale = new Vector3(0.7f, 0.7f);
            image.sprite = sprite;
            go.transform.SetParent(leftObj.transform, false);
        }
        if(rightId == id)
        {
            GameObject go = new GameObject(name);
            Image image = go.AddComponent<Image>();
            image.SetNativeSize();
            go.transform.localScale = new Vector3(0.7f, 0.7f);
            image.sprite = sprite;
            go.transform.SetParent(rightObj.transform, false);
        }
        if(GameManager.id == id)
        {
            GameObject go = new GameObject(name);
            Image image = go.AddComponent<Image>();
            image.SetNativeSize();
            go.transform.localScale = new Vector3(0.7f, 0.7f);
            image.sprite = sprite;
            go.transform.SetParent(playerObj.transform, false);
        }
    }
    public static void SyncCardCount(string id,int count)
    {
        if (leftId == id)
        {
            Text text = leftObj.transform.parent.Find("CardImage/Text").GetComponent<Text>();
            text.text= (int.Parse(text.text)-count).ToString();
        }
        if (rightId == id)
        {
            Text text = rightObj.transform.parent.Find("CardImage/Text").GetComponent<Text>();
            text.text = (int.Parse(text.text) - count).ToString();
        }
    }
}
