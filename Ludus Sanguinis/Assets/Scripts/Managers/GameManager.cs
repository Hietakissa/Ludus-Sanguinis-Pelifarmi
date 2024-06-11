using HietakissaUtils.Serialization;
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
    public const int CONST_BLOOD_DECAL_COUNT = 4;

    public Table Table => table;
    [SerializeField] Table table;
    [SerializeField] Transform deckPos;

    public Player Player;
    public Player DealerRef => dealer;
    [SerializeField] Player dealer;
    Player lastHighestPlayedPlayer;
    bool playerPlayedCards;
    public bool IsPlayerTurn;
    public List<int> lastPlayerPlayedValues = new List<int>();

    [SerializeField] LootTable<int> normalCardValueTable;
    [SerializeField] LootTable<int> lowCardValueTable;
    public LootTable<int> LowCardValueTable => lowCardValueTable;


    public Pot Pot => pot;
    [SerializeField] Pot pot;
    
    Card[] dealerCardReferences;
    Card[] playerCardReferences;

    [SerializeField] bool giveItems;
    [SerializeField] bool skipTutorial;
    public string PlayerName => playerName;
    string playerName;
    [HideInInspector] public bool PlayedTutorial = false;

    public bool IsPaused;
    public int TotalHealth { get; private set; }
    bool isGameRunning;
    float timeWaitedForPlayer;

    [Header("Dealer Ambiance Dialogue")]
    [SerializeField] TextCollectionSO phase1DealerPlaceCard;
    [SerializeField] TextCollectionSO phase1DealerReactToIdle;
    [SerializeField] TextCollectionSO phase2DealerPlaceCard;
    [SerializeField] TextCollectionSO phase2DealerReactToIdle;
    [SerializeField] TextCollectionSO phase3LowDealerPlaceCard;
    [SerializeField] TextCollectionSO phase3LowDealerReactToIdle;
    [SerializeField] TextCollectionSO phase3HighDealerPlaceCard;
    [SerializeField] TextCollectionSO phase3HighDealerReactToIdle;

    void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Instance = this;

        Serializer.Load(out playerName, "PLAYER_NAME");

        PlayedTutorial = false;
#if UNITY_EDITOR
        PlayedTutorial = false;
#else
        //PlayedTutorial = Serializer.Load(out PlayedTutorial, "TUTORIAL_PLAYED");
#endif

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
        EventManager.PlayerDamaged(null, 6);

        Dealer.GameEnded();
        InitializeGameState();
    }

    void InitializeGameState()
    {
        SetPlayerCardLock(true);

        pot.SetCapacity(50);
        Player.Health = 3;
        dealer.Health = 3;

        EventManager.PlayerDamaged(Player, Player.Health, true);
        EventManager.PlayerDamaged(dealer, dealer.Health, true);

        TakeCardsFromCollection(Player.CardCollection);
        TakeCardsFromCollection(dealer.CardCollection);
        TakeCardsFromCollection(table.PlayerCards);
        TakeCardsFromCollection(table.DealerCards);
        table.PlayerPlayedItems.Clear();
        table.DealerPlayedItems.Clear();

        // ToDo: uncomment the below lines before release, stops the items from resetting upon starting a game, disabled to allow for cheating in items
        table.DealerItemCollection.RemoveItems();
        table.PlayerItemCollection.RemoveItems();

#if UNITY_EDITOR
        debugHealthText.text = "";
#endif


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
        if (string.IsNullOrEmpty(playerName) || !skipTutorial)
        {
            yield return UIManager.Instance.TutorialSequenceCor();
            PlayedTutorial = true;
        }
        
        yield return QOL.WaitForSeconds.Get(2f);
        yield return MoveCardsFromDeckToHands();
        SetPlayerCardLock(true);
        yield return QOL.WaitForSeconds.Get(1.5f);

        isGameRunning = true;
        StartCoroutine(PlayTurn());
    }


//#if UNITY_EDITOR
//    bool visibleState = false;
//#endif
//    void Update()
//    {
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.T))
//        {
//            visibleState = !visibleState;
//        }
//
//        Vector3 mousePos = Input.mousePosition;
//        if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
//        {
//            visibleState = true;
//        }
//
//        Cursor.visible = visibleState;
//#endif
//    }


    void TryEndTurn()
    {
        bool canEndTurn = !table.PlayerCards.IsEmpty() && IsPlayerTurn;

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

        int phase = 3 - dealer.Health + 1;
        int dealerHealth = dealer.Health;
        // Play dealer ambiance dialogue
        if (Maf.RandomBool(10))
        {
            if (phase == 1) UIManager.Instance.PlayDialogue(phase1DealerPlaceCard);
            else if (phase == 2) UIManager.Instance.PlayDialogue(phase2DealerPlaceCard);
            else if (phase == 3)
            {
                if (dealerHealth == 1) UIManager.Instance.PlayDialogue(phase3LowDealerPlaceCard);
                else UIManager.Instance.PlayDialogue(phase3HighDealerPlaceCard);
            }
        }

        QOL.Log($"Waiting for the player's turn!");
        IsPlayerTurn = true;
        while (!playerPlayedCards)
        {
            const float CONST_MAX_IDLE_TIME = 30f;
            timeWaitedForPlayer += Time.deltaTime;

            if (timeWaitedForPlayer >= CONST_MAX_IDLE_TIME)
            {
                timeWaitedForPlayer -= CONST_MAX_IDLE_TIME;
                if (phase == 1) UIManager.Instance.PlayDialogue(phase1DealerReactToIdle);
                else if (phase == 2) UIManager.Instance.PlayDialogue(phase1DealerReactToIdle);
                else if (phase == 3)
                {
                    if (dealerHealth == 1) UIManager.Instance.PlayDialogue(phase3LowDealerReactToIdle);
                    else UIManager.Instance.PlayDialogue(phase3HighDealerReactToIdle);
                }
            }
            yield return null;
        }
        timeWaitedForPlayer = 0f;
        table.HookActive = false;
        table.CouponActive = false;
        IsPlayerTurn = false;
        SetPlayerCardLock(true);
        playerPlayedCards = false;
        lastPlayerPlayedValues = table.PlayerCards.GetCardValues();


        yield return HandleItems();
        yield return PlayAnimations();

        if (isGameRunning)
        {
            yield return GiveCardsAndItems();
            StartCoroutine(PlayTurn());
        }

        EndOfRoundCleanup();

        QOL.Log("Turn done");



        IEnumerator PlayAnimations()
        {
            // Turn/move camera, move cards to deck etc. here

            Debug.Log($"Playing turn animations");

            foreach (CardPosition cardPos in table.DealerCards.CardPositions)
            {
                if (cardPos.HasCard)
                {
                    yield return QOL.WaitForSeconds.Get(0.2f);
                    cardPos.Card.SetRevealState(true);
                    cardPos.Card.Flip();

                    EventManager.DealCard();
                }
            }
            yield return QOL.WaitForSeconds.Get(2f);

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
                yield return QOL.WaitForSeconds.Get(0.3f);

                CardPosition playerCardPos = table.PlayerCards.CardPositions[i];
                CardPosition dealerCardPos = table.DealerCards.CardPositions[i];

                if (playerCardPos.HasCard || dealerCardPos.HasCard) EventManager.DealCard();
                if (playerCardPos.HasCard)
                {
                    Card card = table.PlayerCards.TakeCard(playerCardPos.Card);
                    card.SetTargetTransform(deckPos);
                    card.IsInteractable = false;
                }
                if (dealerCardPos.HasCard)
                {
                    Card card = table.DealerCards.TakeCard(dealerCardPos.Card);
                    card.SetTargetTransform(deckPos);
                    card.SetRevealState(false);
                    card.IsInteractable = false;

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
                yield return QOL.WaitForSeconds.Get(1.5f);
                yield return MoveCardsFromDeckToHands();
            }
        }
    }

    IEnumerator HandleItems()
    {
        int swapCount = 0;
        bool playerHas = false;
        bool dealerHas = false;
        if (HasItem(in table.PlayerPlayedItems, ItemType.UnoCard))
        {
            playerHas = true;
            swapCount++;
        }
        if (HasItem(in table.DealerPlayedItems, ItemType.UnoCard))
        {
            dealerHas = true;
            swapCount++;
        }

        for (int i = 0; i < swapCount; i++)
        {
            if (playerHas)
            {
                yield return table.AnimateUnoItemCor(Player);
                playerHas = false;
            }
            else if (dealerHas)
            {
                yield return table.AnimateUnoItemCor(DealerRef);
                dealerHas = false;
            }

            yield return SwapCards(table.PlayerCards, table.DealerCards);
            table.UpdatePlayerValueText();
        }
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
            yield return QOL.WaitForSeconds.Get(0.5f);
            if (i < cards1.Length) collection2.PlaceCard(cards1[i]);
            if (i < cards2.Length) collection1.PlaceCard(cards2[i]);
            EventManager.DealCard();
        }

        yield return QOL.WaitForSeconds.Get(1f);
    }

    [SerializeField] TextMeshPro debugHealthText;
    public void DamagePlayer(Player damagedPlayer, int amount)
    {
        StartCoroutine(DamagePlayerCor());


        IEnumerator DamagePlayerCor()
        {
            damagedPlayer.Health -= amount;

            if (damagedPlayer.Health > 0)
            {
                yield return UIManager.Instance.FadeToBlackFastCor();

                EventManager.PlayerDamaged(damagedPlayer, damagedPlayer.Health);
                TotalHealth = Player.Health + dealer.Health;
                EventManager.PlayerDamaged(null, TotalHealth);

                yield return QOL.WaitForSeconds.Get(0.5f);
                yield return UIManager.Instance.FadeToNoneCor();
            }

#if UNITY_EDITOR
            debugHealthText.text = $"Player: {Player.Health}{(damagedPlayer.IsDealer ? "" : $"(-{amount})")}\n Dealer: {dealer.Health}{(damagedPlayer.IsDealer ? $"(-{amount})" : "")}";
#endif
            if (damagedPlayer.Health <= 0) StartCoroutine(LoseGameCor(damagedPlayer));
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
                    yield return QOL.WaitForSeconds.Get(0.1f);

                    Card card = cards[i];
                    card.SetValue(cardValues[i]);
                    card.State = CardState.InHand;
                    card.IsInteractable = true;

                    collection.PlaceCard(card);
                    EventManager.DealCard();
                }


                if (giveItems)
                {
                    ItemType itemType = (ItemType)Random.Range(0, 6);
                    if (player.IsDealer)
                    {
                        //if (table.DealerItemCollection.GetItemCountForItem(itemType) == 0)
                        //{
                            Item item = table.DealerItemCollection.GetItem(itemType);
                            Transform pos = item.TargetTransform;
                            item.SetTargetTransform(deckPos);
                            item.InstaMoveToTarget();
                            item.SetTargetTransform(pos);
                        EventManager.UseItem(itemType);
                            yield return QOL.WaitForSeconds.Get(1f);
                        //}
                        table.DealerItemCollection.AddItem(itemType);
                    }
                    else
                    {
                        //if (table.PlayerItemCollection.GetItemCountForItem(itemType) == 0)
                        //{
                            Item item = table.PlayerItemCollection.GetItem(itemType);
                            Transform pos = item.TargetTransform;
                            item.SetTargetTransform(deckPos);
                            item.InstaMoveToTarget();
                            item.SetTargetTransform(pos);
                        EventManager.UseItem(itemType);
                            yield return QOL.WaitForSeconds.Get(1f);
                        //}
                        table.PlayerItemCollection.AddItem(itemType);
                    }
                }
            }
        }
    }

    IEnumerator LoseGameCor(Player loser)
    {
        isGameRunning = false;

#if UNITY_EDITOR
        debugHealthText.text = loser.IsDealer ? "You won!" : "You lost :(";
#endif
        if (loser.IsDealer)
        {
            yield return UIManager.Instance.PlayerWinSequenceCor();
        }
        else yield return UIManager.Instance.DealerWinSequenceCor();
    }

    void EndOfRoundCleanup()
    {
        table.PlayerPlayedItems.Clear();
        table.DealerPlayedItems.Clear();
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

    void PlayerSubmitName(string name)
    {
        playerName = name;
        Serializer.Save(playerName, "PLAYER_NAME");
    }

    public void SetTutorialSkip(bool skip) => skipTutorial = skip;

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

        EventManager.OnSubmitPlayerName += PlayerSubmitName;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= TryEndTurn;

        EventManager.OnStartGame -= StartGame;
        EventManager.OnEndGame -= EndGame;

        EventManager.OnPotOverflow -= OnPotOverflow;

        EventManager.OnSubmitPlayerName -= PlayerSubmitName;
    }
}

public enum PlayerType
{
    Player,
    Dealer,
    None
}
