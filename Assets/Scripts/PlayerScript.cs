using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // --- This script is for BOTH player and dealer

    // Other Scripts
    public CardScript cardScript;
    public DeckScript deckScript;
    
    // Total value of player/dealer's hand
    public int handValue = 0;
    public int totalHands = 1;

    // Betting money
    private int balance = 500;

    // Array of card objects on table
    public GameObject[] hand;
    // Index of next card to be turned over
    public int cardIndex = 0;
    // Tracking aces for 1 to 11 conversions
    List<CardScript> aceList = new List<CardScript>();

    // Deal starting hand
    public void DealHand()
    {
        GetCard();
        GetCard();
    }

    public int GetCard()
    {
        // Get a card, use deal card to assign sprite and value to card
        int cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());
        // Show card on game screen
        hand[cardIndex].GetComponent<Renderer>().enabled = true;
        // Add card value to running total of the hand
        handValue += cardValue;
        // If value is 1, it is an ace
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex].GetComponent<CardScript>());
        }
        // Check if we should use an 11 instead of a 1
        AceCheck();
        cardIndex++;
        return handValue;
    }

    // Search for needed ace conversions, 1 to 11 or vice versa
    public void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            // if converting, adjust card object value and hand
            if (handValue + 10 < 22 && ace.GetValue() == 1)
            {
                ace.SetValue(11);
                handValue += 10;
            } else if (handValue > 21 && ace.GetValue() == 11)
            {
                ace.SetValue(1);
                handValue -= 10;
            }
        }
    }

    // Get player's current balance
    public int GetBalance()
    {
        return balance;
    }

    // Add or subtract from player's balance
    public void AdjustBalance(int amount)
    {
        balance += amount;
    }

    // Increase player's total hands (splitting)
    public void AddHand()
    {
        totalHands++; 
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
