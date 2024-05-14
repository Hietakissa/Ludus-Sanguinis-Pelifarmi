using UnityEngine;

public class Table : MonoBehaviour
{
    public CardPosCollection PlayerCards => player1CardCollection;
    [SerializeField] CardPosCollection player1CardCollection;

    public CardPosCollection DealerCards => player2CardCollection;
    [SerializeField] CardPosCollection player2CardCollection;

    public Transform PlayerPosHolder => playerPosHolder;
    [SerializeField] Transform playerPosHolder;

    public Transform DealerPosHolder => dealerPosHolder;
    [SerializeField] Transform dealerPosHolder;

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
