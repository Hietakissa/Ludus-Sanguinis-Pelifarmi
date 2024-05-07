using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] PlayedCardPosition[] player1PlayedCardPositions;
    [SerializeField] PlayedCardPosition[] player2PlayedCardPositions;

    [SerializeField] CardPosCollection player1CardCollection;
    [SerializeField] CardPosCollection player2CardCollection;

    public void PlayCard(Player player, Card card)
    {
        // Move played card to the next available position

        //PlayedCardPosition[] playedCardPositions = GetPlayedCardPositionsForPlayer(player);
        //int cardPosIndex = GetHighestAvailablePosIndexForPlayer(player, playedCardPositions);
        //
        //card.SetTargetTransform(playedCardPositions[cardPosIndex].Transform);
        //playedCardPositions[cardPosIndex].Card = card;

        CardPosCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.PlaceCard(card);

        // Then offset the other cards in the hand
        card.State = CardState.OnTable;
        //player.Hand.OffsetHand();
    }

    public void FreeSpotForCard(Player player, Card card)
    {
        CardPosCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.TakeCard(card);

        /*PlayedCardPosition[] playedCardPositions = GetPlayedCardPositionsForPlayer(player);

        for (int i = 0; i < playedCardPositions.Length; i++)
        {
            PlayedCardPosition cardPos = playedCardPositions[i];

            if (card.TargetTransform == cardPos.Transform)
            {
                cardPos.Card = null;
                break;
            }
        }

        ShiftCards(player);*/
    }

    void ShiftCards(Player player)
    {
        PlayedCardPosition[] playedCardPositions = GetPlayedCardPositionsForPlayer(player);
        int smallestFreeIndex = -1;

        for (int i = 0; i < playedCardPositions.Length; i++)
        {
            PlayedCardPosition cardPos = playedCardPositions[i];
            if (!cardPos.HasCard && smallestFreeIndex == -1) smallestFreeIndex = i;
            else if (cardPos.HasCard && smallestFreeIndex != -1)
            {
                Card card = cardPos.Card;
                cardPos.Card = null;

                MoveCardToCardPosition(card, playedCardPositions[smallestFreeIndex]);
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

    PlayedCardPosition[] GetPlayedCardPositionsForPlayer(Player player) => player.IsDealer ? player2PlayedCardPositions : player1PlayedCardPositions;
    CardPosCollection GetCollectionForPlayer(Player player) => player.IsDealer ? player2CardCollection : player1CardCollection;
    int GetHighestAvailablePosIndexForPlayer(Player player, PlayedCardPosition[] playedCardPositions)
    {
        for (int i = 0; i < playedCardPositions.Length; i++)
        {
            if (!playedCardPositions[i].HasCard) return i;
        }

        Debug.LogError($"Couldn't find available card position for played card by '{(player.IsDealer ? "Dealer" : "Player")}', total card positions: {playedCardPositions.Length}. " +
            $"This should not happen, as a player can only have a maximum of 5 cards and 5 positions should be assigned. Returned 0, expect problems.");
        return 0;
    }
}
