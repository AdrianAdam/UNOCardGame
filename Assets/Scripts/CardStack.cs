using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    List<int> cards;

    public bool isGameDeck;

    public event CardEventHandler CardRemoved;
    public event CardEventHandler CardAdded;

    public int CardCount
    {
        get
        {
            if(cards == null)
            {
                return 0;
            }
            else
            {
                return cards.Count;
            }
        }
    }

    public int CardStackCount()
    {
        return cards.Count;
    }

    public IEnumerable<int> GetCards()
    {
        foreach(int i in cards)
        {
            yield return i;
        }
    }

    public int Pop()
    {
        int temp = cards[0];
        cards.RemoveAt(0);

        if(CardRemoved != null)
        {
            CardRemoved(this, new CardEventArgs(temp));
        }

        if(cards.Count == 0 && isGameDeck)
        {
            CreateDeck();
        }

        return temp;
    }

    public void RemoveCard(int cardIndex)
    {
        cards.Remove(cardIndex);

        if (CardRemoved != null)
        {
            CardRemoved(this, new CardEventArgs(cardIndex));
        }
    }

    public int Peek()
    {
        int temp = cards[0];

        return temp;
    }

    public void Push(int card)
    {
        cards.Add(card);

        if(CardAdded != null)
        {
            CardAdded(this, new CardEventArgs(card));
        }
    }

    public void CreateDeck()
    {
        cards.Clear();

        for(int i = 0; i < 72; i++)
        {
            cards.Add(i);
        }

        int n = cards.Count;
        while(n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }
    }

    void Awake()
    {
        cards = new List<int>();
        if(isGameDeck)
        {
            CreateDeck();
        }
    }
}
