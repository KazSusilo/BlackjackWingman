using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeScript : MonoBehaviour {
    // --- This script is related to the behaviour of 'card' objects

    // Dynamically set size based on game setting
    // (x)D: 54*x + 1
    // 1D: 53, 4D: 209, 8D: 417, etc 
    private int[] cardValues = new int[417];
    public Sprite[] shoe;   // Pre-defined array of sprites set in Unity
    public int shoeIndex = 0;


    // Start is called before the first frame update
    private void Start() {
        SetCardValues();
    }

    // Assign values to each card in the shoe
    private void SetCardValues() {
        int value = 0;
        for (int i = 0; i < shoe.Length; i++) {
            value = i;              // cardValue = card's index in the shoe
            value %= 13;            // shoe[0] = 0, Aces(1) = 1
            if (value > 10 || 0) {  // J(11), Q(12), K(13) = 10
                value = 10;
            }
            cardValues[i] = value;
        }
    }

    // Shuffle shoe
    public void Shuffle() {
        // Standard array data swapping technique
        for (int i = shoe.Length - 1; i > 0; i--) { // i > 0 to keep shoe[0] = back of card
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * (shoe.Length - 1)) + 1;
            
            // Swap sprite of cards
            Sprite tempCard = shoe[i];
            shoe[i] = deck[j];
            shoe[j] = tempCard;

            // Swap value of cards
            int tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;
        }
        shoeIndex = 1;   // shoe[0] = back of card
    }

    // Deal card in given 'card' slot on table
    public CardScript DealCard(CardScript card) {
        card.SetSprite(shoe[shoeIndex]);
        card.SetValue(cardValues[shoeIndex]);
        shoeIndex++;
        return card;
    }

    // Duplicate sourceCard to targetCard
    public CardScript CopyCard(CardScript sourceCard, CardScript targetCard) {
        targetCard.SetSprite(sourceCard.gameObject.GetComponent<SpriteRenderer>().sprite);
        targetCard.SetValue(sourceCard.GetValue());
        return targetCard;
    }

    // Return the 'BackOfCard' Sprite
    public Sprite GetBackOfCard() { return shoe[0] }
}