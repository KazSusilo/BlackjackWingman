using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // --- This script is for managing the flow of the game
    // Game Settings
    private float penetration;  // 6/8
    private bool H17;           // Hard 17 || Soft 17
    private bool D;             // double
    private bool DAS;           // double after splitting
    private bool RSA;           // re-split aces
    private int maxSplit;       // number of hands a player can have
    private bool NS;            // no-surender
    private float maxBet;       // max starting bet
    private bool I;             // insurance

    // Shoe
    public ShoeScript shoe;

    // Dealer
    public PlayerScript dealer;
    public GameObject blinder;  // blinder hiding dealer's hole card

    // Player 
    public PlayerScript player;
    private int handIndex = 0;
    private float totalBet = 0f;
    private float startingBet = 0f;
    private List<float> bets = new List<float> {0f};
    private bool hasInsurance = false;

    // Player HUD
    public TMP_Text playerBalanceText;
    public TMP_Text playerTotalBetText;
    public TMP_Text dealerHandValueText;
    public TMP_Text dealerHandTypeText;
    public TMP_Text [] playerHandValuesText;        // pre-defined in Unity editor
    public TMP_Text [] playerHandTypesText;         // pre-defined in Unity editor
    public TMP_Text [] playerHandIndicatorsText;    // pre-defined in Unity editor
    public TMP_Text [] playerHandResultsText;       // pre-defined in Unity editor
    public TMP_Text [] playerBetsText;              // pre-defined in Unity editor
    public TMP_Text roundText;                      // ("Win", "Lose", "Push", etc)
    public TMP_Text rewardText;                     // how much the player wins/loses

    // Action Buttons
    public Button dealButton;
    public Button hitButton;
    public Button standButton;
    public Button splitButton;
    public Button doubleButton;
    public Button yesInsuranceButton;
    public Button noInsuranceButton;

    // Bet Buttons
    public Button clearBetButton;
    public Button bet1Button;
    public Button bet5Button;
    public Button bet25Button;
    public Button bet100Button;
    public Button bet500Button;


    // Start is called before the first frame update
    private void Start() {
        shoe.Shuffle();
        player.InitializePlayerType(true);  // isPlayer = true
        dealer.InitializePlayerType(false); // isPlayer = false
        InitializeGame();   // Eventually call outside in menu

        // Listener for Action Buttons
        dealButton.onClick.AddListener(() => DealClicked());
        hitButton.onClick.AddListener(() => HitClicked());
        standButton.onClick.AddListener(() => StandClicked());
        splitButton.onClick.AddListener(() => SplitClicked());
        doubleButton.onClick.AddListener(() => DoubleClicked());
        yesInsuranceButton.onClick.AddListener(() => TakeInsuranceClicked(true));
        noInsuranceButton.onClick.AddListener(() => TakeInsuranceClicked(false));
        
        // Listener for Bet Buttons
        clearBetButton.onClick.AddListener(() => ClearBetClicked());
        bet1Button.onClick.AddListener(() => BetXClicked(1));
        bet5Button.onClick.AddListener(() => BetXClicked(5));
        bet25Button.onClick.AddListener(() => BetXClicked(25));
        bet100Button.onClick.AddListener(() => BetXClicked(100));
        bet500Button.onClick.AddListener(() => BetXClicked(500));
    }

    // Initialize table rules
    public void InitializeGame() {
        penetration = 6/8f;
        H17 = false;
        D = true;
        DAS = true;
        RSA = false;
        maxSplit = 4;
        NS = true;
        maxBet = 1000f;
        I = true;
    }

    // Clear starting bet
    private void ClearBetClicked() {
        startingBet = 0f;
        playerTotalBetText.text = startingBet.ToString();
        clearBetButton.gameObject.SetActive(false);
    }

    // Add xBetAmount to player Bet
    private void BetXClicked(float xBetAmount) {
        // Betting variables
        bool betAllowed = true;
        float newBet = startingBet + xBetAmount;
        
        // Check if bet is NOT allowed
        if (!player.HasSufficientFunds(newBet)) {
            print("Insufficient funds");
            betAllowed = false;
        }
        else if (newBet > maxBet) {
            print("Unable to surpass Max Bet");
            betAllowed = false;
        }

        // Process and update starting bet variables
        if (betAllowed) {
            startingBet = newBet;
            playerTotalBetText.text = startingBet.ToString();
            clearBetButton.gameObject.SetActive(true);
        }
    }

    // Deal initial cards to both player and dealer
    private void DealClicked() {
        ClearTable();
        
        // Close and process betting
        SetBettingAvailability(false);
        ProcessBet(handIndex, startingBet);
             
        // Deal cards to player
        player.DealHand();
        playerHandValuesText[handIndex].text = player.handValues[handIndex].ToString();
        playerHandTypesText[handIndex].text = player.handTypes[0]; 

        // Update HUD
        playerHandValuesText[handIndex].gameObject.SetActive(true);
        playerHandTypesText[handIndex].gameObject.SetActive(true);
        playerHandIndicatorsText[handIndex].gameObject.SetActive(true);
        
        // Deal cards to dealer
        dealer.DealHand();
        blinder.GetComponent<Renderer>().enabled = true;
        dealerHandValueText.text = dealer.handValues[0].ToString();
        dealerHandTypeText.text = dealer.handTypes[0];

        // Update HUD
        dealerHandValueText.gameObject.SetActive(true);
        dealerHandTypeText.gameObject.SetActive(true);
        
        // Offer insurance (side-bet)
        float insuranceCost = startingBet / 2;
        if (I && dealer.handValues[handIndex] == 11 && player.HasSufficientFunds(insuranceCost)) {
            yesInsuranceButton.gameObject.SetActive(true);
            noInsuranceButton.gameObject.SetActive(true);
        }

        // Check for BJ
        string playerHandType = player.handTypes[0];
        string dealerHandType = dealer.handTypes[0];
        if (playerHandType == "BJ" || dealerHandType == "BJ") {
            StandClicked();
            return;
        }

        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
        SetAvailableActions(handIndex);
    }

    // Clear table
    private void ClearTable() {
        roundText.gameObject.SetActive(false);
        player.ResetHand();
        handIndex = 0;
        bets = new List<float> {500f};
        dealer.ResetHand();

        playerHandValuesText[0].gameObject.SetActive(false);
        playerHandValuesText[1].gameObject.SetActive(false);
        playerHandValuesText[2].gameObject.SetActive(false);
        playerHandValuesText[3].gameObject.SetActive(false);

        // Shuffle shoe if cut card is reached 
        int cutCard = (int)(shoe.totalCards * penetration);
        if (cutCard <= shoe.shoeIndex) {
            shoe.Shuffle();
        }
    }

    // Enable/Disable Betting Buttons
    private void SetBettingAvailability(bool availability) {
        clearBetButton.gameObject.SetActive(availability);
        bet1Button.gameObject.SetActive(availability);
        bet5Button.gameObject.SetActive(availability);
        bet25Button.gameObject.SetActive(availability);
        bet100Button.gameObject.SetActive(availability);
        bet500Button.gameObject.SetActive(availability);
    }

    // Processes and updates total bet variables
    private void ProcessBet(int handIndex, float betAmount) {
        totalBet += betAmount;
        player.AdjustBalance(-totalBet);
        bets[handIndex] = totalBet;
        playerBetsText[handIndex].text = bets[handIndex].ToString();
        playerTotalBetText.text = "Total Bet: " + totalBet.ToString();
        playerBalanceText.text = "Balance: " + player.GetBalance().ToString();
    }

    // Enable appropriate action buttons on player's turn
    private void SetAvailableActions(int handIndex) {
        // Edge case if hand is 21 (not BJ)
        if (player.handValues[handIndex] == 21) {
            StandClicked();
            return;
        }
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);

        // Address current hand
        List<CardScript> hand = player.hands[handIndex];

        // Check if the player can double
        float additionalBet = startingBet;
        if (hand.Count == 2 && player.HasSufficientFunds(additionalBet)) {
            doubleButton.gameObject.SetActive(true);
            
            // Check if the player can split
            int card1 = hand[0].GetValue();
            int card2 = hand[1].GetValue();
            if (((card1 == card2) || ((card1==1||card1==11) && (card2==1||card2==11))) && (player.handValues.Count < maxSplit)) {
                splitButton.gameObject.SetActive(true);
            }
        }
    }

    // Player hits
    private void HitClicked() {
        // Hit current hand
        player.GetCard(handIndex);
        playerHandValuesText[handIndex].text = player.handValues[handIndex].ToString();
        playerHandTypesText[handIndex].text = player.handTypes[handIndex];
        if (player.handValues[handIndex] > 21) {    // checks if player busted
            StandClicked();
            return;
        }
        SetAvailableActions(handIndex);             // provides available actions

        // Check if no more space on table to keep hitting
        int cardsIndex = player.cardsIndexes[handIndex];
        if (cardsIndex < 14) {   // A,A,A,A,A,A,A,A,2,2,2,2,2,2
            StandClicked();
        }
    }

    // Player stands
    private void StandClicked() {
        // Stand current hand
        playerHandIndicatorsText[handIndex].gameObject.SetActive(false);
        handIndex++;
        
        // Check if no more hands to be played
        if (handIndex == player.hands.Count) {  
            RoundOver();
            return;
        } 

        // Move to next hand
        playerHandIndicatorsText[handIndex].gameObject.SetActive(true);
        
        // Edge case when hand only has one card after splitting
        if (player.hands[handIndex].Count < 2) {
            player.GetCard(handIndex);
        }

        SetAvailableActions(handIndex);
    }

    // Player Spltis
    private void SplitClicked() {
        // Add bet for additional hand
        bets.Add(0f);
        int newHandIndex = player.handValues.Count;
        ProcessBet(newHandIndex, startingBet);

        // Split current hand
        player.SplitHand(handIndex);
        playerHandValuesText[handIndex].text = player.handValues[handIndex].ToString();
        playerHandValuesText[newHandIndex].text = player.handValues[newHandIndex].ToString();
        playerHandValuesText[newHandIndex].gameObject.SetActive(true);

        SetAvailableActions(handIndex);
    }

    // Player doubles
    private void DoubleClicked() {
        // Double bet of current hand
        ProcessBet(handIndex, startingBet);

        // Get one card
        HitClicked();
        int prevHandIndex = handIndex;
        if (prevHandIndex == handIndex) {   // avoids extra StandClicked if triggered in HitClicked
            StandClicked();
        }
    }

    // Player takes/refuses insurance
    private void TakeInsuranceClicked(bool takeInsurance) {
        hasInsurance = takeInsurance;

        // Add side bet
    }

    // Player's turn is over
    private void RoundOver() {
        DisableActionButtons();

        // Dealer plays their turn
        PlayDealer();        
        
        // Define necessary dealer variables
        bool dealerBJ = (dealer.handTypes[0] == "BJ");              
        int dealerHandValue = dealer.handValues[0];
        bool dealerBust = 21 < dealerHandValue;
        
        // Address all player's hands
        bool playerBJ = (player.handTypes[0] == "BJ");
        handIndex = 0;
        float totalReward = 0f;
        while (handIndex < player.handValues.Count) {
            int playerHandValue = player.handValues[handIndex];
            bool playerBust = 21 < playerHandValue;
            playerHandTypesText[handIndex].text = player.handTypes[handIndex];
            float reward = 0f;

            // player wins with BJ
            if (playerBJ && !dealerBJ) {
                roundText.text = "Blackjack";
                playerHandResultsText[handIndex].text = "Win";
                reward = (2.5f * bets[handIndex]);
            }
            // player wins
            else if (dealerBust || ((!playerBust) && (dealerHandValue < playerHandValue))) {
                playerHandResultsText[handIndex].text = "Win";
                reward = (2 * bets[handIndex]);
            }
            // player loses
            else if (playerBust ||  ((!dealerBust) && (playerHandValue < dealerHandValue))) {
                playerHandResultsText[handIndex].text = "Lose";
            }
            // player pushes
            else if (playerHandValue == dealerHandValue) {
                playerHandResultsText[handIndex].text = "Push";
                reward = (bets[handIndex]);
            }

            totalReward += reward;
            handIndex++;
        }

        // Adress side-bet (insurance)
        if (I && dealerBJ && hasInsurance) {
            totalReward += startingBet;
        }

        // Round results
        player.AdjustBalance(totalReward);
        if (totalReward > 0) {
            roundText.text = "You Win";
            rewardText.text = totalReward.ToString();
        } 
        else if (totalReward < 0) {
            roundText.text = "You Lose";
            rewardText.text = (-(totalBet - totalReward)).ToString();
        }
        else if (totalReward == 0) {
            roundText.text = "You Broke Even";
            rewardText.text = "";
        }
        roundText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        playerBalanceText.text = "Balance: " + player.GetBalance().ToString();

        // Set UI for next move / hand / turn
        dealButton.gameObject.SetActive(true);
        SetBettingAvailability(true);
    }

    // Dealer player's their turn and return their hand value
    private void PlayDealer() {
        // Dealer reveals their hand
        blinder.GetComponent<Renderer>().enabled = false;               // Show hole card
        dealer.handValues[0] += dealer.GetHoleCard();                   // Add hole card to handValue
        int dealerHandValue = dealer.handValues[0];
        dealerHandValueText.text = dealerHandValue.ToString();          // Update text

        // Dealer keeps hitting until stopping condition
        while (true) {
            // Define stopping condition
            dealerHandValue = dealer.handValues[0];
            string dealerHandType = dealer.handTypes[0];
            bool canHit = (dealerHandValue < 17 || dealerHandValue == 17 && dealerHandType == "S");
            if (!H17) {     // Game is S17
                canHit = (dealerHandValue < 17);
            }
            
            // Hit until unable
            if (!canHit) {
                break;
            }
            HitDealer();
        }
    }

    // Dealer hits
    private void HitDealer() {
        dealer.GetCard(0);
        dealerHandValueText.text = dealer.handValues[0].ToString();
        dealerHandTypeText.text = dealer.handTypes[0];
    }

    // Disable all action buttons
    private void DisableActionButtons() {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);

    }
}