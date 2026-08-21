using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CardManager
{
    /// <summary>
    /// 卡牌字典
    /// </summary>
    public static Dictionary<string, Card> nameCards = new Dictionary<string, Card>();
    public static void Init()
    {
        for (int i = 1; i < 5; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card card = new Card(i, j);
                string name = ((Suit)i).ToString() + ((Rank)j).ToString();
                nameCards.Add(name, card);
            }
        }

        nameCards.Add("Sjoker", new Card(Suit.None, Rank.SJoker));
        nameCards.Add("LJoker", new Card(Suit.None, Rank.LJoker));
    }

    public static string GetName(Card card)
    {
        foreach (string name in nameCards.Keys)
        {
            if (nameCards[name].suit == card.suit && nameCards[name].rank == card.rank)
                return name;
        }
        return "";
    }
    public static Card GetCard(string name)
    {
        if(nameCards.ContainsKey(name))
            return nameCards[name];
        return null;
    }/// <summary>
     /// Card数组转CardInfo数组
     /// </summary>
     /// <param name="cards"></param>
     /// <returns></returns>
    public static CardInfo[] GetCardInfos(Card[] cards)
    {
        CardInfo[] infos = new CardInfo[cards.Length];
        for (int i = 0; i < infos.Length; i++)
        {
            infos[i] = cards[i].GetCardInfo();
        }
        return infos;
    }
    public static Card[] GetCards(CardInfo[] cardInfos)
    {
        Card[] cards = new Card[cardInfos.Length];
        for (int i = 0; i < cardInfos.Length; i++)
        {
            cards[i] = new Card(cardInfos[i].suit, cardInfos[i].rank);
        }
        return cards;
    }
}

