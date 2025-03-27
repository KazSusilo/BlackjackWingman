using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    // --- This script is for BOTH player and dealer

    // Access other scripts
    public CardScript cardScript;
    public DeckScript deckScript;
    public ShoeScript shoeScript;

    // Player variables
    private string playerType = "player";   // "player" || "dealer"
    private float balance = 5000f;
    
    // Hand variables
    private int holeCard = 0;               // Specifically for dealer
    public List<int> handValues = new List<int> {0};
    public List<string> handTypes = new List<char> {'H'};
    public List<List<CardScript>> hands = new  List<List<CardScript>> {new List<CardScript>()}; 
    public List<List<CardScript>> handsAces = new  List<List<CardScript>> {new List<CardScript>()};
    // Ex. Situation:   [   h1(A,J),    h2(9,A),        h3(2,5,6,3),        h4(A,A,A,Q)         ]
    // handValues   =   [   21,         20,             16,                 13                  ]
    // handTypes    =   [   'BJ',       'S',            'H',                'H'                 ]
    // hands        =   [   h1(c1,c2),  h2(c1, c2),     h3(c1,c2,c3,c4),    h4(c1,c2,c3,c4)     ]
    // handsAces    =   [   h1(A1),     h2(A1),         h3(),               h4(A1,A2)           ]

    // Array of card objects on table to be revealed
    public GameObject[] cards1; // pre-defined in Unity editor
    public GameObject[] cards2; // pre-defined in Unity editor
    public GameObject[] cards3; // pre-defined in Unity editor
    public GameObject[] cards4; // pre-defined in Unity editor
    public List<GameObject[]> cards = new List<GameObject[]>();         // Master array
    public List<int[]> cardsIndexes = new List<int[]> {0, 0, 0, 0};     // Master indexes
    
    // Distinguish type of player
    public void InitializePlayer(string type) {
        playerType = type;

        // Set-up master array of cards
        cards.Add(cards1);
        if (playerType == "player") {
            cards.Add(cards2);
            cards.Add(cards3);
            cards.Add(cards4);
        }
    }

    // Deal starting hand
    public void DealHand() {
        // Add two cards to hands[0]
        CardScript card1 = GetCard(0);
        CardScript card2 = GetCard(0);

        // Specific to dealer - hide second card value
        if (playerType == "dealer") {
            holeCard = card2.GetValue();
            handValues[0] -= holeCard;
        } 

        // Check for BJ to update handType
        HasBlackjack();
    }

    // Add a card to current hand ( hands[handIndex] )
    public CardScript GetCard(int handIndex) {
        // Address current hand
        int handValue = handValues[handIndex];
        string handType = handTypes[handIndex];
        int hand = hands[handIndex];
        int handAces = handsAces[handIndex];
        int cardsIndex = cardsIndexes[handIndex];

        // New card to be added
        CardScript card = deckScript.DealCard(cards[handIndex][cardsIndex].GetComponent<CardScript>())
        //CardScript card = shoeScript.DealCard(cards[handIndex][cardsIndex].GetComponent<CardScript>())
        int cardValue = card.GetValue();
        
        // Add card to hand
        handValue += cardValue;
        hand.Add(card);  
        if (cardValue == 1) {
            handAces.Add(card)
        }

        // Convert aces to maximize hand without busting
        handValue = AceConverter(int handIndex, int handValue);

        // Update master
        handValues[handIndex] = handValue;
        hands[handIndex] = hand;    // handTypes handled in AceConverter
        handsAces[handIndex] = handAces;
        cards[handIndex][cardsIndex].GetComponent<Renderer>().enabled = true;
        cardsIndexes[handIndex] = cardsIndex + 1;
        
        return card;
    }

    // Search for needed ace conversions, 1 to 11 or vice versa
    private int AceConverter(int handIndex, int handValue) {
        bool softHand = false;
        foreach (CardScript ace in handsAces[handIndex]) {
            int aceValue = ace.GetValue();
            // if converting, adjust 'card' object value and handValue
            if (aceValue == 1 && handValue + 10 < 22) {
                ace.SetValue(11);
                handValue += 10;
                softHand = true;
            } else if (aceValue == 11 && handValue > 21) {
                ace.SetValue(1);
                handValue -= 10;
            }
        }

        // Indicate Hard or Soft hand
        handTypes[handIndex] = "H";
        if (softHand) {
            handTypes[handIndex] = 'S';
        }

        return handValue;
    }

    // Check if player/dealer has blackjack
    private bool HasBlackjack() {
        if ((handValues.Count == 1) && (hands[0].Count == 2) && (handValues[0] + holeCard == 21)) {
            handTypes[0] = "BJ" // Update handType
            return true;
        }
        return false;
    }

    

    

    // Split hand
    public void SplitHand(int handIndex) {
        // Create new hand
        hands.Add(new List<CardScript>());
        handsAces.Add(new List<CardScript>());
        handValues.Add(0);

        // Get card to split off     
        int newHandIndex = handValues.Count - 1;
        CardScript sourceCard = hands[handIndex][1];
        CardScript targetCard = cardsM[newHandIndex][0].GetComponent<CardScript>();
        CardScript card = deckScript.CopyCard(sourceCard, targetCard);    
        int cardValue = card.GetValue();

        // Split cards into two hands
        hands[handIndex].RemoveAt(1);
        hands[newHandIndex].Add(card);
        // Check if card is ace to add it to handsAces
        if (cardValue == 1 || cardValue == 11) {
            handsAces[handIndex].RemoveAt(1);
            handsAces[newHandIndex].Add(card);
        }

        // New Hand
        handValues[newHandIndex] += cardValue;
        cardsM[newHandIndex][0].GetComponent<Renderer>().enabled = true;

        // OG Hand
        handValues[handIndex] -= cardValue;
        sourceCard.ResetCard();
        cardsM[handIndex][0].GetComponent<Renderer>().enabled = true;
        cardsIndex--;

        // Populate both hands
        GetCard(newHandIndex);
        cardsIndex--;
        GetCard(handIndex);
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