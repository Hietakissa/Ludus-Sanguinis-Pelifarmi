using HietakissaUtils.CameraShake;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using HietakissaUtils;
using HietakissaUtils.QOL;

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

    [SerializeField] ParticleSystem explosion;
    [SerializeField] SoundContainer explosionSound;

    [SerializeField] TextMeshPro scaleText;

    Player dealer;

    public void PlayCard(Player player, Card card)
    {
        if (player.IsDealer) dealer = player;

        CardCollection cardCollection = GetCollectionForPlayer(player);
        cardCollection.PlaceCard(card);
        card.State = CardState.OnTable;

        CameraShaker.Instance.Shake(playCardShake);
        EventManager.PlayCard();

        if (!player.IsDealer) UpdatePlayerValueText();
    }

    // Items implemented:
    //DP Scale
    //DP Mirror
    //DP Uno
    //__ Coupon
    //D_ Hook
    //DP Heart
    public void PlayItem(Player player, Item item) => StartCoroutine(PlayItemCor(player, item));

    public IEnumerator PlayItemCor(Player player, Item item)
    {
        if (!CanPlayerUseItem(player, item)) yield break;

        explosion.Play();
        SoundManager.Instance.PlaySoundAtPosition(explosionSound, transform.position);

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

            // Animations that only happen for the player
            switch (item.Type)
            {
                case ItemType.Mirror:
                    for (int i = 0; i < dealer.CardCollection.CardPositions.Length; i++)
                    {
                        CardPosition cardPos = dealer.CardCollection.CardPositions[i];
                        if (cardPos.HasCard)
                        {
                            cardPos.Card.Flip();
                            cardPos.Card.SetRevealState(true);
                            yield return QOL.GetWaitForSeconds(0.4f);
                        }
                    }

                    yield return QOL.GetWaitForSeconds(5);

                    for (int i = 0; i < dealer.CardCollection.CardPositions.Length; i++)
                    {
                        CardPosition cardPos = dealer.CardCollection.CardPositions[i];
                        if (cardPos.HasCard)
                        {
                            cardPos.Card.Flip();
                            cardPos.Card.SetRevealState(false);
                            yield return QOL.GetWaitForSeconds(0.2f);
                        }
                    }
                    break;
            }
        }

        // Animations that are the same for both players
        switch (item.Type)
        {
            case ItemType.Scale:
                // scale anim
                if (player.IsDealer) scaleText.text = "?";
                else scaleText.text = GameManager.Instance.Pot.FillAmount.ToString();
                QOL.GetWaitForSeconds(3f);
                scaleText.text = "";
                break;

            case ItemType.Hook: 
                StealItemCor(player.IsDealer ? GameManager.Instance.Player : dealer, ItemToSteal);
                break;
        }

        Debug.Log($"{(player.IsDealer ? "Dealer" : "Player")} used item of type: '{item.Type}'");
        yield return null;


        bool CanPlayerUseItem(Player player, Item item)
        {
            if (player.IsDealer && dealerItemCollection.GetItemCountForItem(item) > 0 && !DealerPlayedItems.Contains(item)) return true;
            else if (playerItemCollection.GetItemCountForItem(item) > 0 && !PlayerPlayedItems.Contains(item)) return true;
            else return false;
        }
    }

    public IEnumerator StealItemCor(Player target, Item item)
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
