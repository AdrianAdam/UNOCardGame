using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClick : MonoBehaviour
{
    public GameObject gameController;

    private CardModel cardModel;

    private void Start()
    {
        cardModel = GetComponent<CardModel>();
    }

    private void OnMouseDown()
    {
        gameController.GetComponent<GameController>().PlayCard(cardModel.cardIndex);
    }
}
