using Random = UnityEngine.Random;
using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int MAX_BLOOD_INDEX = 1;

    [SerializeField] Table table;
    public Player player;
    [SerializeField] Player dealer;

    [SerializeField] Transform deckPos;
    [SerializeField] TextMeshPro text;

    bool playerPlayedCards;

    Card[] dealerCardReferences;
    Card[] playerCardReferences;


    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        dealer.InitCards(deckPos);
        player.InitCards(deckPos);

        dealerCardReferences = dealer.CardCollection.GetCards();
        playerCardReferences = player.CardCollection.GetCards();

        SetPlayerCardLock(true);

        foreach (Card card in playerCardReferences)
        {
            player.CardCollection.TakeCard(card, false);
            card.InstaMoveToTarget();
        }
        foreach (Card card in dealerCardReferences)
        {
            dealer.CardCollection.TakeCard(card, false);
            card.InstaMoveToTarget();
        }

        yield return QOL.GetWaitForSeconds(2f);
        Debug.Log($"moving");
        yield return MoveCardsFromDeckToHands();
        Debug.Log($"moved");
        Debug.Log($"waiting");
        yield return QOL.GetWaitForSeconds(1.5f);
        Debug.Log($"waited");
        StartCoroutine(PlayTurn());
    }


#if UNITY_EDITOR
    bool visibleState = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            visibleState = !visibleState;
        }

        bool state = visibleState;


        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
        {
            state = true;
        }


        Cursor.visible = state;
    }
#endif

    void TryEndTurn()
    {
        int playerSum = GetCardSumForCollection(table.PlayerCards);
        bool canEndTurn = table.PlayerCards.GetCards().Length > 0;

        Debug.Log($"Tried to end turn, would have been successful: {canEndTurn}");

        /// thís shouldn't actually be here, but whatever for now. The dealer should play their turn first
        if (canEndTurn)
        {
            //Dealer.PlayTurn(table, dealer);
            //
            //int dealerSum = GetCardSumForCollection(table.DealerCards);
            //int sumDifference = Mathf.Abs(playerSum - dealerSum);
            //
            //text.text = $"{dealerSum}\n" +
            //    $"diff: {sumDifference}\n" +
            //    $"{playerSum}";
            playerPlayedCards = true;
        }
    }

    IEnumerator PlayTurn()
    {
        SetPlayerCardLock(true);
        yield return Dealer.PlayTurn(table, dealer);
        SetPlayerCardLock(false);

        Debug.Log($"Waiting for player's turn");
        while (!playerPlayedCards) yield return null;
        SetPlayerCardLock(true);
        playerPlayedCards = false;

        yield return PlayAnimations();
        yield return GiveCardsAndItems();

        Debug.Log("Turn done");
        StartCoroutine(PlayTurn());
    }

    IEnumerator PlayAnimations()
    {
        // Turn/move camera, move cards to deck etc. here

        Debug.Log($"Playing turn animations");

        table.DealerPosHolder.localScale = Vector3.one;
        foreach (PlayedCardPosition cardPos in table.DealerCards.CardPositions)
        {
            if (cardPos.HasCard) cardPos.Card.SetRevealState(true);
        }
        yield return QOL.GetWaitForSeconds(2f);
        foreach (PlayedCardPosition cardPos in table.DealerCards.CardPositions)
        {
            if (cardPos.HasCard) cardPos.Card.SetRevealState(false);
        }


        int maxCardCount = Mathf.Max(table.PlayerCards.GetCards().Length, table.DealerCards.GetCards().Length);
        for (int i = maxCardCount - 1; i >= 0; i--)
        {
            yield return QOL.GetWaitForSeconds(0.3f);
            Debug.Log($"animating index: {i}");
            PlayedCardPosition playerCardPos = table.PlayerCards.CardPositions[i];
            PlayedCardPosition dealerCardPos = table.DealerCards.CardPositions[i];

            if (playerCardPos.HasCard)
            {
                Card card = table.PlayerCards.TakeCard(playerCardPos.Card);
                card.SetTargetTransform(deckPos);
            }
            if (dealerCardPos.HasCard)
            {
                Card card = table.DealerCards.TakeCard(dealerCardPos.Card);
                card.SetTargetTransform(deckPos);
            }
        }

        table.DealerPosHolder.localScale = new Vector3(1, -1, 1);

        if (player.CardCollection.IsEmpty() || dealer.CardCollection.IsEmpty())
        {
            yield return QOL.GetWaitForSeconds(1.5f);
            yield return MoveCardsFromDeckToHands();
        }
    }

    IEnumerator MoveCardsFromDeckToHands()
    {
        if (player.CardCollection.IsEmpty())
        {
            foreach (Card card in playerCardReferences)
            {
                yield return QOL.GetWaitForSeconds(0.1f);
                card.SetValue(Random.Range(0, 17));
                player.CardCollection.PlaceCard(card);
                card.State = CardState.InHand;
            }
        }

        if (dealer.CardCollection.IsEmpty())
        {
            foreach (Card card in dealerCardReferences)
            {
                yield return QOL.GetWaitForSeconds(0.1f);
                card.SetValue(Random.Range(0, 17));
                dealer.CardCollection.PlaceCard(card);
                card.State = CardState.InHand;
            }
        }
    }

    IEnumerator GiveCardsAndItems()
    {
        // Give both players their new hands (if empty), and items (if new hand)

        Debug.Log($"Giving cards and items");

        yield return null;
    }

    void SetPlayerCardLock(bool locked)
    {
        Debug.Log($"set card lock to: {locked}, player hand cardpositions: {player.CardCollection.CardPositions.Length}");

        foreach (PlayedCardPosition cardPos in table.PlayerCards.CardPositions)
        {
            //Debug.Log($"current: {cardPos.Transform.name}, has card: {cardPos.HasCard}");
            if (cardPos.HasCard) cardPos.Card.IsInteractable = !locked;
        }

        foreach (PlayedCardPosition cardPos in player.CardCollection.CardPositions)
        {
            //Debug.Log($"current: {cardPos.Transform.name}, has card: {cardPos.HasCard}");
            if (cardPos.HasCard) cardPos.Card.IsInteractable = !locked;
        }
    }

    /// start turn
    ///	  (internal) dealer play item(s)
    ///	  dealer play card(s)
    ///	  player play item(s) & card(s)
    /// end turn (ring bell)
    /// animations
    /// give new hand(s) (if applicable)
    /// give item(s) (if applicable)
    /// repeat



    int GetCardSumForCollection(CardPosCollection collection)
    {
        int sum = 0;
        foreach (PlayedCardPosition cardPos in collection.CardPositions)
        {
            if (cardPos.HasCard) sum += cardPos.Card.Value;
        }
        return sum;
    }


    void OnEnable()
    {
        EventManager.OnBellRung += TryEndTurn;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= TryEndTurn;
    }
}

public enum PlayerType
{
    Player,
    Dealer
}
