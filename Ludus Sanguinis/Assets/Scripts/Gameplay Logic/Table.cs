using HietakissaUtils.CameraShake;
using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
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

    [SerializeField] ParticleSystem explosion;
    [SerializeField] SoundContainer explosionSound;

    [SerializeField] TextMeshPro scaleText;
    [SerializeField] Transform deckPos;

    Player dealer;
    public bool HookActive;
    public bool CouponActive;

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

    public void PlayItem(Player player, Item item) => StartCoroutine(PlayItemCor(player, item));

    public IEnumerator PlayItemCor(Player user, Item item)
    {
        if (!CanPlayerUseItem(user, item)) yield break;

        Player opponent = user.IsDealer ? GameManager.Instance.Player : dealer;
        Debug.Log($"{(user.IsDealer ? "Dealer" : "Player")} used item of type: '{item.Type}'");

        explosion.Play();
        //SoundManager.Instance.PlaySoundAtPosition(explosionSound, transform.position);

        // Do item usage animation here


        if (user.IsDealer)
        {
            DealerPlayedItems.Add(item);
            dealerItemCollection.RemoveItem(item);

            // Animations that only happen for the dealer
            switch (item.Type)
            {
                case ItemType.Hook:
                    yield return StealItemCor(opponent, ItemToSteal);
                    break;

                case ItemType.Coupon:
                    Card largestCard = null;
                    int largestValue = -1;

                    foreach (CardPosition cardPos in player2CardCollection.CardPositions)
                    {
                        if (cardPos.HasCard && cardPos.Card.Value > largestValue)
                        {
                            largestCard = cardPos.Card;
                            largestValue = largestCard.Value;
                        }
                    }

                    if (largestCard) yield return RerollCardCor(largestCard);
                    break;
            }
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

                case ItemType.Hook:
                    HookActive = true;
                    break;
            }
        }

        // Animations that are the same for both players
        switch (item.Type)
        {
            case ItemType.Scale:
                // scale anim
                if (user.IsDealer) scaleText.text = "?";
                else scaleText.text = GameManager.Instance.Pot.FillAmount.ToString();
                yield return QOL.GetWaitForSeconds(3f);
                scaleText.text = "";
                break;

            case ItemType.Heart:
                if (Maf.RandomBool(50)) GameManager.Instance.DamagePlayer(user, 1);
                else GameManager.Instance.DamagePlayer(opponent, 1);
                break;

            case ItemType.Coupon:
                CouponActive = true;
                break;
        }


        bool CanPlayerUseItem(Player player, Item item)
        {
            if (player.IsDealer && dealerItemCollection.GetItemCountForItem(item) > 0 && !DealerPlayedItems.Contains(item)) return true;
            else if (GameManager.Instance.IsPlayerTurn && playerItemCollection.GetItemCountForItem(item) > 0 && !PlayerPlayedItems.Contains(item)) return true;
            else return false;
        }
    }

    public void StealItem(Player target, Item item) => StartCoroutine(StealItemCor(target, item));
    public IEnumerator StealItemCor(Player target, Item item)
    {
        Debug.Log($"Stole '{item.Type}' from '{(target.IsDealer ? "Dealer" : "Player")}'");
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
        yield return null;
    }

    public void RerollCard(Card card) => StartCoroutine(RerollCardCor(card));
    IEnumerator RerollCardCor(Card card)
    {
        CardCollection collection;
        if (card.Owner == PlayerType.Player) collection = GameManager.Instance.Player.CardCollection;
        else collection = GameManager.Instance.DealerRef.CardCollection;

        Transform oldTarget = card.TargetTransform;
        collection.TakeCard(card);
        card.SetTargetTransform(deckPos);
        yield return QOL.GetWaitForSeconds(2);

        card.SetValue(Random.Range(0, 17));
        //card.SetTargetTransform(oldTarget);
        collection.PlaceCard(card);
        yield return QOL.GetWaitForSeconds(2);
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

    public void UpdatePlayerValueText()
    {
        int sum = player1CardCollection.GetSum();
        if (sum == 0) playerValueText.text = "";
        else playerValueText.text = sum.ToString();
    }


    CardCollection GetCollectionForPlayer(Player player) => player.IsDealer ? player2CardCollection : player1CardCollection;
    public ItemCollection GetItemCollectionForPlayer(Player player) => player.IsDealer ? dealerItemCollection : playerItemCollection;
}
