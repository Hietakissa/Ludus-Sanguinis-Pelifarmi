using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] Table table;
    [SerializeField] Player dealer;

    [SerializeField] TextMeshPro text;

    void Awake()
    {
        dealer.InitCards();
    }

    bool visibleState = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            visibleState = !visibleState;
        }

        bool state = visibleState;

#if UNITY_EDITOR
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
        {
            state = true;
        }
#endif

        Cursor.visible = state;
    }

    void TryEndTurn()
    {
        int playerSum = GetCardSumForCollection(table.PlayerCards);
        bool canEndTurn = playerSum > 0;

        Debug.Log($"Tried to end turn, would have been successful: {playerSum > 0}");

        /// thís shouldn't actually be here, but whatever for now. The dealer should play their turn first
        if (canEndTurn)
        {
            Dealer.PlayTurn(table, dealer);

            int dealerSum = GetCardSumForCollection(table.DealerCards);
            int sumDifference = Mathf.Abs(playerSum - dealerSum);

            text.text = $"{dealerSum}\n" +
                $"diff: {sumDifference}\n" +
                $"{playerSum}";
        }
    }


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
