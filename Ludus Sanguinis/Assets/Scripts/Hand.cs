using UnityEngine;

public class Hand
{
    CardStartingTransform[] cardStartingTransforms = new CardStartingTransform[5];
    [SerializeField] Card[] cards;

    void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            cardStartingTransforms[i] = new CardStartingTransform(card.transform.position, card.transform.up, card.transform.forward);
        }
    }
}

public struct CardStartingTransform
{
    public readonly Vector3 StartPos;
    public readonly Vector3 StartUp;
    public readonly Vector3 StartForward;

    public CardStartingTransform(Vector3 startPos, Vector3 startUp, Vector3 startForward)
    {
        StartPos = startPos;
        StartUp = startUp;
        StartForward = startForward;
    }
}
