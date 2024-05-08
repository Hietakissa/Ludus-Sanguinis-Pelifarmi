using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] PlayedCardPosition[] player1PlayedCardPositions;
    [SerializeField] PlayedCardPosition[] player2PlayedCardPositions;

    public CardPosCollection PlayerCards => player1CardCollection;
    [SerializeField] CardPosCollection player1CardCollection;

    public CardPosCollection DealerCards => player2CardCollection;
    [SerializeField] CardPosCollection player2CardCollection;

    public void PlayCard(Player player, Card card)
    {
        CardPosCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.PlaceCard(card);
        card.State = CardState.OnTable;
    }

    public void FreeSpotForCard(Player player, Card card)
    {
        CardPosCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.TakeCard(card);
    }

    CardPosCollection GetCollectionForPlayer(Player player) => player.IsDealer ? player2CardCollection : player1CardCollection;
}
