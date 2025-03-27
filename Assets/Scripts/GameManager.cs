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
    private List<float> bets = new List<float> {500f};
    private float originalBet = 500f;

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
    public TMP_Text [] playerHandValues;
    public TMP_Text [] playerHandIndicators;
    public TMP_Text dealerHandValue;
    public TMP_Text roundText;  // ("Win", "Lose", "Push", etc)
    

    // Start is called before the first frame update
    void Start() {
        deck.Shuffle();
        player.InitializePlayerType("player");
        dealer.InitializePlayerType("dealer");

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
        bets[handIndex] += intBet;
        playerBet.text = totalBet().ToString();

        // Open Chips to select from 
    }

    private float totalBet() {
        float total = 0f;
        foreach (float bet in bets) {
            total += bet;
        }
        return total;
    }

    // Deal initial cards to both player and dealer
    private void DealClicked() {
        ClearTable();
        
        // Close Betting
        CloseBetting();
        player.AdjustBalance(-bets[handIndex]);
        playerBet.text = "Bet: " + totalBet().ToString();
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
             
        // Deal cards to player
        player.DealHand();
        playerHandValues[handIndex].text = player.handValues[handIndex].ToString();
        playerHandValues[handIndex].gameObject.SetActive(true);
        playerHandIndicators[handIndex].gameObject.SetActive(true);
        
        // Deal cards to dealer
        dealer.DealHand();
        blinder.GetComponent<Renderer>().enabled = true;
        dealerHandValue.text = dealer.handValues[0].ToString();
        dealerHandValue.gameObject.SetActive(true);
        
        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
        SetAvailableActions(handIndex);

        // Offer insurance
        if (dealer.handValues[handIndex] == 11) {
            // do something
        }

        // Check for BJ (check handType instead)

        if (player.handTypes[handIndex] == 'BJ' || dealer.HasBlackjack()) {
            StandClicked();
        }
    }

    // Clear table before a hand is dealt
    private void ClearTable() {
        roundText.gameObject.SetActive(false);
        player.ResetHand();
        handIndex = 0;
        bets = new List<float> {500f};
        dealer.ResetHand();

        playerHandValues[1].gameObject.SetActive(false);
        playerHandValues[2].gameObject.SetActive(false);
        playerHandValues[3].gameObject.SetActive(false);

        // Shuffle shoe if cut card is reached 
        // (NEED TO CHANGE HARD CODED CUT CARD)
        if (deck.currentIndex >= 42) {
            deck.Shuffle();
        }
    }

    // Enable appropriate action buttons on player's turn
    private void SetAvailableActions(int handIndex) {
        // Edge case if hand is already perfect
        if (handIndex < player.handValues.Count && player.handValues[handIndex] == 21) {
            StandClicked();
            return;
        }

        List<CardScript> hand = player.hands[handIndex];
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);


        // Check if the player can double
        if (hand.Count == 2 && SufficientFunds(bets[handIndex])) {
            doubleButton.gameObject.SetActive(true);
            
            // Check if the player can split
            int card1 = hand[0].GetValue();
            int card2 = hand[1].GetValue();
            // Dynamically change what handIndex is less than for max split hands
            if (((card1 == card2) || ((card1 == 1 || card1 == 11) && (card2 == 1 || card2 == 11))) && player.handValues.Count < 4) {
                splitButton.gameObject.SetActive(true);
            }
        }
    }

    // Check if player has sufficient funds for a bet/action
    private bool SufficientFunds(float amount) {
        if (player.GetBalance() - amount >= 0) {
            return true;
        }
        return false;
    }

    private void HitClicked() {
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        if (player.cardsIndex <= 14) {
            player.GetCard(handIndex);
            playerHandValues[handIndex].text = player.handValues[handIndex].ToString();
            SetAvailableActions(handIndex);
            if (handIndex < player.handValues.Count && player.handValues[handIndex] > 21) {
                StandClicked();
            }
        }
    }

    private void StandClicked() {
        playerHandIndicators[handIndex].gameObject.SetActive(false);
        handIndex++;
        
        // Check if no more hands to be played
        if (handIndex == player.hands.Count) {  
            RoundOver();
            return;
        } 
        player.cardsIndex = 2; // Next card to be flipped over in new hand
        playerHandIndicators[handIndex].gameObject.SetActive(true);
        SetAvailableActions(handIndex);
    }

    private void SplitClicked() {
        // Add another bet for other hand
        player.AdjustBalance(-bets[handIndex]);
        bets.Add(bets[handIndex]);
        playerBet.text = "Bet: " + totalBet().ToString();
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
        
        player.SplitHand(handIndex);
        int newHandIndex = player.handValues.Count - 1;
        playerHandValues[handIndex].text = player.handValues[handIndex].ToString();
        playerHandValues[newHandIndex].text = player.handValues[newHandIndex].ToString();
        playerHandValues[newHandIndex].gameObject.SetActive(true);

        SetAvailableActions(handIndex);
        if (player.handValues[handIndex] == 21) {
            StandClicked();
        }

        foreach (List<CardScript> hand in player.hands) {
            print("Hand (i) : ");
            foreach (CardScript card in hand) {
                print(card.GetValue());
            }
        }
    }

    private void DoubleClicked() {
        // Take original bet and double it
        player.AdjustBalance(-bets[handIndex]);
        bets[handIndex] *= 2;
        playerBet.text = "Bet: " + totalBet().ToString();
        playerBalance.text = "Balance: " + player.GetBalance().ToString();

        int prevHandIndex = handIndex;
        HitClicked();
        // Avoid an extra StandClicked if already occured in HitClicked()
        if (prevHandIndex == handIndex) {    
            StandClicked();
        }
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
        bool dealerBJ = dealer.HasBlackjack();
        blinder.GetComponent<Renderer>().enabled = false;           // Show hole card
        dealer.handValues[0] += dealer.GetHoleCard();               // Add hole card to handValue
        dealerHandValue.text = dealer.handValues[0].ToString();     // Update text
        //StartCoroutine(HitDealer());    
        while (dealer.handValues[0] < 17) {
            HitDealer();
        }
        bool dealerBust = 21 < dealer.handValues[0];
        if (dealerBJ) { 
            dealerHandValue.text = "BJ";
        }
        

        // Address all player's hands
        bool playerBJ = player.HasBlackjack();
        float reward = 0f;

        handIndex = 0;
        while (handIndex < player.handValues.Count) {
            int handValue = player.handValues[handIndex];
            bool playerBust = 21 < handValue;
            reward = 0;

            if (playerBJ && !dealerBJ) {
                roundText.text = "Blackjack";
                playerHandValues[handIndex].text = "BJ";
                reward = (2.5f * bets[handIndex]);
            }
            // if player busts or player hand < dealer hand, player loses
            else if (playerBust ||  ((!dealerBust) && (handValue < dealer.handValues[0]))) {
                roundText.text = "Lose";
            }
            // if dealer busts or player hand > dealer hand, player wins
            else if (dealerBust || ((!playerBust) && (dealer.handValues[0] < handValue))) {
                roundText.text = "Win";
                reward = (2 * bets[handIndex]);
            }
            else if (handValue == dealer.handValues[0]) {
                roundText.text = "Push";
                reward = (bets[handIndex]);
            }
            player.AdjustBalance(reward);
            handIndex++;
        }

        // Set UI for next move / hand / turn
        roundText.gameObject.SetActive(true);
        dealButton.gameObject.SetActive(true);

        playerBalance.text = "Balance: " + player.GetBalance().ToString();
    }
}