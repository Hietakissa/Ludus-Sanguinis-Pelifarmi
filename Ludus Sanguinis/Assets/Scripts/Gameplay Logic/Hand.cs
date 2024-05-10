using UnityEngine;

[System.Serializable]
public class Hand
{
    [SerializeField] Card[] cards;

    public CardPosCollection CardCollection => cardCollection;
    [SerializeField] CardPosCollection cardCollection;

    public void Init()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            
            cardCollection.PlaceCard(card);
            Debug.Log($"initializing card");
            //cardCollection.CardPositions[i].Card = card;
        }
    }

    public void ReturnCard(Card card)
    {
        CardCollection.PlaceCard(card);
    }
}
