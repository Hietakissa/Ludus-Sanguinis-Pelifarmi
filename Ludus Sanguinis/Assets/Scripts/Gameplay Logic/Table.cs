using HietakissaUtils.CameraShake;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class Table : MonoBehaviour
{
    public CardCollection PlayerCards => player1CardCollection;
    [SerializeField] CardCollection player1CardCollection;
    public CardCollection DealerCards => player2CardCollection;
    [SerializeField] CardCollection player2CardCollection;

    //public Transform PlayerPosHolder => playerPosHolder;
    //[SerializeField] Transform playerPosHolder;
    //public Transform DealerPosHolder => dealerPosHolder;
    //[SerializeField] Transform dealerPosHolder;

    public ItemCollection PlayerItemCollection => playerItemCollection;
    [SerializeField] ItemCollection playerItemCollection;
    public ItemCollection DealerItemCollection => dealerItemCollection;
    [SerializeField] ItemCollection dealerItemCollection;

    [HideInInspector] public List<Item> PlayerPlayedItems;
    [HideInInspector] public List<Item> DealerPlayedItems;

    [SerializeField] CameraShakeSO playCardShake;

    [SerializeField] TextMeshPro playerValueText;

    [HideInInspector] public Item ItemToSteal;

    public void PlayCard(Player player, Card card)
    {
        CardCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.PlaceCard(card);
        card.State = CardState.OnTable;

        CameraShaker.Instance.Shake(playCardShake);
        EventManager.PlayCard();

        if (!player.IsDealer) UpdatePlayerValueText();
    }

    public IEnumerator PlayItem(Player player, Item item)
    {
        if (!CanPlayerUseItem(player, item)) yield break;

        yield return null;

        // Do item usage animation here

        if (player.IsDealer)
        {
            DealerPlayedItems.Add(item);
            dealerItemCollection.RemoveItem(item);
        }
        else
        {
            PlayerPlayedItems.Add(item);
            PlayerItemCollection.RemoveItem(item);
        }

        Debug.Log($"{(player.IsDealer ? "Dealer" : "Player")} used item of type: '{item.Type}'");


        bool CanPlayerUseItem(Player player, Item item)
        {
            if (player.IsDealer && dealerItemCollection.GetItemCountForItem(item) > 0 && !DealerPlayedItems.Contains(item)) return true;
            else if (playerItemCollection.GetItemCountForItem(item) > 0 && !PlayerPlayedItems.Contains(item)) return true;
            else return false;
        }
    }

    public IEnumerator StealItem(Player target, Item item)
    {
        yield return null;

        // ToDo: some item stealing animation here

        if (target.IsDealer)
        {
            // Stealing from dealer, remove target item, add to player, remove hook
            dealerItemCollection.RemoveItem(item);
            playerItemCollection.AddItem(item.Type);
            PlayerItemCollection.RemoveItem(ItemType.Hook);
        }
        else
        {
            // Stealing from player, remove target item, add to dealer, remove hook
            playerItemCollection.RemoveItem(item);
            dealerItemCollection.AddItem(item.Type);
            DealerItemCollection.RemoveItem(ItemType.Hook);
        }
    }

    public void ClearedTable()
    {
        UpdatePlayerValueText();
    }

    public void FreeSpotForCard(Player player, Card card)
    {
        CardCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.TakeCard(card);

        EventManager.PlayCard();

        if (!player.IsDealer) UpdatePlayerValueText();
    }

    void UpdatePlayerValueText()
    {
        int sum = player1CardCollection.GetSum();
        if (sum == 0) playerValueText.text = "";
        else playerValueText.text = sum.ToString();
    }


    CardCollection GetCollectionForPlayer(Player player) => player.IsDealer ? player2CardCollection : player1CardCollection;
    public ItemCollection GetItemCollectionForPlayer(Player player) => player.IsDealer ? dealerItemCollection : playerItemCollection;
}
