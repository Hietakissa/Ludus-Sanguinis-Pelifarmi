using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] PlayedCardPosition[] player1PlayedCardPositions;
    [SerializeField] PlayedCardPosition[] player2PlayedCardPositions;

    public void PlayCard(Player player, Card card)
    {
        // Move played card to the next available position
        PlayedCardPosition[] playedCardPositions = player.IsDealer ? player2PlayedCardPositions : player1PlayedCardPositions;
        int cardPosIndex = GetHighestAvailablePosIndexForPlayer(player, playedCardPositions);
        card.MoveToTransform(playedCardPositions[cardPosIndex].Transform);

        Debug.Log($"moving card to: {playedCardPositions[cardPosIndex].Transform.position}");

        // Then offset the other cards in the hand
    }

    int GetHighestAvailablePosIndexForPlayer(Player player, PlayedCardPosition[] playedCardPositions)
    {
        for (int i = 0; i < playedCardPositions.Length; i++)
        {
            if (!playedCardPositions[i].HasCard) return i;
        }

        Debug.LogError($"Couldn't find available card position for played card by '{(player.IsDealer ? "Dealer" : "Player")}', total card positions: {playedCardPositions.Length}. " +
            $"This should not happen, as a player can only have a maximum of 5 cards and 5 postitions should be assigned. Returned 0, expect problems.");
        return 0;
    }
}

[System.Serializable]
class PlayedCardPosition
{
    [field: SerializeField] public Transform Transform { get; private set; }
    public bool HasCard;
}
