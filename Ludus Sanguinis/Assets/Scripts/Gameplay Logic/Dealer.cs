using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public static class Dealer
{
    const int MAX_VARIATION = 8;
    const int ITEM_USE_CHANCE = 30;

    static int currentVariation;

    static List<int> playerCardValues = new List<int>();
    static List<Card> dealerCardsList = new List<Card>();
    static ItemCollection itemCollection;

    static Table table;
    static Player dealer;
    static Player player;

    static int potCapacity;
    static int potInaccurateFillAmount;
    static int potFillAmount;
    static int largestSafeValue;

    static ItemType[] reactiveItemTypes = new ItemType[]{ ItemType.UnoCard, ItemType.Coupon };

    static Item stealItem;
    static int variation;

    public static IEnumerator PlayTurn(Table _table, Player dealer)
    {
        #region Initialization
        table = _table;

        Dealer.dealer = dealer;
        player = GameManager.Instance.Player;

        // Remove all items the player played last turn from the memory
        foreach (int cardValue in GameManager.Instance.lastPlayerPlayedValues) playerCardValues.Remove(cardValue);

        variation = Random.Range(-currentVariation, currentVariation + 1);
        TurnStartInitialCalculations();

        dealerCardsList = dealer.CardCollection.GetCardsList();
        dealerCardsList.Sort((card1, card2) => card2.Value - card1.Value); // Sort by descending, largest cards first
        itemCollection = table.DealerItemCollection;
        #endregion


        Debug.Log($"Start of dealer turn. Largest safe value: {largestSafeValue}. Pot: {potInaccurateFillAmount}({potFillAmount})/{potCapacity})");
        yield return QOL.GetWaitForSeconds(0.5f);


        List<Card> safeCards = GetSafeCardsToPlay();
        if (safeCards.Count == 0)
        {
            // Check to use reactive items
            List<Item> availableReactiveItems = GetReactiveItems();
            if (stealItem)
            {
                Debug.Log($"stealing item after getreactiveitems said so");
                yield return table.StealItemCor(player, stealItem);
                availableReactiveItems.Add(stealItem);
                stealItem = null;
            }


            if (availableReactiveItems.Count == 0)
            {
                // No reactive items, no safe cards, can't steal reactive items

                // Check to use heart
                Item heartItem = itemCollection.GetItem(ItemType.Heart);
                if (itemCollection.GetItemCountForItem(heartItem) > 0) yield return PlayItemCor(heartItem);
                else
                {
                    // No heart, can steal any item?
                    Item hookItem = itemCollection.GetItem(ItemType.Hook);
                    if (itemCollection.GetItemCountForItem(hookItem) > 0)
                    {
                        List<Item> playerItems = table.PlayerItemCollection.GetAvailableItems();
                        if (playerItems.Count > 0)
                        {
                            // Player has an item, steal a random one
                            Item itemToSteal = playerItems.RandomElement();
                            Debug.Log($"stealing and playing item after no safe cards to play and stole item");
                            yield return table.StealItemCor(player, itemToSteal);
                            yield return PlayItemCor(itemToSteal);
                        }
                    }
                }
            }
            else
            {
                Item randomItem = availableReactiveItems.RandomElement();
                availableReactiveItems.Remove(randomItem);
                yield return PlayItemCor(randomItem);
            }

            if (availableReactiveItems.Count > 0)
            {
                // Stole a random non-reactive item to use

                Item itemToPlay = availableReactiveItems.RandomElement();
                availableReactiveItems.Remove(itemToPlay);
                yield return PlayItemCor(itemToPlay);
            }


            List<Card> finalCards = GetFinalCardsToPlay();
            yield return PlayCardsCor(finalCards);
        }
        else yield return PlayCardsCor(safeCards);

        //currentVariation = Mathf.Min(currentVariation + 1, MAX_VARIATION);
    }

    static void TurnStartInitialCalculations()
    {
        potCapacity = GameManager.Instance.Pot.Capacity;
        potFillAmount = GameManager.Instance.Pot.FillAmount;
        potInaccurateFillAmount = Mathf.Clamp(GameManager.Instance.Pot.FillAmount + variation, 0, potCapacity);

        int smallestPlayerCardValue = 17;
        for (int i = 0; i < playerCardValues.Count; i++)
        {
            int cardValue = playerCardValues[i];
            if (cardValue < smallestPlayerCardValue) smallestPlayerCardValue = cardValue;
        }
        if (smallestPlayerCardValue == 17) smallestPlayerCardValue = 0;

        largestSafeValue = potCapacity - potFillAmount + smallestPlayerCardValue;
    }

    // Play highest safe cards?
    // (3, 4, 6, 10) - 40/50 > 10
    // (2, 2, 8) - 40/50 > (8, 2), (8) could be better to save the low value cards
    //
    // If no safe cards
    // (6, 10) - 47/50 > check if items can be used(uno to reverse, coin to force player to play a high value, reroll 10-card, steal one of said items from the player with hook, last resort try to kill with heart attack)
    //
    //
    // Item Types:
    //DP Scale - Passive random, Reactive random - Use when pot is almost full and inaccuracy is high, also override use with the same conditions when there aren’t any safe cards to play
    //DP Mirror - Passive random, Reactive random - Use when no safe cards and inaccuracy is high, also randomly if inaccuracy is very high(>= 9 or 8)
    //DP Uno - Reactive, play-making
    //__ Coupon - Reactive
    //D_ Hook - Reactive
    //DP Heart - Reactive, last resort

    static List<Card> GetSafeCardsToPlay()
    {
        int safeSum = 0;
        List<Card> safeCards = new List<Card>();
        for (int i = 0; i < dealerCardsList.Count; i++)
        {
            Card card = dealerCardsList[i];
            if (safeSum + card.Value <= largestSafeValue)
            {
                safeCards.Add(card);
                safeSum += card.Value;
            }
        }
        return safeCards;
    }

    static List<Card> GetFinalCardsToPlay()
    {
        List<Card> cardsToPlay = new List<Card>();
        
        Item scaleItem = itemCollection.GetItem(ItemType.Scale);
        Item mirrorItem = itemCollection.GetItem(ItemType.Mirror);
        if (table.DealerPlayedItems.Contains(scaleItem) || table.DealerPlayedItems.Contains(mirrorItem))
        {
            // Played scale or mirror, redo calculations
            if (table.DealerPlayedItems.Contains(scaleItem)) variation = 0;
            TurnStartInitialCalculations();

            // Check again for safe cards with updated info
            List<Card> safeCards = GetSafeCardsToPlay();
            if (safeCards.Count > 0) return safeCards;
        }


        Item unoItem = itemCollection.GetItem(ItemType.UnoCard);
        if (table.DealerPlayedItems.Contains(unoItem))
        {
            // Played UNO, play all cards
            cardsToPlay = dealerCardsList;
        }
        else
        {
            // Didn't use uno, play the smallest card and pray we live due to inaccuracies
            Card smallestCard = null;
            foreach (Card card in dealerCardsList)
            {
                if (smallestCard == null) smallestCard = card;
                else if (card.Value < smallestCard.Value) smallestCard = card;
            }
            cardsToPlay.Add(smallestCard);
        }

        return cardsToPlay;
    }

    static List<Item> GetReactiveItems()
    {
        List<Item> availableReactiveItems = itemCollection.GetAvailableItems(reactiveItemTypes);

        if (availableReactiveItems.Count == 0)
        {
            // No reactive items, check if we have hook and the player has some

            Item hookItem = itemCollection.GetItem(ItemType.Hook);
            if (itemCollection.GetItemCountForItem(hookItem) > 0)
            {
                List<Item> playerReactiveItems = table.PlayerItemCollection.GetAvailableItems(reactiveItemTypes);
                if (playerReactiveItems.Count > 0)
                {
                    stealItem = playerReactiveItems.RandomElement();
                    table.ItemToSteal = playerReactiveItems.RandomElement();
                    table.PlayItem(dealer, hookItem);
                }
            }
        }
        else stealItem = null;

        return availableReactiveItems;
    }

    static void PlayCard(Card card)
    {
        dealer.CardCollection.TakeCard(card);
        table.PlayCard(dealer, card);
    }

    static IEnumerator PlayCardsCor(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            yield return QOL.GetWaitForSeconds(0.3f);
            Card card = cards[i];
            PlayCard(card);
        }
    }

    static IEnumerator PlayItemCor(Item item)
    {
        yield return table.PlayItemCor(dealer, item);
    }

    public static void GameEnded()
    {
        playerCardValues.Clear();
    }
}
