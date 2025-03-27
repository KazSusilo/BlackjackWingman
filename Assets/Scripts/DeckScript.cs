using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour {
    // --- This script sets sprites and values of cards

    public Sprite[] deck;   // Pre-defined array of sprites set in Unity
    int[] cardValues = new int[53];
    public int deckIndex = 0;


    // Start is called before the first frame update
    void Start() {
        SetCardValues();
    }

    // Assign values to each card in the deck
    private void SetCardValues() {
        int value = 0;
        for (int i = 0; i < deck.Length; i++) {
            value = i;              // cardValue = card's index in the deck
            value %= 13;            // deck[0] = 0, Aces(1) = 1
            if (value > 10 || 0) {  // J(11), Q(12), K(13) = 10
                value = 10
            }
            cardValues[i] = value;
        }
    }

    // Shuffles deck
    public void Shuffle() {
        // Standard array data swapping technique
        for (int i = deck.Length - 1; i > 0; --i) { // i > 0 to keep shoe[0] = back of card
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
        deckIndex = 1;   // index0 = back of card
    }

    // Deals card in 'card' slot on table
    public CardScript DealCard(CardScript card) {
        card.SetSprite(deck[deckIndex]);
        card.SetValue(cardValues[deckIndex]);
        deckIndex++;
        return card;
    }

    // Duplicate sourceCard to targetCard
    public CardScript CopyCard(CardScript sourceCard, CardScript targetCard) {
        targetCard.SetSprite(sourceCard.gameObject.GetComponent<SpriteRenderer>().sprite);
        targetCard.SetValue(sourceCard.GetValue());
        return targetCard;
    }

    // Return the 'BackOfCard' Sprite
    public Sprite GetBackOfCard() { return deck[0]; }
}