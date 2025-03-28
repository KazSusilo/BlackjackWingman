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
    private bool isPlayer = true;
    private float balance = 5000f;
    
    // Hand variables
    private int holeCard = 0;               // Specifically for dealer
    public List<int> handValues = new List<int> {0};
    public List<string> handTypes = new List<string> {'H'};
    public List<List<CardScript>> hands = new  List<List<CardScript>> {new List<CardScript>()}; 
    public List<List<CardScript>> handsAces = new  List<List<CardScript>> {new List<CardScript>()};
    // Ex. Situation:   [   h1(A,J),    h2(9,A),        h3(2,5,6,3),        h4(A,A,A,Q)         ]
    // handValues   =   [   21,         20,             16,                 13                  ]
    // handTypes    =   [   'BJ',       'S',            'H',                'H'                 ]
    // hands        =   [   h1(c1,c2),  h2(c1, c2),     h3(c1,c2,c3,c4),    h4(c1,c2,c3,c4)     ]
    // handsAces    =   [   h1(A1),     h2(A1),         h3(),               h4(A1,A2,A3)        ]

    // Array of card objects on table to be revealed
    public GameObject[] cards1; // pre-defined in Unity editor
    public GameObject[] cards2; // pre-defined in Unity editor
    public GameObject[] cards3; // pre-defined in Unity editor
    public GameObject[] cards4; // pre-defined in Unity editor
    public List<GameObject[]> cards = new List<GameObject[]>();     // Master array
    public List<int> cardsIndexes = new List<int> {0, 0, 0, 0};     // Master indexes
    
    // Distinguish type of player
    public void InitializePlayer(bool type) {
        isPlayer = type;

        // Set-up master array of cards
        cards.Add(cards1);
        if (isPlayer) {
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
        if (!isPlayer) {
            holeCard = card2.GetValue();
            handValues[0] -= holeCard;
        } 

        // Check for BJ to update handType
        HasBlackjack();
    }

    // Gets the top card in the shoe and adds it to the given hand (using 'handIndex')
    public CardScript GetCard(int handIndex) {
        int cardsIndex = cardsIndexes[handIndex];   // Index of where card should be placed in hand
        CardScript card = deckScript.DealCard(cards[handIndex][cardsIndex].GetComponent<CardScript>())
        //CardScript card = shoeScript.DealCard(cards[handIndex][cardsIndex].GetComponent<CardScript>())
        AddCardToHand(card, handIndex);
        return card;
    }

    // Helper function to add given 'card' to given hand (hands[handIndex])
    private void AddCardToHand(CardScript card, int handIndex) {
        // Address specific hand
        int handValue = handValues[handIndex];
        int hand = hands[handIndex];
        int handAces = handsAces[handIndex];
        int cardsIndex = cardsIndexes[handIndex];
        int cardValue = card.GetValue();    // given card's value

        // Add card to hand
        handValue += cardValue;
        hand.Add(card);
        if (cardValue == 1 || cardValue == 11) {
            handAces.Add(card)
        }
        
        // Convert aces to maximize hand without busting
        handValue = AceConverter(hand, handValue)

        // Update master
        handValues[handIndex] = handValue;
        hands[handIndex] = hand;    // handTypes handled in AceConverter
        handsAces[handIndex] = handAces;
        cards[handIndex][cardsIndex].GetComponent<Renderer>().enabled = true;
        cardsIndexes[handIndex] = cardsIndex + 1;
    }

    // Search for needed ace conversions, 1 to 11 or vice versa
    private int AceConverter(List<CardScript> hand, int handValue) {
        bool softHand = false;
        foreach (CardScript ace in hand) {
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
        handTypes[handIndex] = 'H';
        if (softHand) {
            handTypes[handIndex] = 'S';
        }

        return handValue;
    }

    // Check if player/dealer has blackjack
    private bool HasBlackjack() {
        int numOfHands = handValues.Count;
        int numOfCards = hands[0].Count;
        int handValue = handValues[0] + holeCard;
        if ((numOfHands == 1) && (numOfCards == 2) && (handValue == 21)) {
            handTypes[0] = "BJ" // Update handType
            return true;
        }
        return false;
    }

    // Split current hand
    public void SplitHand(int handIndex) {
        // Create an additional hand
        AddHand();

        // Split cards
        int currCardsIndex = cardsIndexes[handIndex];   // Index of which card should be split-off
        int newHandIndex = handValues.Count - 1;        // Index of which hand to place card
        int newCardsIndex = cardsIndexes[newHandIndex]; // Index of where card should be newly placed in hand
        CardScript currCard = hands[handIndex][currCardsIndex - 1];                         // split-off card origin
        CardScript newCard = cards[newHandIndex][newCardsIndex].GetComponent<CardScript>(); // split-off card destination
        newCard = deckScript.CopyCard(currCard, newCard);   
        //newCard = shoeScript.CopyCard(currCard, newCard);
        
        // Add newCard to newHand
        AddCardToHand(newCard, newHandIndex);
        // Remove currCard from currHand
        RemoveCardFromHand(currCard, handIndex)

        // Deal two more cards
        GetCard(handIndex);
        GetCard(newHandIndex);
    }

    // Create an additional hand
    private void AddHand() {
        handValues.Add(0);
        handTypes.Add('H');
        hands.Add(new List<CardScript>());
        handsAces.Add(new List<CardScript>());
    }

    // Helper function to remove given 'card' (last added card) from given hand (using 'handIndex')
    private void RemoveCardFromHand(CardScript card, int handIndex) {
        // Address specific hand
        int handValue = handValues[handIndex];
        int hand = hands[handIndex];
        int handAces = handsAces[handIndex];
        int cardsIndex = cardsIndexes[handIndex];
        int cardValue = card.GetValue();    // given card's value

        // Remove card from hand
        cardsIndex--; 
        handValue -= cardValue;
        hand.RemoveAt(cardsIndex);
        if (cardValue == 1 || cardValue == 11) {
            handAces.RemoveAt(cardsIndex);
        }
        card.ResetCard();
        
        // Convert aces to maximize hand without busting
        handValue = AceConverter(hand, handValue)

        // Update master
        handValues[handIndex] = handValue;
        hands[handIndex] = hand;    // handTypes handled in AceConverter
        handsAces[handIndex] = handAces;
        cards[handIndex][cardsIndex].GetComponent<Renderer>().enabled = true;
        cardsIndexes[handIndex] = cardsIndex;
    }

    // Balance Accessor Functions
    public float GetBalance() { return balance; }
    public void AdjustBalance(float amount) { balance += amount; }
    
    // Hole Card Accessor Functions
    public int GetHoleCard() { return holeCard; }

    // Reset hand and card variables to initial states
    public void ResetHand() {
        // Reset master array
        for (int i = 0; i < hands.Count; i++;) {
            int handIndex = i;
            for (int j = 0; j < hands[handIndex].Count; j++) {
                int cardsIndex = j;
                GameObject card = cards[handIndex][cardsIndex];
                card.GetComponent<CardScript>().ResetCard();
                card.GetComponent<Renderer>().enabled = false;
            }
            cardsIndexes[handIndex] = 0;    // reset master indexes
        }

        // Reset Hand Variables
        holeCard = 0;
        handValues = new List<int> {0};
        handTypes = new List<string> {'H'};
        hands = new  List<List<CardScript>> {new List<CardScript>()};
        handsAces = new  List<List<CardScript>> {new List<CardScript>()};
    }
}