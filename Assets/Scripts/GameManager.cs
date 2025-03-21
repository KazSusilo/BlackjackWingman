using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Buttons
    public Button dealButton;
    public Button betButton;
    public Button hitButton;
    public Button standButton;
    public Button splitButton;
    public Button doubleButton;

    // Deck
    public DeckScript deck;

    // Player 
    public PlayerScript player;
    private int handIndex = 0;
    private float betAmount = 0;

    // Dealer
    public PlayerScript dealer;
    public GameObject blinder;  // Blinder hiding dealer's hole card

    // Game Settings
    private float penetration; 
    /*
    private int maxSplit;
    private bool surrender;
    */

    // Player HUD
    public TMP_Text playerBalance;
    public TMP_Text playerBet; 
    public TMP_Text playerHandValue;
    public TMP_Text dealerHandValue;
    public TMP_Text roundText;  // ("Win", "Lose", "Push", etc)
    

    // Start is called before the first frame update
    void Start() {
        player.SetPlayerType("player");
        dealer.SetPlayerType("dealer");
        deck.Shuffle();

        // Listener for Buttons
        dealButton.onClick.AddListener(() => DealClicked());

        betButton.onClick.AddListener(() => BetClicked());
        /*
        bet1Button.onClick.AddListener(() => 1Clicked());
        bet5Button.onClick.AddListener(() => 5Clicked());
        bet25Button.onClick.AddListener(() => 25Clicked());
        bet100Button.onClick.AddListener(() => 100Clicked());
        bet500Button.onClick.AddListener(() => 500Clicked());
        */

        hitButton.onClick.AddListener(() => HitClicked());
        standButton.onClick.AddListener(() => StandClicked());
        splitButton.onClick.AddListener(() => SplitClicked());
        doubleButton.onClick.AddListener(() => DoubleClicked());
    }

    // Set betting amount for player
    private void BetClicked() {
        Text newBet = betButton.GetComponentInChildren(typeof(TMP_Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString());
        player.AdjustBalance(-intBet);
        playerBalance.text = player.GetBalance().ToString();
        betAmount += intBet;
        playerBet.text = betAmount.ToString();

        // Open Chips to select from 
    }

    // Deal initial cards to both player and dealer
    private void DealClicked() {
        ClearTable();
        
        // Close Betting
        CloseBetting();
        playerBet.text = "Bet: " + betAmount.ToString();
        player.AdjustBalance(-betAmount);
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
             
        // Deal cards to player
        player.DealHand();
        playerHandValue.text = player.handValues[0].ToString();
        playerHandValue.gameObject.SetActive(true);
        
        // Deal cards to dealer
        dealer.DealHand();
        blinder.GetComponent<Renderer>().enabled = true;
        dealerHandValue.text = dealer.handValues[0].ToString();
        dealerHandValue.gameObject.SetActive(true);
        
        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
        SetAvailableActions(player.hands[handIndex]);

        // Check for BJ
        if (player.HasBlackjack() || dealer.HasBlackjack()) {
            RoundOver();
        }
    }

    // Clear table before a hand is dealt
    private void ClearTable() {
        roundText.gameObject.SetActive(false);
        player.ResetHand();
        handIndex = 0;
        dealer.ResetHand();

        // Shuffle shoe if cut card is reached 
        // (NEED TO CHANGE HARD CODED CUT CARD)
        if (deck.currentIndex >= 42) {
            deck.Shuffle();
        }
    }

    // Enable appropriate action buttons on player's turn
    private void SetAvailableActions(List<CardScript> hand) {
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);

        // Check if the player can double
        if (hand.Count == 2) {
            doubleButton.gameObject.SetActive(true);
            
            // Check if the player can split
            int card1 = hand[0].GetValue();
            int card2 = hand[1].GetValue();
            if ((card1 == card2) || ((card1 == 1 || card1 == 11) && (card2 == 1 || card2 == 11))) {
                splitButton.gameObject.SetActive(true);
            }
        }
    }

    private void HitClicked() {
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        if (player.cardsIndex <= 10) {
            player.GetCard(handIndex);
            playerHandValue.text = player.handValues[handIndex].ToString();
            if (player.handValues[handIndex] > 20) {
                StandClicked();
            }
        }
        SetAvailableActions(player.hands[handIndex]);
    }

    private void StandClicked() { 
        handIndex++;
        if (handIndex == player.hands.Count) {
            RoundOver();
        }
    }

    private void SplitClicked() {
        player.SplitHand(handIndex);
    }

    private void DoubleClicked() {
        // Take original bet and double it
        HitClicked();
        StandClicked();
    }

    private void HitDealer() {
        //yield return new WaitForSeconds(.5f);
        dealer.GetCard(0);
        dealerHandValue.text = dealer.handValues[0].ToString();
    }


    // Disable all action buttons
    private void DisableActionButtons() {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);
    }

    private void CloseBetting() {
        betButton.gameObject.SetActive(false);
        // Disable various amounts 
    }

    // Hand is over
    private void RoundOver() {
        DisableActionButtons();

        // Dealer plays their hand
        blinder.GetComponent<Renderer>().enabled = false;           // Show hole card
        dealer.handValues[0] += dealer.holeCard;                    // Add hole card to handValue
        dealerHandValue.text = dealer.handValues[0].ToString();     // Update text
        //StartCoroutine(HitDealer());    
        while (dealer.handValues[0] < 17) {
            HitDealer();
        }
        bool dealerBust = 21 < dealer.handValues[0];
        bool dealerBJ = dealer.HasBlackjack();

        // Address all player's hands
        bool playerBJ = player.HasBlackjack();
        foreach (int handValue in player.handValues) {
            bool playerBust = 21 < handValue;

            // if player busts or player hand < dealer hand, player loses
            if (playerBust ||  ((!dealerBust) && (handValue < dealer.handValues[0]))) {
                roundText.text = "Lose";
            }
            // if dealer busts or player hand > dealer hand, player wins
            else if (dealerBust || ((!playerBust) && (dealer.handValues[0] < handValue))) {
                roundText.text = "Win";
                player.AdjustBalance(2 * betAmount);
            }
            else if (playerBJ && !dealerBJ) {
                roundText.text = "Blackjack";
                player.AdjustBalance(2.5f * betAmount);
            }
            else if (handValue == dealer.handValues[0]) {
                roundText.text = "Push";
            }
        }

        // Set UI for next move / hand / turn
        dealButton.gameObject.SetActive(true);
        roundText.gameObject.SetActive(true);

        playerBalance.text = "Balance: " + player.GetBalance().ToString();
    }
}