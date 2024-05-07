using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public List<int> PlayedCards = new List<int>();
    public List<int> PlayedItems = new List<int>();

    public bool IsDealer;

    //public Hand Hand;
    public CardPosCollection CardCollection => cardCollection;
    [SerializeField] CardPosCollection cardCollection;
}
