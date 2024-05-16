using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public static class Dealer
{
    const int MAX_VARIATION = 10;
    const int ITEM_USE_CHANCE = 30;

    static int currentVariation;

    static List<int> playerCardValues;
    static List<Item> items;

    static Table table;


    public static IEnumerator PlayTurn(Table _table, Player dealer)
    {
        table = _table;
        yield return QOL.GetWaitForSeconds(0.5f);

        items = table.DealerItemCollection.GetAvailableItems();
        foreach (Item item in items)
        {
            if (ShouldUseItem(item))
            {
                table.PlayItem(dealer, item);

                if (item.Type == ItemType.Scale) currentVariation = 0;
                else if (item.Type == ItemType.Mirror) playerCardValues = table.PlayerCards.GetCardValues();
            }
        }


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

        Debug.Log($"dealer played turn, listing available items:");
        foreach (ItemSlot slot in table.DealerItemCollection.Slots) if (slot.count > 0) Debug.Log($"{slot.Item.Type}");
    }

    static bool ShouldUseItem(Item item)
    {
        switch (item.Type)
        {
            case ItemType.Scale: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Mirror: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.UnoCard: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Coin: return Maf.RandomBool(ITEM_USE_CHANCE);
            case ItemType.Coupon: return Maf.RandomBool(ITEM_USE_CHANCE);
            //case ItemType.Hook: 
            //    if (HasItem(ref items, ItemType.Heart))
            case ItemType.Heart: return Maf.RandomBool(ITEM_USE_CHANCE);
            default: return false;
        }
    }

    static bool HasItem(ref List<Item> items, ItemType type)
    {
        foreach (Item item in items) if (item.Type == type) return true;
        return false;
    }

    public static void GameEnded()
    {
        playerCardValues.Clear();
    }

    //* scale > use immediately to set scale inaccuracy to 0
    //* handmirror > use immediately to update card memory
    // uno > in gamemanager after both, if used swap cards (not if both players use it)
    // coin > use immediately to force-play player cards
    // coupon > use immediately to reroll a card
    // hook > play immediately to steal item
    // heart > play immediately for %chance to dmg


    // Joillain itemeill‰ joku x% chance k‰ytt‰‰ itemi vuorollaan jos on itemi, k‰ytt‰‰ random itemin,
    // jos k‰ytt‰‰ itemin ja on viel‰ itemi rollaa uudestaan. Jotkin itemit k‰ytet‰‰n omien conditioneiden mukaan

    // Itemien k‰ytˆn j‰lkeen pelaa kortit

    // Laskee suurimman ëturvallisení pelattavan arvon: potin capacity - pot amount + pelaajan pienin kortti jos tiet‰‰
    // ^ Tekee loot tablen k‰yville korteille, weight 5 suurimmalla turvallisella, 4 sit‰ seuraavalla yms. X yrityst‰ valita kortti,
    // lis‰‰ kortin pelattavaksi jos ei mene turvallisesta arvosta yli, koska paino ja rajallinen m‰‰r‰ yrityksi‰ joskus pelaa enemm‰n ja joskus v‰hemm‰n.
}
