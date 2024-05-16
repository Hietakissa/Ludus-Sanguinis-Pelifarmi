using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;

public static class Dealer
{
    const int MAX_VARIATION = 10;

    static int currentVariation;

    static List<int> playerCardValues = new List<int>();

    public static IEnumerator PlayTurn(Table table, Player dealer)
    {
        yield return QOL.GetWaitForSeconds(0.5f);

        int potCapacity = GameManager.Instance.Pot.Capacity;
        int fillAmount = Mathf.Clamp(GameManager.Instance.Pot.FillAmount + Random.Range(-currentVariation, currentVariation), 0, potCapacity);

        int smallestPlayerCardValue = 17;
        for (int i = 0; i < playerCardValues.Count; i++)
        {
            int cardValue = playerCardValues[i];
            if (cardValue < smallestPlayerCardValue) smallestPlayerCardValue = cardValue;
        }
        if (smallestPlayerCardValue == 17) smallestPlayerCardValue = 0;

        int largestSafeValue = potCapacity - fillAmount + smallestPlayerCardValue;


        Card[] cards = dealer.CardCollection.GetCards();


        int count = Random.Range(1, cards.Length + 1);
        for (int i = 0; i < count; i++)
        {
            yield return QOL.GetWaitForSeconds(0.3f);
            Card card = cards[i];
            dealer.CardCollection.TakeCard(card);
            table.PlayCard(dealer, card);
        }


        currentVariation = Mathf.Min(currentVariation + 1, MAX_VARIATION);
    }


    public static void GameEnded()
    {
        playerCardValues.Clear();
    }

    // Joillain itemeill� joku 30-40% chance k�ytt�� itemi vuorollaan jos on itemi, k�ytt�� random itemin,
    // jos k�ytt�� itemin ja on viel� itemi rollaa uudestaan. Jotkin itemit k�ytet��n omien conditioneiden mukaan

    // Itemien k�yt�n j�lkeen pelaa kortit

    // Laskee suurimman �turvallisen� pelattavan arvon: potin capacity - pot amount + pelaajan pienin kortti jos tiet��
    // ^ Tekee loot tablen k�yville korteille, weight 5 suurimmalla turvallisella, 4 sit� seuraavalla yms. X yrityst� valita kortti,
    // lis�� kortin pelattavaksi jos ei mene turvallisesta arvosta yli, koska paino ja rajallinen m��r� yrityksi� joskus pelaa enemm�n ja joskus v�hemm�n.
}
