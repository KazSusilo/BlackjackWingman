using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeScript : MonoBehaviour {
    // --- This script is related to the behaviour of 'card' objects

    // Dynamically set size based on game setting
    // 1D: 53, 4D: 209, 8D: 417, etc 
    public int totalCards;             
    private List<int> cardValues = new List<int>();
    public List<Sprite> shoe = new List<Sprite>();
    public Sprite[] Deck;   // Pre-defined array of card sprites set in Unity
    public int shoeIndex = 0;
    public int runningCount = 0;
    public int trueCount = 0;


    // Start is called before the first frame update
    private void Start() {
        SetShoe(2);
        SetCardValues();
    }

    // Assigns the amount of decks there are in the shoe
    public void SetShoe(int decks) {
        shoe.Add(Deck[0]);    // Add initial card
        for (int i = 0; i < decks; i++) {           // iD game ~ i=8: 8D game
            for (int j = 1; j < Deck.Length; j++) {
                shoe.Add(Deck[j]);
            }
        }
        totalCards = shoe.Count - 1;
    }

    // Assign values to each card in the shoe
    private void SetCardValues() {
        int value = 0;
        for (int i = 0; i < shoe.Count; i++) {
            value = i;              // cardValue = card's index in the shoe
            value %= 13;            // shoe[0] = 0, Aces(1) = 1
            if (value > 10 || value == 0) {  // J(11), Q(12), K(13) = 10
                value = 10;
            }
            cardValues.Add(value);
        }
    }

    // Shuffle shoe
    public void Shuffle() {
        // Standard array data swapping technique
        for (int i = shoe.Count - 1; i > 0; i--) { // i > 0 to keep shoe[0] = back of card
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * (shoe.Count - 1)) + 1;
            
            // Swap sprite of cards
            Sprite tempCard = shoe[i];
            shoe[i] = shoe[j];
            shoe[j] = tempCard;

            // Swap value of cards
            int tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;
        }
        shoeIndex = 1;    // shoe[0] = back of card
        runningCount = 0; // reset running count
    }

    // Deals the top card of the shoe and assigns to the given 'card'
    public CardScript DealCard(CardScript card) {
        card.SetSprite(shoe[shoeIndex]);
        card.SetValue(cardValues[shoeIndex]);
        shoeIndex++;

        // Adjust running count
        int cardValue = card.GetValue();
        if (2 <= cardValue && cardValue <= 6) {
            runningCount++;
        }
        else if (cardValue == 10 || cardValue == 1 || cardValue == 11) {
            runningCount--;
        }

        // Compute true count
        float cardsRemaining = (float)(totalCards - shoeIndex - 1);                 // -1 for backofCard
        float decksRemaining = Mathf.Round((cardsRemaining / 52) / .25f) * .25f;    // Round to nearest .25
        trueCount = (int)(Mathf.Floor(runningCount / decksRemaining));              // Floor trueCount

        return card;
    }

    // Duplicate sourceCard to targetCard
    public CardScript CopyCard(CardScript sourceCard, CardScript targetCard) {
        targetCard.SetSprite(sourceCard.gameObject.GetComponent<SpriteRenderer>().sprite);
        targetCard.SetValue(sourceCard.GetValue());
        return targetCard;
    }

    // Return the 'BackOfCard' Sprite
    public Sprite GetBackOfCard() { return shoe[0]; }
}