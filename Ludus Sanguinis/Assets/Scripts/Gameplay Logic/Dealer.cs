using UnityEngine;

public static class Dealer
{
    public static void PlayTurn(Table table, Player player)
    {
        Card card = player.CardCollection.CardPositions[0].Card;
        player.CardCollection.TakeCard(card);

        table.PlayCard(player, card);
    }
}
