using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    /// <summary>
    /// 玩家游戏物体
    /// </summary>
    private GameObject playerObj;
    //按钮
    private Button callButton;
    private Button notCallButton;
    private Button robButton;
    private Button notRobButton;
    private Button playButton;
    private Button notPlayButton;
    private Text winText;
    private AudioSource audio;
    public override void OnInit()
    {
        skinPath = "BattlePanel";
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        //寻找组件
        playerObj = skin.transform.Find("Player").gameObject;
        callButton = skin.transform.Find("CallButton").GetComponent<Button>();
        notCallButton = skin.transform.Find("NotCallButton").GetComponent<Button>();
        robButton = skin.transform.Find("RobButton").GetComponent<Button>();
        notRobButton = skin.transform.Find("NotRobButton").GetComponent<Button>();
        playButton = skin.transform.Find("PlayButton").GetComponent<Button>();
        notPlayButton = skin.transform.Find("NotPlayButton").GetComponent<Button>();
        winText= skin.transform.Find("WinPanel/WinText").GetComponent<Text>();
        audio= skin.transform.Find("AudioSource").GetComponent<AudioSource>();

        GameManager.leftObj = skin.transform.Find("LeftPlayer/GameObject").gameObject;
        GameManager.rightObj = skin.transform.Find("RightPlayer/GameObject").gameObject;
        GameManager.playerObj = skin.transform.Find("Player/GameObject").gameObject;
        GameManager.threeCardsObj = skin.transform.Find("ThreeCards").gameObject;


        callButton.gameObject.SetActive(false);
        notRobButton.gameObject.SetActive(false);
        robButton.gameObject.SetActive(false);
        notRobButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        notPlayButton.gameObject.SetActive(false);

        //监听网络事件
        NetManager.AddMsgListener("MsgGetCardList", OnMsgGetCardList);
        NetManager.AddMsgListener("MsgGetStartPlayer", OnMsgGetStartPlayer);
        NetManager.AddMsgListener("MsgSwitchTurn", OnMsgSwitchTurn);
        NetManager.AddMsgListener("MsgGetPlayer", OnMsgGetPlayer);
        NetManager.AddMsgListener("MsgCall", OnMsgCall);
        NetManager.AddMsgListener("MsgReStart", OnMsgReStart);
        NetManager.AddMsgListener("MsgStartRob", OnMsgStartRob);
        NetManager.AddMsgListener("MsgRob", OnMsgRob);
        NetManager.AddMsgListener("MsgPlayCards", OnMsgPlayCards);

        //按钮事件
        callButton.onClick.AddListener(OnCallClick);
        notCallButton.onClick.AddListener(OnNotCallClick);
        robButton.onClick.AddListener(OnRobClick);
        notRobButton.onClick.AddListener(OnNotRobClick);
        playButton.onClick.AddListener(OnPlayClick);
        notPlayButton.onClick.AddListener(OnNotPlayClick);

        MsgGetPlayer msgGetPlayer = new MsgGetPlayer();
        NetManager.Send(msgGetPlayer);

        MsgGetCardList msgGetCardList = new MsgGetCardList();
        NetManager.Send(msgGetCardList);

        MsgGetStartPlayer msgGetStartPlayer = new MsgGetStartPlayer();
        NetManager.Send(msgGetStartPlayer);
    }
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgGetCardList", OnMsgGetCardList);
        NetManager.RemoveMsgListener("MsgGetStartPlayer", OnMsgGetStartPlayer);
        NetManager.RemoveMsgListener("MsgSwitchTurn", OnMsgSwitchTurn);
        NetManager.RemoveMsgListener("MsgGetPlayer", OnMsgGetPlayer);
        NetManager.RemoveMsgListener("MsgCall", OnMsgCall);
        NetManager.RemoveMsgListener("MsgReStart", OnMsgReStart);
        NetManager.RemoveMsgListener("MsgStartRob", OnMsgStartRob);
        NetManager.RemoveMsgListener("MsgRob", OnMsgRob);
        NetManager.RemoveMsgListener("MsgPlayCards", OnMsgPlayCards);
    }
    public void OnMsgGetCardList(MsgBase msgBase)
    {
        MsgGetCardList msg = msgBase as MsgGetCardList;
        for (int i = 0; i < 17; i++)
        {
            Card card = new Card(msg.cardInfos[i].suit, msg.cardInfos[i].rank);
            GameManager.cards.Add(card);
        }


        for (int i = 0; i < 3; i++)
        {
            Card card = new Card(msg.threeCards[i].suit, msg.threeCards[i].rank);
            GameManager.threeCards.Add(card);
        }

        //生成卡牌
        GenerateCard(GameManager.cards.ToArray());
    }
    /// <summary>
    /// 生成卡牌
    /// </summary>
    /// <param name="cards"></param>
    public void GenerateCard(Card[] cards)
    {
        Transform cardTf = playerObj.transform.Find("Cards");
        for (int i = cardTf.childCount - 1; i >= 0; i--)
        {
            Destroy(cardTf.GetChild(i).gameObject);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            string name = CardManager.GetName(cards[i]);
            GameObject cardObj = new GameObject(name);
            cardObj.transform.SetParent(cardTf, false);
            Image image = cardObj.AddComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>("Card/" + name);
            image.sprite = sprite;
            cardObj.layer = LayerMask.NameToLayer("UI");
            cardObj.AddComponent<CardUI>();
        }

        CardSort();
    }
    /// <summary>
    /// 排序
    /// </summary>
    public void CardSort()
    {
        Transform cardsTra = playerObj.transform.Find("Cards");
        for (int i = 1; i < cardsTra.childCount; i++)
        {
            int currentRank = (int)CardManager.GetCard(cardsTra.GetChild(i).name).rank;
            int currentSuit = (int)CardManager.GetCard(cardsTra.GetChild(i).name).suit;
            for (int j = 0; j < i; j++)
            {
                int rank = (int)CardManager.GetCard(cardsTra.GetChild(j).name).rank;
                int suit = (int)CardManager.GetCard(cardsTra.GetChild(j).name).suit;
                if (currentRank > rank)
                {
                    cardsTra.GetChild(i).SetSiblingIndex(j);
                    break;
                }
                else if (currentRank == rank && currentSuit > suit)
                {
                    cardsTra.GetChild(i).SetSiblingIndex(j);
                    break;
                }
            }
        }
    }
    public void OnMsgGetStartPlayer(MsgBase msgBase)
    {
        MsgGetStartPlayer msg = msgBase as MsgGetStartPlayer;
        if (GameManager.id == msg.id)
        {
            callButton.gameObject.SetActive(true);
            notCallButton.gameObject.SetActive(true);
        }
    }

    public void OnCallClick()
    {
        MsgCall msgCall = new MsgCall();
        msgCall.call = true;
        NetManager.Send(msgCall);
    }
    public void OnNotCallClick()
    {
        MsgCall msgCall = new MsgCall();
        msgCall.call = false;
        NetManager.Send(msgCall);
    }
    public void OnRobClick()
    {
        MsgRob msgRob = new MsgRob();
        msgRob.rob = true;
        NetManager.Send(msgRob);
    }
    public void OnNotRobClick()
    {
        MsgRob msgRob = new MsgRob();
        msgRob.rob = false;
        NetManager.Send(msgRob);
    }
    public void OnPlayClick()
    {
        MsgPlayCards msgPlayCards = new MsgPlayCards();
        msgPlayCards.play = true;
        msgPlayCards.cards = CardManager.GetCardInfos(GameManager.selectCard.ToArray());
        NetManager.Send(msgPlayCards);
    }
    public void OnNotPlayClick()
    {
        MsgPlayCards msgPlayCards = new MsgPlayCards();
        msgPlayCards.play = false;
        NetManager.Send(msgPlayCards);
    }
    public void OnMsgSwitchTurn(MsgBase msgBase)
    {
        MsgSwitchTurn msg = msgBase as MsgSwitchTurn;
        switch (GameManager.status)
        {
            case PlayerStatus.call:
                if (msg.id == GameManager.id)
                {
                    callButton.gameObject.SetActive(true);
                    notCallButton.gameObject.SetActive(true);
                }
                else
                {
                    callButton.gameObject.SetActive(false);
                    notCallButton.gameObject.SetActive(false);
                }
                break;
            case PlayerStatus.rob:
                callButton.gameObject.SetActive(false);
                notCallButton.gameObject.SetActive(false);
                if (msg.id == GameManager.id)
                {
                    robButton.gameObject.SetActive(true);
                    notRobButton.gameObject.SetActive(true);
                }
                else
                {
                    robButton.gameObject.SetActive(false);
                    notRobButton.gameObject.SetActive(false);
                }
                break;
            case PlayerStatus.play:
                callButton.gameObject.SetActive(false);
                notCallButton.gameObject.SetActive(false);
                robButton.gameObject.SetActive(false);
                notRobButton.gameObject.SetActive(false);
                if (msg.id == GameManager.id)
                {
                    playButton.gameObject.SetActive(true);
                    notPlayButton.gameObject.SetActive(true);
                    if (GameManager.canNotPlay)
                    {
                        notPlayButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                        notPlayButton.enabled = true;
                    }
                    else
                    {
                        notPlayButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.6f);
                        notPlayButton.enabled = false;
                    }
                }
                else
                {
                    playButton.gameObject.SetActive(false);
                    notPlayButton.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }
    }
    public void OnMsgGetPlayer(MsgBase msgBase)
    {
        MsgGetPlayer msg = msgBase as MsgGetPlayer;
        GameManager.leftId = msg.leftId;
        GameManager.rightId = msg.rightId;
    }
    public void OnMsgCall(MsgBase msgBase)
    {
        MsgCall msg = msgBase as MsgCall;
        MsgSwitchTurn msgSwitchTurn = new MsgSwitchTurn();
        if (msg.call)
        {
            GameManager.SyncDestroy(msg.id);
            GameManager.SyncGenerate(msg.id, "Word/Call");
        }
        else
        {
            GameManager.SyncDestroy(msg.id);
            GameManager.SyncGenerate(msg.id, "Word/NotCall");
        }
        //地主出来了
        if (msg.result == 3)
        {
            SyncLandLord(msg.id);
            RevealCards(GameManager.threeCards.ToArray());
            GameManager.status = PlayerStatus.play;
        }

        if (msg.id != GameManager.id)
            return;
        switch (msg.result)
        {
            case 0:
                break;
            case 1:
                //抢地主
                MsgStartRob msgStartRob = new MsgStartRob();
                NetManager.Send(msgStartRob);
                break;
            case 2:
                //重新洗牌
                MsgReStart msgReStart = new MsgReStart();
                NetManager.Send(msgReStart);
                break;
            case 3:
                //自己是地主
                TurnLandLord();
                msgSwitchTurn.round = 0;
                break;
        }
        NetManager.Send(msgSwitchTurn);
    }
    /// <summary>
    /// 变成地主
    /// </summary>
    public void TurnLandLord()
    {
        GameManager.isLandLord = true;
        GameObject go = Resources.Load<GameObject>("LandLord");
        Sprite sprite = go.GetComponent<SpriteRenderer>().sprite;
        playerObj.transform.Find("Image").GetComponent<Image>().sprite = sprite;

        Card[] cards = new Card[20];
        Array.Copy(GameManager.cards.ToArray(), 0, cards, 0, 17);
        Array.Copy(GameManager.threeCards.ToArray(), 0, cards, 17, 3);

        GenerateCard(cards);
    }

    public void SyncLandLord(string id)
    {
        GameObject go = Resources.Load<GameObject>("LandLord");
        Sprite sprite = go.GetComponent<SpriteRenderer>().sprite;
        if (GameManager.leftId == id)
        {
            GameManager.leftObj.transform.parent.Find("Image").GetComponent<Image>().sprite = sprite;
            Text text = GameManager.leftObj.transform.parent.Find("CardImage/Text").GetComponent<Text>();
            text.text = "20";
        }
        if (GameManager.rightId == id)
        {
            GameManager.rightObj.transform.parent.Find("Image").GetComponent<Image>().sprite = sprite;
            Text text = GameManager.rightObj.transform.parent.Find("CardImage/Text").GetComponent<Text>();
            text.text = "20";
        }
    }

    public void OnMsgReStart(MsgBase msgBase)
    {
        MsgReStart msg = msgBase as MsgReStart;
        Transform cardsTra = playerObj.transform.Find("Cards");
        for (int i = cardsTra.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsTra.GetChild(i).gameObject);
        }
        GameManager.cards.Clear();
        GameManager.threeCards.Clear();
        MsgGetCardList msgGetCardList = new MsgGetCardList();
        NetManager.Send(msgGetCardList);
    }
    public void OnMsgStartRob(MsgBase msgBase)
    {
        MsgStartRob msg = msgBase as MsgStartRob;
        GameManager.status = PlayerStatus.rob;
    }
    public void OnMsgRob(MsgBase msgBase)
    {
        MsgRob msg = msgBase as MsgRob;
        MsgSwitchTurn msgSwitchTurn = new MsgSwitchTurn();

        if (msg.rob)
        {
            //音乐
            string audioPath = "Sounds/Man_Rob";
            audioPath = audioPath + UnityEngine.Random.Range(1, 4);
            audio.clip = Resources.Load<AudioClip>(audioPath);
            audio.Play();

            GameManager.SyncDestroy(msg.id);
            GameManager.SyncGenerate(msg.id, "Word/Rob");
        }
        else
        {
            //音乐
            string audioPath = "Sounds/Man_NoRob";
            audio.clip = Resources.Load<AudioClip>(audioPath);
            audio.Play();

            GameManager.SyncDestroy(msg.id);
            GameManager.SyncGenerate(msg.id, "Word/NotRob");
        }

        SyncLandLord(msg.landLord);

        //地主出来了
        if (msg.landLord != "")
        {
            RevealCards(GameManager.threeCards.ToArray());
            GameManager.status = PlayerStatus.play;
            msgSwitchTurn.round = 0;
        }

        //自己是地主
        if (msg.landLord == GameManager.id)
        {
            TurnLandLord();
        }

        if (msg.id != GameManager.id)
            return;

        if (!msg.needRob)
        {
            msgSwitchTurn.round = 2;
            NetManager.Send(msgSwitchTurn);
            return;
        }
        NetManager.Send(msgSwitchTurn);
    }
    /// <summary>
    /// 揭示底牌
    /// </summary>
    /// <param name="cards"></param>
    public void RevealCards(Card[] cards)
    {
        for (int i = 0; i < 3; i++)
        {
            string name = CardManager.GetName(cards[i]);
            Sprite sprite = Resources.Load<Sprite>("Card/" + name);
            GameManager.threeCardsObj.transform.GetChild(i).GetComponent<Image>().sprite = sprite;

            GameManager.cards.Add(cards[i]);
        }
    }
    public void OnMsgPlayCards(MsgBase msgBase)
    {
        MsgPlayCards msg = msgBase as MsgPlayCards;
        GameManager.canNotPlay = msg.canNotPlay;

        if (msg.win == 2)
        {
            winText.transform.parent.gameObject.SetActive(true);
            if (GameManager.isLandLord)
            {
                winText.text = "地主胜利";
            }
            else
            {
                winText.text = "农民失败";
                winText.color = new(0.4f, 0.4f, 0.4f);
            }
        }
        else if (msg.win == 1)
        {
            winText.transform.parent.gameObject.SetActive(true);
            if (GameManager.isLandLord)
            {
                winText.text = "地主失败";
                winText.color = new(0.4f, 0.4f, 0.4f);
            }
            else
            {
                winText.text = "农民胜利";
            }
        }

        if (msg.result)
        {
            if (msg.play)
            {
                Card[] cards = CardManager.GetCards(msg.cards);
                Array.Sort(cards, (Card card1, Card card2) => (int)card1.rank - (int)card2.rank);
                GameManager.SyncDestroy(msg.id);
                GameManager.SyncCardCount(msg.id, cards.Length);
                //生成同步的卡牌
                for (int i = 0; i < cards.Length; i++)
                {
                    GameManager.SyncGenerateCard(msg.id, CardManager.GetName(cards[i]));
                }
            }
            else
            {
                GameManager.SyncDestroy(msg.id);
                GameManager.SyncGenerate(msg.id, "Word/NotPlay");
            }
        }
        if (GameManager.id != msg.id)
            return;


        if (msg.result)
        {
            if (msg.play)
            {
                Card[] cards = CardManager.GetCards(msg.cards);
                Array.Sort(cards, (Card card1, Card card2) => (int)card1.rank - (int)card2.rank);

                //删除客户端储存的牌
                for (int i = 0; i < cards.Length; i++)
                {
                    for (int j = GameManager.cards.Count - 1; j >= 0; j--)
                    {
                        if (GameManager.cards[j].Equals(cards[i]))
                            GameManager.cards.RemoveAt(j);
                    }
                    for (int j = GameManager.selectCard.Count - 1; j >= 0; j--)
                    {
                        if (GameManager.selectCard[j].Equals(cards[i]))
                            GameManager.selectCard.RemoveAt(j);
                    }
                }
                GenerateCard(GameManager.cards.ToArray());
            }
            MsgSwitchTurn msgSwitchTurn = new MsgSwitchTurn();
            NetManager.Send(msgSwitchTurn);
        }
    }
}
