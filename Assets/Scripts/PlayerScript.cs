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
    public GameObject[] cards1;
    public GameObject[] cards2;
    public GameObject[] cards3;
    public GameObject[] cards4;
    public List<GameObject[]> cardsM = new List<GameObject[]>();
    public int cardsIndex = 0;  // Index of next card to be revealed

    // Betting Variables
    private float balance = 5000f;

    // Distringuish type of player
    public void SetPlayerType(string type) {
        playerType = type;
        cardsM.Add(cards1);

        // Specific to player
        if (playerType == "player") {
            cardsM.Add(cards2);
            cardsM.Add(cards3);
            cardsM.Add(cards4);
        }
    }

    // Deal starting hand (hands[0])
    public void DealHand() {
        CardScript card1 = GetCard(0);
        CardScript card2 = GetCard(0);

        // Specific to dealer
        if (playerType == "dealer") {
            holeCard = card2.GetValue();
            handValues[0] -= holeCard;
        } 
    }

    // Check if player/dealer has blackjack
    public bool HasBlackjack() {
        if ((handValues.Count == 1) && (hands[0].Count == 2) && (handValues[0] + holeCard == 21)) {
            return true;
        }
        return false;
    }

    // Get a card and add it to hands[handindex]
    public CardScript GetCard(int handIndex) {
        // Get a card, use deal card to assign sprite and value to card
        CardScript card = deckScript.DealCard(cardsM[handIndex][cardsIndex].GetComponent<CardScript>());
        int handValue = handValues[handIndex];
        handValue += card.GetValue();

        // Handle Aces
        if (card.GetValue() == 1) { 
            handsAces[handIndex].Add(card);
        }
        handValue = AceConverter(handIndex, handValue);

        // Update handValues/hands and show card
        handValues[handIndex] = handValue;
        hands[handIndex].Add(card);
        cardsM[handIndex][cardsIndex].GetComponent<Renderer>().enabled = true;
        cardsIndex++;
        return card;
    }

    // Search for needed ace conversions, 1 to 11 or vice versa
    private int AceConverter(int handIndex, int handValue) {
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
        return handValue;
    }

    public void SplitHand(int handIndex) {
        //handValues.Add(handValues[0] / 2)
        hands.Add(new List<CardScript>());
        handsAces.Add(new List<CardScript>());
        CardScript card = hands[handIndex][1];

        // Make a new hand in hands
        // Remove one of the cards from the first hand and add it to the second
        hands[handIndex].RemoveAt(1);
        hands[handIndex].Add(card);
        // play out first hand as normal, play out second hand as normal
        GetCard(handIndex);
        GetCard(handIndex + 1);
    }

    // Balance Accessor Functions
    public float GetBalance() { return balance; }
    public void AdjustBalance(float amount) { balance += amount; }
    
    // Hole Card Accessor Functions
    public int GetHoleCard() { return holeCard; }

    // Hides all cards, resets the needed variables
    public void ResetHand() {
        for (int i = 0; i < cards1.Length; i++) {
            // Hand 1
            cardsM[0][i].GetComponent<CardScript>().ResetCard();
            cardsM[0][i].GetComponent<Renderer>().enabled = false;
            if (playerType == "player") {   // Specific to player
                // Hand 2
                cardsM[1][i].GetComponent<CardScript>().ResetCard();
                cardsM[1][i].GetComponent<Renderer>().enabled = false;
                // Hand 3
                cardsM[2][i].GetComponent<CardScript>().ResetCard();
                cardsM[2][i].GetComponent<Renderer>().enabled = false;
                // Hand 4
                cardsM[3][i].GetComponent<CardScript>().ResetCard();
                cardsM[3][i].GetComponent<Renderer>().enabled = false;
            }
        }
        holeCard = 0;
        handValues = new List<int> {0};
        hands = new  List<List<CardScript>> {new List<CardScript>()};
        handsAces = new  List<List<CardScript>> {new List<CardScript>()};
        cardsIndex = 0;
    }
}