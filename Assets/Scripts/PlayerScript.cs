using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // --- This script is for BOTH player and dealer

    // Other Scripts
    public CardScript cardScript;
    public DeckScript deckScript;
    
    private string playerType = "player";

    // Hand & Card Variables
    private int holeCard = 0;    // Specifically for dealer
    public List<int> handValues = new List<int> {0};
    public List<List<CardScript>> hands = new  List<List<CardScript>> {new List<CardScript>()};
    public List<List<CardScript>> handsAces = new  List<List<CardScript>> {new List<CardScript>()};

    // Pre-defined array of card objects on table to be revealed
    public GameObject[] cards;
    public int cardsIndex = 0;  // Index of next card to be revealed

    // Betting Variables
    private int balance = 500;
    private int bet = 0;

    // Array of card objects on table
    public GameObject[] hand;
    // Tracking aces for 1 to 11 conversions
    List<CardScript> aceList = new List<CardScript>();

    // Deal starting hand (hands[0])
    public void DealHand() {
        CardScript card1 = GetCard(0);
        CardScript card2 = GetCard(0);
        hands[0].Add(card1)
        hands[0].Add(card2)

        // Specific to dealer
        if (playerType == "dealer") {
            holeCard = card2.GetValue()
            handValues[handIndex] -= holeCard
        } 
    }

    // Returns the value of a given hand
    public int GetHandValue(List<CardScript> hand) {
        value = 0;
        foreach (CardScript card in hand) {
            value += card.GetValue();
        }
        return value;
    }

    // Get a card and add it to hands[handindex]
    public CardScript GetCard(int handIndex) {
        // Get a card, use deal card to assign sprite and value to card
        CardScript card = deckScript.DealCard(cards[cardsIndex].GetComponent<CardScript>());
        int handValue = handValues[handIndex];
        handValue += card.GetValue();

        // Handle Aces
        if (card.GetValue() == 1) { 
            handsAces[handIndex].Add(card)
        }
        handValue = AceConverter(handIndex, handValue);

        // Update handValues and show card
        handValues[handIndex] = handValue;
        cards[cardsIndex].GetComponent<Renderer>().enabled = true;
        cardsIndex++;
        return card;
    }

    // Search for needed ace conversions, 1 to 11 or vice versa
    public int AceConverter(int handIndex, int handValue) {
        foreach (CardScript ace in handsAces[handIndex]) {
            // if converting, adjust card object value and hand
            if (ace.GetValue() == 1 && handValue + 10 < 22) {
                ace.SetValue(11);
                handValue += 10;
            } else if (ace.GetValue() == 11 && handValue > 21) {
                ace.SetValue(1);
                handValue -= 10;
            }
        }
        return handValue
    }

    public void SplitHand(int handIndex) {
        hands.Add(new List<CardScript>())
        CardScript card = hands[handIndex][1]

        // Make a new hand in hands
        // Remove one of the cards from the first hand and add it to the second
        // play out first hand as normal, play out second hand as normal 
    }

    // Balance Accessor Functions
    public int GetBalance() { return balance; }
    public void AdjustBalance(int amount) { balance += amount; }

    // Hole Card Accessor Functions
    public int GetHoleCard() { return holeCard; }

    // Increase player's total hands (splitting)
    public void AddHand() {

    }


    // 
    public void SetPlayerType(string type) {
        playerType = type
    }


    // Hides all cards, resets the needed variables
    public void ResetHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex = 0;
        handValue = 0;
        totalHands = 0;
        aceList = new List<CardScript>();
    }
}
