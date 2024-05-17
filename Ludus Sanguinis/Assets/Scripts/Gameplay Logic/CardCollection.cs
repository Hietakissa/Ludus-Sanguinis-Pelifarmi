using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardCollection
{
    public CardPosition[] CardPositions => cardPositions;
    [SerializeField] CardPosition[] cardPositions;


    public void InitializeCardTransforms()
    {
        foreach (CardPosition cardPos in cardPositions)
        {
            if (cardPos.HasCard) cardPos.Card.SetTargetTransform(cardPos.Transform);
            else
            {
                Debug.LogError("Tried to initialize card collection but an invalid position was found.");
            }
        }
    }

    public Card TakeCard(Card card, bool shift = true)
    {
        int index = GetPosIndexForCard(card);
        cardPositions[index].Card = null;
        if (shift) Shift();
        return card;
    }

    public void PlaceCard(Card card)
    {
        int index = GetHighestAvailablePosIndex();
        cardPositions[index].Card = card;
        card.SetTargetTransform(cardPositions[index].Transform);
        Shift();
    }

    public void RemoveCards()
    {
        foreach (CardPosition cardPos in CardPositions)
        {
            cardPos.Card = null;
        }
    }

    public Card[] GetCards()
    {
        List<Card> cards = new List<Card>();

        foreach (CardPosition cardPos in cardPositions)
        {
            if (cardPos.HasCard) cards.Add(cardPos.Card);
        }

        return cards.ToArray();
    }

    public List<int> GetCardValues()
    {
        List<int> values = new List<int>();
        foreach (CardPosition cardPos in cardPositions)
        {
            if (cardPos.HasCard) values.Add(cardPos.Card.Value);
        }
        return values;
    }

    public int GetSum()
    {
        int sum = 0;

        for (int i = 0; i < cardPositions.Length; i++)
        {
            CardPosition cardPos = cardPositions[i];
            if (cardPos.HasCard) sum += cardPos.Card.Value;
        }

        return sum;
    }

    public bool IsEmpty()
    {
        foreach (CardPosition cardPos in cardPositions)
        {
            if (cardPos.HasCard) return false;
        }
        return true;
    }


    void Shift()
    {
        int smallestFreeIndex = -1;

        for (int i = 0; i < cardPositions.Length; i++)
        {
            CardPosition cardPos = cardPositions[i];
            if (!cardPos.HasCard && smallestFreeIndex == -1) smallestFreeIndex = i;
            else if (cardPos.HasCard && smallestFreeIndex != -1)
            {
                Card card = cardPos.Card;
                cardPos.Card = null;

                MoveCardToCardPosition(card, cardPositions[smallestFreeIndex]);
                i = smallestFreeIndex;
                smallestFreeIndex = -1;
            }
        }
    }

    void MoveCardToCardPosition(Card card, CardPosition cardPos)
    {
        cardPos.Card = card;
        card.SetTargetTransform(cardPos.Transform);
    }

    int GetHighestAvailablePosIndex()
    {
        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (!cardPositions[i].HasCard) return i;
        }

        Debug.LogError($"Couldn't find available card position for played card, total card positions: {cardPositions.Length}. " +
            $"This should not happen, as a player can only have a maximum of 5 cards and 5 positions should be assigned. Returned 0, expect problems.");
        return 0;
    }
    int GetPosIndexForCard(Card card)
    {
        for (int i = 0; i < cardPositions.Length; i++)
        {
            if (cardPositions[i].HasCard && cardPositions[i].Card == card) return i;
        }

        Debug.LogError($"Could not find pos index for card. Is the card somehow from the wrong player? Returned 0, bugs incoming.");
        return 0;
    }
}

[System.Serializable]
public class CardPosition
{
    [field: SerializeField] public Transform Transform { get; private set; }
    public Card Card;
    public bool HasCard => Card != null;
}
