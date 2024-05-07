using UnityEngine;

[System.Serializable]
public class Hand
{
    //CardStartingTransform[] cardStartingTransforms = new CardStartingTransform[5];
    [SerializeField] Transform[] cardHandPositions;
    [SerializeField] PlayedCardPosition[] handCardPositions;
    [SerializeField] Card[] cards;

    public CardPosCollection CardCollection => cardCollection;
    [SerializeField] CardPosCollection cardCollection;

    public void Init()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            //card.SetTargetTransform(cardHandPositions[i], true);
            //card.SetTargetTransform(handCardPositions[i].Transform, true);
            //handCardPositions[i].Card = card;

            cardCollection.PlaceCard(card);
            handCardPositions[i].Card = card;
            Debug.Log($"initializing card");
        }
    }

    public void ReturnCard(Card card)
    {
        //ShiftHand();
        CardCollection.PlaceCard(card);
    }

    void ShiftHand()
    {
        int smallestFreeIndex = -1;

        for (int i = 0; i < handCardPositions.Length; i++)
        {
            PlayedCardPosition cardPos = handCardPositions[i];
            if (!cardPos.HasCard && smallestFreeIndex == -1) smallestFreeIndex = i;
            else if (cardPos.HasCard && smallestFreeIndex != -1)
            {
                Card card = cardPos.Card;
                cardPos.Card = null;

                MoveCardToCardPosition(card, handCardPositions[smallestFreeIndex]);
                i = smallestFreeIndex;
                smallestFreeIndex = -1;
            }
        }
    }

    void MoveCardToCardPosition(Card card, PlayedCardPosition cardPos)
    {
        cardPos.Card = card;
        card.SetTargetTransform(cardPos.Transform);
    }

    int GetHighestAvailablePosIndex()
    {
        for (int i = 0; i < handCardPositions.Length; i++)
        {
            if (!handCardPositions[i].HasCard) return i;
        }

        Debug.LogError($"Couldn't find available card position for played card, total card positions: {handCardPositions.Length}. " +
            $"This should not happen, as a player can only have a maximum of 5 cards and 5 positions should be assigned. Returned 0, expect problems.");
        return 0;
    }
}

public struct CardStartingTransform
{
    public readonly Vector3 StartPos;
    public readonly Vector3 StartUp;
    public readonly Vector3 StartForward;

    public CardStartingTransform(Vector3 startPos, Vector3 startUp, Vector3 startForward)
    {
        StartPos = startPos;
        StartUp = startUp;
        StartForward = startForward;
    }
}
