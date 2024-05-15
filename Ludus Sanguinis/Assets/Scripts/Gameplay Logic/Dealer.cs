using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;

public static class Dealer
{
    public static IEnumerator PlayTurn(Table table, Player dealer)
    {
        yield return QOL.GetWaitForSeconds(0.5f);

        Card[] cards = dealer.CardCollection.GetCards();

        //Card card = dealer.CardCollection.CardPositions[0].Card;

        int count = Random.Range(1, cards.Length + 1); // 4 cards, 1 to 4
        for (int i = 0; i < count; i++)
        {
            yield return QOL.GetWaitForSeconds(0.3f);
            Card card = cards[i];
            dealer.CardCollection.TakeCard(card);
            table.PlayCard(dealer, card);
        }
    }
}
