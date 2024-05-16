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

    // Joillain itemeillä joku 30-40% chance käyttää itemi vuorollaan jos on itemi, käyttää random itemin,
    // jos käyttää itemin ja on vielä itemi rollaa uudestaan. Jotkin itemit käytetään omien conditioneiden mukaan

    // Itemien käytön jälkeen pelaa kortit

    // Laskee suurimman ‘turvallisen’ pelattavan arvon: potin capacity - pot amount + pelaajan pienin kortti jos tietää
    // ^ Tekee loot tablen käyville korteille, weight 5 suurimmalla turvallisella, 4 sitä seuraavalla yms. X yritystä valita kortti,
    // lisää kortin pelattavaksi jos ei mene turvallisesta arvosta yli, koska paino ja rajallinen määrä yrityksiä joskus pelaa enemmän ja joskus vähemmän.
}
