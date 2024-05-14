using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public List<int> PlayedCards = new List<int>();
    public List<int> PlayedItems = new List<int>();

    public bool IsDealer;

    public CardPosCollection CardCollection => cardCollection;
    [SerializeField] CardPosCollection cardCollection;


    public void InitCards(Transform overrideTransform = null)
    {
        for (int i = 0; i < cardCollection.CardPositions.Length; i++)
        {
            Card card = cardCollection.CardPositions[i].Card;
            card.SetTargetTransform(overrideTransform ?? cardCollection.CardPositions[i].Transform);
        }
    }
}
