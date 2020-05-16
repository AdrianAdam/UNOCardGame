using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardStack))]
public class CardStackView : MonoBehaviour
{
    CardStack cardStack;
    Dictionary<int, GameObject> fetchedCards;
    int lastCount;

    public Vector3 start;
    public GameObject cardPrefab;
    public Dictionary<int, GameObject> cardCopies;
    public float cardOffset;
    public bool isHumanPlayerHand = false;
    public bool reverseLayerOrder = false;

    private void Awake()
    {
        fetchedCards = new Dictionary<int, GameObject>();
        cardCopies = new Dictionary<int, GameObject>();
        cardStack = GetComponent<CardStack>();
    }

    private void Start()
    {
        ShowCards();
        lastCount = cardStack.CardCount;

        cardStack.CardRemoved += CardStack_CardRemoved;
        cardStack.CardAdded += CardStack_CardAdded;
    }

    private void CardStack_CardAdded(object sender, CardEventArgs e)
    {
        float co = cardOffset * cardStack.CardCount;
        Vector3 temp = start + new Vector3(co, 0f);
        AddCard(temp, e.CardIndex, cardStack.CardCount);
    }

    private void CardStack_CardRemoved(object sender, CardEventArgs e)
    {
        if(fetchedCards.ContainsKey(e.CardIndex))
        {
            Destroy(fetchedCards[e.CardIndex]);
            fetchedCards.Remove(e.CardIndex);
        }
    }

    private void Update()
    {
        if(lastCount != cardStack.CardCount)
        {
            lastCount = cardStack.CardCount;
            ShowCards();
        }
    }

    void ShowCards()
    {
        int cardCount = 0;

        foreach(int i in cardStack.GetCards())
        {
            float co = cardOffset * cardCount;

            Vector3 temp = start + new Vector3(co, 0f);

            AddCard(temp, i, cardCount);

            cardCount++;
        }
    }

    public void ArrangeCards(int currentPlayer)
    {
        int cardCount = 0;

        foreach (int i in cardStack.GetCards())
        {
            float co = cardOffset * cardCount;

            Vector3 temp = new Vector3(0f, 0f);
            if(currentPlayer == 0)
            {
                temp = start + new Vector3(co, 0f);
            }
            else
            {
                if (currentPlayer == 1)
                {
                    temp = start + new Vector3(0f, co);
                    cardCopies[i].transform.eulerAngles = new Vector3(0, 0, 90);
                }
                else if(currentPlayer == 2)
                {
                    temp = start - new Vector3(co, 0f);
                    cardCopies[i].transform.eulerAngles = new Vector3(0, 0, 180);
                }
                else if(currentPlayer == 3)
                {
                    temp = start - new Vector3(0f, co);
                    cardCopies[i].transform.eulerAngles = new Vector3(0, 0, 270);
                }
            }

            cardCopies[i].transform.position = temp;

            cardCount++;
        }
    }

    void AddCard(Vector3 position, int cardIndex, int positionalIndex)
    {
        if(fetchedCards.ContainsKey(cardIndex))
        {
            return;
        }
        GameObject cardCopy = (GameObject)Instantiate(cardPrefab);
        cardCopy.transform.position = position;
        cardCopies.Add(cardIndex, cardCopy);

        CardModel cardModel = cardCopy.GetComponent<CardModel>();
        cardModel.cardIndex = cardIndex;
        DetermineCurrentCard(cardModel, cardIndex);
        cardModel.ToggleFace(isHumanPlayerHand);

        SpriteRenderer spriteRenderer = cardCopy.GetComponent<SpriteRenderer>();
        if(reverseLayerOrder)
        {
            spriteRenderer.sortingOrder = 71 - positionalIndex;
        }
        else
        {
            spriteRenderer.sortingOrder = positionalIndex;
        }

        fetchedCards.Add(cardIndex, cardCopy);
    }

    public void DetermineCurrentCard(CardModel cardModel, int cardIndex)
    {
        // 0-9 normal cards (0-9)
        // 10 wait 1 turn
        // 11 swap cards
        // 12 draw 2
        // 13 change color
        // 14 draw 4 and change color

        // Example for card structure: 0-9 normal cards, 10 and 11 wait 1 turn, 12 and 13 swap cards
        // 14 and 15 draw 2 cards. All are red.

        //64 - 67 change color, 68 - 71 draw 4 
        int cardColorIndex = cardIndex / 16;
        int cardID = cardIndex % 16;

        if (cardID == 9)
        {
            cardModel.cardNumber = 0;
        }
        else
        {
            if(cardID < 9)
            {
                cardModel.cardNumber = cardID + 1;
            }
            else if(cardID == 10 || cardID == 11)
            {
                cardModel.cardNumber = 10;
            }
            else if (cardID == 12 || cardID == 13)
            {
                cardModel.cardNumber = 11;
            }
            else if (cardID == 14 || cardID == 15)
            {
                cardModel.cardNumber = 12;
            }
        }

        if (cardColorIndex == 0)
        {
            cardModel.cardColor = "red";
        }
        else if (cardColorIndex == 1)
        {
            cardModel.cardColor = "yellow";
        }
        else if (cardColorIndex == 2)
        {
            cardModel.cardColor = "green";
        }
        else if (cardColorIndex == 3)
        {
            cardModel.cardColor = "blue";
        }
        else if (cardColorIndex == 4)
        {
            cardModel.cardColor = "default";

            if(cardID >= 0 && cardID <= 3)
            {
                cardModel.cardNumber = 13;
            }
            if (cardID >= 4 && cardID <= 7)
            {
                cardModel.cardNumber = 14;
            }
        }
    }

    public bool checkIfCardIsSimilar(CardModel cardModel, int cardIndex, bool specialCard, bool wasCardEffectApplied)
    {
        // 0-9 normal cards (0-9)
        // 10 wait 1 turn
        // 11 swap cards, reverse order of play
        // 12 draw 2
        // 13 change color
        // 14 draw 4 and change color

        // Example for card structure: 0-9 normal cards, 10 and 11 wait 1 turn, 12 and 13 swap cards
        // 14 and 15 draw 2 cards. All are red.

        //64 - 67 change color, 68 - 71 draw 4 
        int cardColorIndex = cardIndex / 16;
        int cardID = cardIndex % 16;

        int cardNumber = -1;
        string cardColor = "";

        if (cardID == 9)
        {
            cardNumber = 0;
        }
        else
        {
            if (cardID < 9)
            {
                cardNumber = cardID + 1;
            }
            else if (cardID == 10 || cardID == 11)
            {
                cardNumber = 10;
            }
            else if (cardID == 12 || cardID == 13)
            {
                cardNumber = 11;
            }
            else if (cardID == 14 || cardID == 15)
            {
                cardNumber = 12;
            }
        }

        if (cardColorIndex == 0)
        {
            cardColor = "red";
        }
        else if (cardColorIndex == 1)
        {
            cardColor = "yellow";
        }
        else if (cardColorIndex == 2)
        {
            cardColor = "green";
        }
        else if (cardColorIndex == 3)
        {
            cardColor = "blue";
        }
        else if (cardColorIndex == 4)
        {
            cardColor = "default";

            if (cardID >= 0 && cardID <= 3)
            {
                cardNumber = 13;
            }
            if (cardID >= 4 && cardID <= 7)
            {
                cardNumber = 14;
            }
        }

        if(specialCard && !wasCardEffectApplied)
        {
            // Check if we have +2 and +2, wait and wait, +4 and +4 OR +2 and +4 OR +4 and +2
            if(cardModel.cardNumber == cardNumber || (cardModel.cardNumber == 12 && cardNumber == 14) || (cardModel.cardNumber == 14 && cardNumber == 12))
            {
                return true;
            }

            return false;
        }

        if(cardColor.Equals("default") || cardModel.cardColor.Equals("default"))
        {
            return true;
        }
        else
        {
            if(cardModel.cardNumber == cardNumber || cardModel.cardColor.Equals(cardColor))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
