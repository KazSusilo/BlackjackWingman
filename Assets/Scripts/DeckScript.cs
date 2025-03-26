using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] deck;
    int[] cardValues = new int[53];
    public int currentIndex = 0;

    // Start is called before the first frame update
    void Start() {
        SetCardValues();
    }

    // Assigns a value to each card in the deck
    void SetCardValues() {
        int value = 0;
        // Loop to assign values to each card in the deck
        for (int i = 0; i < deck.Length; i++) {
            // Count up to the amount of cards, 52 for a single deck
            // if there is a remainder after x/13, then remainder
            // is used as the value, unless over 10, then use 10
            value = i;
            value %= 13;
            if (value > 10 || value == 0) {
                value = 10;
            }
            cardValues[i] = value;
            value++;
        }
    }

    // Shuffles deck
    public void Shuffle() {
        // Standard array data swapping technique
        for (int i = deck.Length - 1; i > 0; --i) {
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * (deck.Length - 1)) + 1;
            
            // Swap sprite of cards
            Sprite tempCard = deck[i];
            deck[i] = deck[j];
            deck[j] = tempCard;

            // Swap value of cards
            int tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;
        }
        currentIndex = 1;   // index0 = back of card
    }

    // Deals card in 'card' slot on table
    public CardScript DealCard(CardScript card) {
        card.SetSprite(deck[currentIndex]);
        card.SetValue(cardValues[currentIndex]);
        currentIndex++;
        return card;
    }

    // Copy sourceCard to targetCard
    public CardScript CopyCard(CardScript sourceCard, CardScript targetCard) {
        targetCard.SetSprite(sourceCard.gameObject.GetComponent<SpriteRenderer>().sprite);
        targetCard.SetValue(sourceCard.GetValue());
        return targetCard;
    }

    public Sprite GetBackOfCard()
    {
        return deck[0];
    }
}
