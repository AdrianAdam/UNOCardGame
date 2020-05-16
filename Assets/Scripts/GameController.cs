using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public CardStack[] players;
    public GameObject[] playersObjects;
    public CardStack deck;

    public Button drawButton;
    public Button playButton;
    public Button drawAgainButton;
    public Button endTurnButton;
    public GameObject currentCard;
    public GameObject blankCard;
    public GameObject currentPlayerArrow;
    public RectTransform panelGameOver;
    public Text winnerText;

    private int currentPlayer;
    private int nextPlayerOrder = 1;
    private int nextPlayerArrowOrder = 90;
    private bool didDraw;
    private CardStackView view;
    private CardStackView[] views;
    private CardModel cardModel;
    private CardModel blankCardModel;
    private CardFlipper cardFlipper;
    private bool bWasCardEffectApplied;
    private bool bWasSpecialCardPlayed;
    private bool bHasCounterPlay;
    private int nCardsToDraw;
    private int nTurnsToWait;
    private Dictionary<int, int> nTurnsToWaitForPlayer;

    void Start()
    {
        drawButton.interactable = true;
        drawAgainButton.interactable = false;
        playButton.interactable = false;
        endTurnButton.interactable = false;
        bWasSpecialCardPlayed = false;
        bHasCounterPlay = false;
        nCardsToDraw = 0;
        nTurnsToWait = 0;

        panelGameOver.gameObject.SetActive(false);

        nTurnsToWaitForPlayer = new Dictionary<int, int>();

        currentPlayer = 0;

        view = deck.GetComponent<CardStackView>();
        cardModel = currentCard.GetComponent<CardModel>();
        blankCardModel = blankCard.GetComponent<CardModel>();

        views = new CardStackView[players.Length];

        for (int i = 0; i < playersObjects.Length; i++)
        {
            views[i] = playersObjects[i].GetComponent<CardStackView>();
        }

        StartGame();

        Invoke("MakeInitialChanges", 0.5f);
    }
    void StartGame()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                players[j].Push(deck.Pop());
            }
        }

        cardModel.cardIndex = deck.Pop();
        cardModel.ToggleFace(true);
        view.DetermineCurrentCard(cardModel, cardModel.cardIndex);
    }

    void MakeInitialChanges()
    {
        for (int i = 0; i < views.Length; i++)
        {
            views[i].ArrangeCards(i);
        }

        // Show cards that can be played
        foreach (int i in players[currentPlayer].GetCards())
        {
            if (view.checkIfCardIsSimilar(cardModel, i, bWasSpecialCardPlayed, bWasCardEffectApplied))
            {
                ShowCardsThatCanBePlayed(i);
                views[currentPlayer].cardCopies[i].AddComponent<BoxCollider2D>();
                drawButton.interactable = false;
            }
        }
    }

    public void DrawCard()
    {
        if (didDraw)
        {
            players[currentPlayer].Push(deck.Pop());
        }

        drawButton.interactable = false;
        playButton.interactable = false;
        drawAgainButton.interactable = true;

        int card = deck.Peek();

        view.DetermineCurrentCard(blankCardModel, card);
        cardFlipper = view.cardCopies[card].GetComponent<CardFlipper>();
        cardFlipper.FlipCard(cardModel.cardBack, cardModel.faces[card], card);

        if (cardModel.cardColor.Equals(blankCardModel.cardColor) || blankCardModel.cardColor.Equals("default") || cardModel.cardColor.Equals("default") || cardModel.cardNumber == blankCardModel.cardNumber)
        {
            playButton.interactable = true;
            drawAgainButton.interactable = false;
            blankCardModel.cardIndex = card;
        }

        didDraw = true;

        views[currentPlayer].ArrangeCards(currentPlayer);
    }

    public void PlayCard(int nCardIndex = -1)
    {
        if (didDraw)
        {
            deck.Pop();
        }
        else
        {
            blankCardModel.cardIndex = nCardIndex;
            players[currentPlayer].RemoveCard(nCardIndex);
        }

        cardModel.cardIndex = blankCardModel.cardIndex;
        view.DetermineCurrentCard(cardModel, cardModel.cardIndex);
        cardModel.ToggleFace(true);

        playButton.interactable = false;
        endTurnButton.interactable = true;
        bWasCardEffectApplied = false;

        views[currentPlayer].ArrangeCards(currentPlayer);

        // Wait 1 turn, +2 or +4
        if(cardModel.cardNumber == 10 || cardModel.cardNumber == 12 || cardModel.cardNumber == 14)
        {
            bWasSpecialCardPlayed = true;

            if(cardModel.cardNumber == 10)
            {
                nTurnsToWait++;
            }
            if(cardModel.cardNumber == 12)
            {
                nCardsToDraw += 2;
            }
            if(cardModel.cardNumber == 14)
            {
                nCardsToDraw += 4;
            }
        }

        if(players[currentPlayer].CardStackCount() == 0)
        {
            panelGameOver.gameObject.SetActive(true);
            winnerText.text = "Player " + currentPlayer + " has won!";
        }

        // TODO: when you have 1 card, make sound "UNO".
        // TODO: when you play a special card, make the specific sound.
    }

    public void EndTurn()
    {
        // Reset cards that can be played.
        foreach (int i in players[currentPlayer].GetCards())
        {
            if (views[currentPlayer].cardCopies[i].transform.position.y != -4)
            {
                ResetCardsThatCanBePlayed(i);
                Destroy(views[currentPlayer].cardCopies[i].GetComponent<BoxCollider2D>());
            }
        }

        views[currentPlayer].ArrangeCards(currentPlayer);

        currentPlayer += nextPlayerOrder;

        currentPlayerArrow.transform.eulerAngles += new UnityEngine.Vector3(0, 0, nextPlayerArrowOrder);

        if (currentPlayer >= players.Length)
        {
            currentPlayer = 0;
        }
        else if (currentPlayer < 0)
        {
            currentPlayer = players.Length - 1;
        }

        didDraw = false;

        drawButton.interactable = true;
        drawAgainButton.interactable = false;
        playButton.interactable = false;
        endTurnButton.interactable = false;

        // Show cards that can be played
        foreach (int i in players[currentPlayer].GetCards())
        {
            if (view.checkIfCardIsSimilar(cardModel, i, bWasSpecialCardPlayed, bWasCardEffectApplied))
            {
                ShowCardsThatCanBePlayed(i);
                views[currentPlayer].cardCopies[i].AddComponent<BoxCollider2D>();
                drawButton.interactable = false;

                if(cardModel.cardNumber != 11)
                {
                    bHasCounterPlay = true;
                }
                else
                {
                    bHasCounterPlay = false;
                }
            }
        }

        bWasSpecialCardPlayed = false;

        if (nTurnsToWaitForPlayer.ContainsKey(currentPlayer))
        {
            nTurnsToWaitForPlayer[currentPlayer]--;

            if(nTurnsToWaitForPlayer[currentPlayer] <= 0)
            {
                nTurnsToWaitForPlayer.Remove(currentPlayer);
            }

            EndTurn();
        }
        
        // Counter the played card
        if (bHasCounterPlay && cardModel.cardNumber != 11)
        {
            bHasCounterPlay = false;
        }
        else if (!bWasCardEffectApplied)
        {
            bWasCardEffectApplied = true;

            // Card 0 means swap all cards, in order of play.
            if(cardModel.cardNumber == 0)
            {
                // TODO: swap cards between all players
            }

            // Card 7 means current player can swap cards with any player he wants.
            if (cardModel.cardNumber == 7)
            {
                // TODO: swap cards between 2 players
            }

            if (cardModel.cardNumber == 10)
            {
                if(nTurnsToWait > 1)
                {
                    nTurnsToWaitForPlayer.Add(currentPlayer, nTurnsToWait - 1);
                }

                nTurnsToWait = 0;

                EndTurn();
            }

            if(cardModel.cardNumber == 11)
            {
                nextPlayerOrder = -nextPlayerOrder;
                nextPlayerArrowOrder = -nextPlayerArrowOrder;

                EndTurn();
            }

            if (cardModel.cardNumber == 12)
            {
                for(int i = 0; i < nCardsToDraw; i++)
                {
                    players[currentPlayer].Push(deck.Pop());
                }

                nCardsToDraw = 0;

                EndTurn();
            }

            if (cardModel.cardNumber == 13)
            {
                // TODO: change color of card.

                currentPlayer += -nextPlayerOrder;
                if(currentPlayer < 0)
                {
                    currentPlayer = players.Length;
                }

                EndTurn();
            }

            if (cardModel.cardNumber == 14)
            {
                for (int i = 0; i < nCardsToDraw; i++)
                {
                    players[currentPlayer].Push(deck.Pop());
                }

                nCardsToDraw = 0;

                // TODO: change color of card.

                EndTurn();
            }
        }
    }

    private void ShowCardsThatCanBePlayed(int cardIndex)
    {
        if (currentPlayer == 0)
        {
            views[currentPlayer].cardCopies[cardIndex].transform.position += new UnityEngine.Vector3(0, 1, 0);
        }
        else
        {
            if(currentPlayer == 1)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position -= new UnityEngine.Vector3(1, 0, 0);
            }
            else if (currentPlayer == 2)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position -= new UnityEngine.Vector3(0, 1, 0);
            }
            else if(currentPlayer == 3)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position += new UnityEngine.Vector3(1, 0, 0);
            }
        }
    }

    private void ResetCardsThatCanBePlayed(int cardIndex)
    {
        if (currentPlayer == 0)
        {
            views[currentPlayer].cardCopies[cardIndex].transform.position -= new UnityEngine.Vector3(0, 1, 0);
        }
        else
        {
            if (currentPlayer == 1)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position += new UnityEngine.Vector3(1, 0, 0);
            }
            else if (currentPlayer == 2)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position += new UnityEngine.Vector3(0, 1, 0);
            }
            else if (currentPlayer == 3)
            {
                views[currentPlayer].cardCopies[cardIndex].transform.position -= new UnityEngine.Vector3(1, 0, 0);
            }
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
    }
}
