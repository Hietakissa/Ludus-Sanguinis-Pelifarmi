using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int MAX_BLOOD_INDEX = 1;

    public Table Table => table;
    [SerializeField] Table table;

    public Player player;
    [SerializeField] Player dealer;

    [SerializeField] Transform deckPos;

    bool playerPlayedCards;

    Card[] dealerCardReferences;
    Card[] playerCardReferences;

    [SerializeField] LootTable<int> normalCardValueTable;
    [SerializeField] LootTable<int> lowCardValueTable;

    public Pot Pot => pot;
    [SerializeField] Pot pot;

    Player lastHighestPlayedPlayer;


    void Awake() => Instance = this;

    void Start()
    {
        dealer.InitCards(deckPos);
        player.InitCards(deckPos);

        dealerCardReferences = dealer.CardCollection.GetCards();
        playerCardReferences = player.CardCollection.GetCards();

        InitializeGameState();
    }


    void StartGame() => StartCoroutine(StartGameCor());
    void EndGame()
    {
        Dealer.GameEnded();
        InitializeGameState();
    }

    void InitializeGameState()
    {
        SetPlayerCardLock(true);

        //foreach (Card card in playerCardReferences) HandleCardReset(player.CardCollection.TakeCard(card, false));
        //foreach (Card card in dealerCardReferences) HandleCardReset(dealer.CardCollection.TakeCard(card, false));

        TakeCardsFromCollection(player.CardCollection);
        TakeCardsFromCollection(dealer.CardCollection);
        TakeCardsFromCollection(table.PlayerCards);
        TakeCardsFromCollection(table.DealerCards);



        void TakeCardsFromCollection(CardPosCollection collection)
        {
            foreach (PlayedCardPosition cardPos in collection.CardPositions)
            {
                if (cardPos.HasCard) HandleCardReset(collection.TakeCard(cardPos.Card, false));
            }
        }

        void HandleCardReset(Card card)
        {
            card.SetTargetTransform(deckPos);
            card.InstaMoveToTarget();
        }
    }

    IEnumerator StartGameCor()
    {
        pot.SetCapacity(30);

        yield return QOL.GetWaitForSeconds(2f);
        yield return MoveCardsFromDeckToHands();
        yield return QOL.GetWaitForSeconds(1.5f);

        StartCoroutine(PlayTurn());
    }


#if UNITY_EDITOR
    bool visibleState = false;
#endif
    void Update()
    {
#if UNITY_EDITOR
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
#endif
    }


    void TryEndTurn()
    {
        bool canEndTurn = !table.PlayerCards.IsEmpty();

        if (canEndTurn)
        {
            playerPlayedCards = true;
            Debug.Log($"Ended turn.");
        }
        else Debug.Log($"Tried to end turn but player cards on table are empty.");
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

            int playerSum = table.PlayerCards.GetSum();
            int dealerSum = table.DealerCards.GetSum();
            int sumDifference = Mathf.Abs(playerSum - dealerSum);

            if (playerSum > dealerSum) lastHighestPlayedPlayer = player;
            else lastHighestPlayedPlayer = dealer;

            yield return pot.AddValue(sumDifference);
            yield return MoveCardsToDeck();
            

            table.DealerPosHolder.localScale = new Vector3(1, -1, 1);
        }


        IEnumerator MoveCardsToDeck()
        {
            int maxCardCount = Mathf.Max(table.PlayerCards.GetCards().Length, table.DealerCards.GetCards().Length);
            for (int i = maxCardCount - 1; i >= 0; i--)
            {
                yield return QOL.GetWaitForSeconds(0.3f);

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
                    card.SetRevealState(false);
                }
            }
        }

        IEnumerator GiveCardsAndItems()
        {
            // Give both players their new hands (if empty), and items (if new hand)

            Debug.Log($"Giving cards and items");
            if (player.CardCollection.IsEmpty() || dealer.CardCollection.IsEmpty())
            {
                yield return QOL.GetWaitForSeconds(1.5f);
                yield return MoveCardsFromDeckToHands();
            }
        }
    }

    IEnumerator MoveCardsFromDeckToHands()
    {
        yield return HandleCollectionForPlayer(player);
        yield return HandleCollectionForPlayer(dealer);


        IEnumerator HandleCollectionForPlayer(Player player)
        {
            CardPosCollection collection = player.CardCollection;

            if (collection.IsEmpty())
            {
                Card[] cards = player.IsDealer ? dealerCardReferences : playerCardReferences;
                int[] cardValues = GetCardValues();

                for (int i = 0; i < cards.Length; i++)
                {
                    yield return QOL.GetWaitForSeconds(0.1f);

                    Card card = cards[i];
                    card.SetValue(cardValues[i]);
                    card.State = CardState.InHand;

                    collection.PlaceCard(card);
                }
            }
        }
    }


    void OnPotOverflow(int times)
    {
        Debug.Log($"{(lastHighestPlayedPlayer.IsDealer ? "Dealer" : "Player")} took damage!");
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

    int[] GetCardValues()
    {
        List<int> values = new List<int>();

        float total = 0f;
        for (int i = 0; i < 3; i++)
        {
            int value = normalCardValueTable.GetItem();
            values.Add(value);
            total += value;
        }

        const int NUM_OF_CARDS = 17;
        const int MAX_AVERAGE_FOR_LOW_TABLE = 11;
        if (total / NUM_OF_CARDS > MAX_AVERAGE_FOR_LOW_TABLE)
        {
            for (int i = 0; i < 2; i++)
            {
                int value = lowCardValueTable.GetItem();
                values.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                int value = normalCardValueTable.GetItem();
                values.Add(value);
            }
        }


        values.Sort();
        return values.ToArray();
    }


    void OnEnable()
    {
        EventManager.OnBellRung += TryEndTurn;

        EventManager.OnStartGame += StartGame;
        EventManager.OnEndGame += EndGame;

        EventManager.OnPotOverflow += OnPotOverflow;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= TryEndTurn;

        EventManager.OnStartGame -= StartGame;
        EventManager.OnEndGame -= EndGame;

        EventManager.OnPotOverflow -= OnPotOverflow;
    }
}

public enum PlayerType
{
    Player,
    Dealer
}
