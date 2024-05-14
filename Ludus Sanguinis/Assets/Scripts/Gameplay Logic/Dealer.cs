using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;

public static class Dealer
{
    public static IEnumerator PlayTurn(Table table, Player dealer)
    {
        yield return QOL.GetWaitForSeconds(1f);

        Card card = dealer.CardCollection.CardPositions[0].Card;
        dealer.CardCollection.TakeCard(card);

        table.PlayCard(dealer, card);
    }
}
