using UnityEngine.SceneManagement;
using System.Collections.Generic;
using HietakissaUtils.LootTable;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int MAX_BLOOD_INDEX = 4;

    public Table Table => table;
    [SerializeField] Table table;
    [SerializeField] Transform deckPos;

    public Player Player;
    public Player DealerRef => dealer;
    [SerializeField] Player dealer;
    Player lastHighestPlayedPlayer;
    bool playerPlayedCards;
    bool playerStoleItem;
    bool isPlayerTurn;
    public List<int> lastPlayerPlayedValues = new List<int>();


    [SerializeField] LootTable<int> normalCardValueTable;
    [SerializeField] LootTable<int> lowCardValueTable;

    public Pot Pot => pot;
    [SerializeField] Pot pot;
    
    Card[] dealerCardReferences;
    Card[] playerCardReferences;


    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        Instance = this;

        table.PlayerItemCollection.Init();
        table.DealerItemCollection.Init();

        normalCardValueTable.BakeTable();
        lowCardValueTable.BakeTable();
    }

    void Start()
    {
        dealer.InitCards(deckPos);
        Player.InitCards(deckPos);

        dealerCardReferences = dealer.CardCollection.GetCards();
        playerCardReferences = Player.CardCollection.GetCards();

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

        TakeCardsFromCollection(Player.CardCollection);
        TakeCardsFromCollection(dealer.CardCollection);
        TakeCardsFromCollection(table.PlayerCards);
        TakeCardsFromCollection(table.DealerCards);
        table.PlayerPlayedItems.Clear();
        table.DealerPlayedItems.Clear();

        // ToDo: uncomment the below lines before release, stops the items from resetting upon starting a game, disabled to allow for cheating in items
        //table.DealerItemCollection.RemoveItems();
        //table.PlayerItemCollection.RemoveItems();


        void TakeCardsFromCollection(CardCollection collection)
        {
            foreach (CardPosition cardPos in collection.CardPositions)
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
        // ToDo: set pot capacity back to 50
        //pot.SetCapacity(50);
        pot.SetCapacity(50);
        Player.Health = 3;
        dealer.Health = 3;

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
        bool canEndTurn = !table.PlayerCards.IsEmpty() && isPlayerTurn;

        if (canEndTurn)
        {
            playerPlayedCards = true;
            Debug.Log($"Ended turn.");
        }
        //else Debug.Log($"Tried to end turn but player cards on table are empty.");
    }

    IEnumerator PlayTurn()
    {
        SetPlayerCardLock(true);
        yield return Dealer.PlayTurn(table, dealer);
        SetPlayerCardLock(false);

        Debug.Log($"Waiting for the player's turn!");
        isPlayerTurn = true;
        while (!playerPlayedCards) yield return null;
        isPlayerTurn = false;
        SetPlayerCardLock(true);
        playerPlayedCards = false;
        lastPlayerPlayedValues = table.PlayerCards.GetCardValues();


        yield return HandleItems();
        yield return PlayAnimations();
        yield return GiveCardsAndItems();

        EndOfRoundCleanup();

        Debug.Log("Turn done");
        StartCoroutine(PlayTurn());



        IEnumerator PlayAnimations()
        {
            // Turn/move camera, move cards to deck etc. here

            Debug.Log($"Playing turn animations");

            foreach (CardPosition cardPos in table.DealerCards.CardPositions)
            {
                if (cardPos.HasCard)
                {
                    yield return QOL.GetWaitForSeconds(0.2f);
                    cardPos.Card.SetRevealState(true);
                    cardPos.Card.Flip();

                    EventManager.DealCard();
                }
            }
            yield return QOL.GetWaitForSeconds(2f);

            int playerSum = table.PlayerCards.GetSum();
            int dealerSum = table.DealerCards.GetSum();
            int sumDifference = Mathf.Abs(playerSum - dealerSum);

            if (playerSum > dealerSum) lastHighestPlayedPlayer = Player;
            else lastHighestPlayedPlayer = dealer;

            yield return pot.AddValue(sumDifference);
            yield return MoveCardsToDeck();
        }
        IEnumerator MoveCardsToDeck()
        {
            int maxCardCount = Mathf.Max(table.PlayerCards.GetCards().Length, table.DealerCards.GetCards().Length);
            for (int i = maxCardCount - 1; i >= 0; i--)
            {
                yield return QOL.GetWaitForSeconds(0.3f);

                CardPosition playerCardPos = table.PlayerCards.CardPositions[i];
                CardPosition dealerCardPos = table.DealerCards.CardPositions[i];

                if (playerCardPos.HasCard || dealerCardPos.HasCard) EventManager.DealCard();
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

                    if (card.IsFlipped) card.Flip();
                }
            }

            table.ClearedTable();
        }
        IEnumerator GiveCardsAndItems()
        {
            // Give both players their new hands (if empty), and items (if new hand)

            Debug.Log($"Giving cards and items");
            if (Player.CardCollection.IsEmpty() || dealer.CardCollection.IsEmpty())
            {
                yield return QOL.GetWaitForSeconds(1.5f);
                yield return MoveCardsFromDeckToHands();
            }
        }
    }

    IEnumerator HandleItems()
    {
        if (HasItem(in table.PlayerPlayedItems, ItemType.UnoCard)) yield return SwapCards(table.PlayerCards, table.DealerCards);
        if (HasItem(in table.DealerPlayedItems, ItemType.UnoCard)) yield return SwapCards(table.PlayerCards, table.DealerCards);

        if (HasItem(in table.PlayerPlayedItems, ItemType.Heart)) yield return UseHeart();
        if (HasItem(in table.DealerPlayedItems, ItemType.Heart)) yield return UseHeart();
    }
    IEnumerator SwapCards(CardCollection collection1, CardCollection collection2)
    {
        Card[] cards1 = collection1.GetCards();
        Card[] cards2 = collection2.GetCards();

        collection1.RemoveCards();
        collection2.RemoveCards();

        int maxLength = Mathf.Max(cards1.Length, cards2.Length);
        for (int i = 0; i < maxLength; i++)
        {
            yield return QOL.GetWaitForSeconds(0.5f);
            if (i < cards1.Length) collection2.PlaceCard(cards1[i]);
            if (i < cards2.Length) collection1.PlaceCard(cards2[i]);
            EventManager.DealCard();
        }

        yield return QOL.GetWaitForSeconds(1f);
    }
    IEnumerator UseHeart()
    {
        bool damagePlayer = Maf.RandomBool(50);
        if (damagePlayer) DamagePlayer(Player, 1);
        else DamagePlayer(dealer, 1);
        yield return null;
    }

    [SerializeField] TextMeshPro debugHealthText;
    void DamagePlayer(Player player, int amount)
    {
        player.Health -= amount;

        debugHealthText.text = $"Player: {player.Health}{(player.IsDealer ? "" : $"(-{amount})")}\n Dealer: {dealer.Health}{(player.IsDealer ? $"(-{amount})" : "")}";
        if (player.Health <= 0)
        {
            //Debug.Log($"Killed {(player.IsDealer ? "Dealer" : "Player")} by dealing {amount} damage. ({player.Health}/3)");
            StartCoroutine(TempDiedThingCor(player));
        }
        //else Debug.Log($"Dealt {amount} damage to {(player.IsDealer ? "Dealer" : "Player")}, Health: {player.Health}/3");



        IEnumerator TempDiedThingCor(Player loser)
        {
            debugHealthText.text = loser.IsDealer ? "You won!" : "You lost :(";
            yield return QOL.GetWaitForSeconds(5);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    IEnumerator MoveCardsFromDeckToHands()
    {
        yield return HandleCollectionForPlayer(Player);
        yield return HandleCollectionForPlayer(dealer);


        IEnumerator HandleCollectionForPlayer(Player player)
        {
            CardCollection collection = player.CardCollection;

            if (collection.IsEmpty())
            {
                Card[] cards = player.IsDealer ? dealerCardReferences : playerCardReferences;
                int[] cardValues = RandomizeNewHandValues();

                for (int i = 0; i < cards.Length; i++)
                {
                    yield return QOL.GetWaitForSeconds(0.1f);

                    Card card = cards[i];
                    card.SetValue(cardValues[i]);
                    card.State = CardState.InHand;

                    collection.PlaceCard(card);
                    EventManager.DealCard();
                }

                if (player.IsDealer) table.DealerItemCollection.AddItem((ItemType)Random.Range(0, 7));
                else table.PlayerItemCollection.AddItem((ItemType)Random.Range(0, 7));
            }
        }
    }

    void EndOfRoundCleanup()
    {
        table.PlayerPlayedItems.Clear();
        table.DealerPlayedItems.Clear();
        playerStoleItem = false;
    }


    void OnPotOverflow(int times)
    {
        DamagePlayer(lastHighestPlayedPlayer, times);
    }

    void SetPlayerCardLock(bool locked)
    {
        //Debug.Log($"set card lock to: {locked}, player hand cardpositions: {Player.CardCollection.CardPositions.Length}");

        foreach (CardPosition cardPos in table.PlayerCards.CardPositions)
        {
            //Debug.Log($"current: {cardPos.Transform.name}, has card: {cardPos.HasCard}");
            if (cardPos.HasCard) cardPos.Card.IsInteractable = !locked;
        }

        foreach (CardPosition cardPos in Player.CardCollection.CardPositions)
        {
            //Debug.Log($"current: {cardPos.Transform.name}, has card: {cardPos.HasCard}");
            if (cardPos.HasCard) cardPos.Card.IsInteractable = !locked;
        }
    }

    int[] RandomizeNewHandValues()
    {
        List<int> values = new List<int>();

        float total = 0f;
        for (int i = 0; i < 3; i++)
        {
            int value = normalCardValueTable.Get();
            values.Add(value);
            total += value;
        }

        const int NUM_OF_CARDS = 17;
        const int MAX_AVERAGE_FOR_LOW_TABLE = 11;
        if (total / NUM_OF_CARDS > MAX_AVERAGE_FOR_LOW_TABLE)
        {
            for (int i = 0; i < 2; i++)
            {
                int value = lowCardValueTable.Get();
                values.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                int value = normalCardValueTable.Get();
                values.Add(value);
            }
        }


        values.Sort();
        return values.ToArray();
    }


    public static bool HasItem(in List<Item> items, ItemType type)
    {
        foreach (Item item in items) if (item.Type == type) return true;
        return false;
    }
    public void TakeItem(Player stealer, Player victim, ItemType itemType)
    {
        table.GetItemCollectionForPlayer(victim).RemoveItem(itemType);
        table.GetItemCollectionForPlayer(stealer).AddItem(itemType);
    }

    // scale > use immediately to set scale inaccuracy to 0
    // handmirror > use immediately to update card memory
    // uno > in gamemanager after both, if used swap cards (not if both players use it)
    // coin > use immediately to force-play player cards
    // coupon > use immediately to reroll a card
    // hook > play immediately to steal item
    // heart > play immediately for %chance to dmg


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
