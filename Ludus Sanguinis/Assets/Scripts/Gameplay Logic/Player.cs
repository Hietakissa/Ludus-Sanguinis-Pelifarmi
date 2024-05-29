using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public bool IsDealer;

    public int Health;

    public CardCollection CardCollection => cardCollection;
    [SerializeField] CardCollection cardCollection;


    public void InitCards(Transform overrideTransform = null)
    {
        for (int i = 0; i < cardCollection.CardPositions.Length; i++)
        {
            Card card = cardCollection.CardPositions[i].Card;
            card.SetTargetTransform(overrideTransform ?? cardCollection.CardPositions[i].Transform);
        }
    }

    public PlayerType GetPlayerType() => IsDealer ? PlayerType.Dealer : PlayerType.Player;
}
