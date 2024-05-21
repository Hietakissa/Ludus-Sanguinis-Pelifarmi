using System.Collections.Generic;
using HietakissaUtils.LootTable;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public static class Dealer
{
    const int MAX_VARIATION = 10;
    const int ITEM_USE_CHANCE = 30;

    static int currentVariation;

    static List<int> playerCardValues = new List<int>();
    static List<Item> items;

    static Table table;
    static Player dealer;
    static Player player;

    public static IEnumerator PlayTurn(Table _table, Player dealer)
    {
        table = _table;

        List<int> playerValues = table.PlayerCards.GetCardValues();
        Dealer.dealer = dealer;
        player = GameManager.Instance.Player;

        // Remove all items the player played last turn from the memory
        foreach (int value in playerValues) playerCardValues.Remove(value);

        yield return QOL.GetWaitForSeconds(0.5f);


        items = table.DealerItemCollection.GetAvailableItems();
        foreach (Item item in items)
        {
            if (ShouldUseItem(item))
            {
                table.PlayItem(dealer, item);

                if (item.Type == ItemType.Scale) currentVariation = 0;
                else if (item.Type == ItemType.Mirror) playerCardValues = player.CardCollection.GetCardValues();
                else if (item.Type == ItemType.Coin)
                {

                }
            }
        }


        //int potCapacity = GameManager.Instance.Pot.Capacity;
        //int fillAmount = Mathf.Clamp(GameManager.Instance.Pot.FillAmount + Random.Range(-currentVariation, currentVariation), 0, potCapacity);
        //
        //int smallestPlayerCardValue = 17;
        //for (int i = 0; i < playerCardValues.Count; i++)
        //{
        //    int cardValue = playerCardValues[i];
        //    if (cardValue < smallestPlayerCardValue) smallestPlayerCardValue = cardValue;
        //}
        //if (smallestPlayerCardValue == 17) smallestPlayerCardValue = 0;
        //
        //int largestSafeValue = potCapacity - fillAmount + smallestPlayerCardValue;
        //
        //
        //Card[] cards = dealer.CardCollection.GetCards();
        //
        //int count = Random.Range(1, cards.Length + 1);
        //for (int i = 0; i < count; i++)
        //{
        //    yield return QOL.GetWaitForSeconds(0.3f);
        //    Card card = cards[i];
        //    dealer.CardCollection.TakeCard(card);
        //    table.PlayCard(dealer, card);
        //}

        List<Card> cards = GetCardsToPlay();
        for (int i = 0; i < cards.Count; i++)
        {
            yield return QOL.GetWaitForSeconds(0.3f);
            Card card = cards[i];
            dealer.CardCollection.TakeCard(card);
            table.PlayCard(dealer, card);
        }

        //currentVariation = Mathf.Min(currentVariation + 1, MAX_VARIATION);
    }

    static List<Card> GetCardsToPlay()
    {
        // Initial calculations
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
        List<Card> cardsToPlay = new List<Card>();
        Card[] dealerCards = dealer.CardCollection.GetCards();
        dealerCards.Shuffle();

        Debug.Log($"Start of dealer turn. Largest safe value: {largestSafeValue}. Pot: {fillAmount}({GameManager.Instance.Pot.FillAmount})/{potCapacity})");


        // Get safe cards and assemble loot table
        List<Card> safeCards = new List<Card>();
        foreach (CardPosition cardPos in dealer.CardCollection.CardPositions) if (cardPos.HasCard && cardPos.Card.Value <= largestSafeValue) safeCards.Add(cardPos.Card);
        safeCards.Sort((card1, card2) => card2.Value - card1.Value); // Sort by descending


        if (safeCards.Count == 0)
        {
            // No safe cards, play random amount of random cards

            int count = Random.Range(1, dealerCards.Length + 1);
            Debug.Log($"No safe cards, playing {count} cards");
            for (int i = 0; i < count; i++)
            {
                cardsToPlay.Add(dealerCards[i]);
                Debug.Log($"{dealerCards[i].Value}, ®{dealerCards[i].transform.name}®");
            }
        }
        else
        {
            // There are safe cards, play large-ish cards from loot table

            LootTable<Card> lootTable = new LootTable<Card>();
            for (int i = 0; i < safeCards.Count; i++)
            {
                //Debug.Log($"Adding card to loot table value: {safeCards[i].Value}, weight: {5 - i}");
                lootTable.Add(safeCards[i], 5 - i);
            }
            lootTable.BakeTable();


            // Get cards from loot table
            int sum = 0;
            for (int i = 0; i < 30; i++)
            {
                Card card = lootTable.Get();
                if (cardsToPlay.Contains(card)) continue;

                if (sum + card.Value <= largestSafeValue)
                {
                    cardsToPlay.Add(card);
                    sum += card.Value;

                    if (cardsToPlay.Count == dealerCards.Length) break;
                }
            }
        }

        int cardSum = 0;
        foreach (Card card in cardsToPlay) cardSum += card.Value;
        Debug.Log($"Dealer played {cardsToPlay.Count} cards with sum {cardSum}. List:");
        for (int i = 0; i < cardsToPlay.Count; i++)
        {
            Debug.Log($"{cardsToPlay[i].Value}, ®{cardsToPlay[i].transform.name}®");
        }
        return cardsToPlay;
    }

    // Laskee suurimman ëturvallisení pelattavan arvon: potin capacity - pot amount + pelaajan pienin kortti jos tiet‰‰
    // ^ Tekee loot tablen k‰yville korteille, weight 5 suurimmalla turvallisella, 4 sit‰ seuraavalla yms. X yrityst‰ valita kortti,
    // lis‰‰ kortin pelattavaksi jos ei mene turvallisesta arvosta yli, koska paino ja rajallinen m‰‰r‰ yrityksi‰ joskus pelaa enemm‰n ja joskus v‰hemm‰n.

    static bool ShouldUseItem(Item item)
    {
        switch (item.Type)
        {
            case ItemType.Scale: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Mirror: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.UnoCard: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Coin: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Coupon: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Hook: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Heart: return Maf.RandomBool(ITEM_USE_CHANCE);
            default: return false;
        }
    }


    public static void GameEnded()
    {
        playerCardValues.Clear();
    }

    //* scale > use immediately to set scale inaccuracy to 0
    //* handmirror > use immediately to update card memory
    //* uno > in gamemanager after both, if used swap cards (not if both players use it)
    // coin > use immediately to force-play player cards
    // coupon > use immediately to reroll a card
    // hook > play immediately to steal item
    //* heart > play immediately for %chance to dmg


    // Joillain itemeill‰ joku x% chance k‰ytt‰‰ itemi vuorollaan jos on itemi, k‰ytt‰‰ random itemin,
    // jos k‰ytt‰‰ itemin ja on viel‰ itemi rollaa uudestaan. Jotkin itemit k‰ytet‰‰n omien conditioneiden mukaan

    // Itemien k‰ytˆn j‰lkeen pelaa kortit
}
