using System;
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
    public int handIndex = 0;
    private float totalBet = 0f;
    private float startingBet = 10.0f;
    private List<float> playerBets = new List<float> {0f};
    private List<float> playerSideBets = new List<float> {0f};  // insurance, etc.
    public float playerTotalReward;

    // Player HUD
    public TMP_Text playerBalanceText;
    public TMP_Text playerTotalSideBetText;
    public TMP_Text playerTotalBetText;
    public TMP_Text dealerHandValueText;
    public TMP_Text [] playerHandValuesText;        // pre-defined in Unity editor
    public TMP_Text [] playerHandIndicatorsText;    // pre-defined in Unity editor
    public TMP_Text [] playerHandResultsText;       // pre-defined in Unity editor
    public TMP_Text [] playerBetsText;              // pre-defined in Unity editor

    public TMP_Text [] playerSideValuesText;
    public TMP_Text [] playerSideResultsText;       // pre-defined in Unity editor
    public TMP_Text [] playerSideBetsText;          // pre-defined in Unity editor
    public TMP_Text insuranceText;                  // offer insurance? 
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

    // MLAgent Event
    public event Action OnRoundOver;


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
        yesInsuranceButton.onClick.AddListener(() => InsuranceClicked(true));
        noInsuranceButton.onClick.AddListener(() => InsuranceClicked(false));
        
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
        playerTotalBetText.text = "Total Bet: " + startingBet.ToString();
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

        // Cannot surpass max bet
        if (newBet > maxBet) {
            float betDifference = newBet - maxBet;
            newBet -= betDifference;
        }

        // Process and update starting bet variables
        if (betAllowed) {
            startingBet = newBet;
            playerTotalBetText.text = "Total Bet: " + startingBet.ToString();
            clearBetButton.gameObject.SetActive(true);
        }
    }

    // Deal initial cards to both player and dealer
    public void DealClicked() {
        ClearTable();
        
        // Close and process betting
        SetBettingAvailability(false);
        ProcessBet(handIndex, startingBet);
             
        // Deal cards to player
        player.DealHand();
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];
        playerHandValuesText[handIndex].text = playerHandType + playerHandValue.ToString();
        
        // Deal cards to dealer
        dealer.DealHand();
        blinder.GetComponent<Renderer>().enabled = true;
        int dealerHandValue = dealer.handValues[0];
        dealerHandValueText.text = dealerHandValue.ToString();

        // Update HUD
        playerHandValuesText[handIndex].gameObject.SetActive(true);
        playerHandIndicatorsText[handIndex].gameObject.SetActive(true);
        playerBetsText[handIndex].gameObject.SetActive(true);
        dealerHandValueText.gameObject.SetActive(true);
        dealButton.gameObject.SetActive(false);

        // Check for potential Blackjack
        if (dealerHandValue >= 10) {
            // Offer insurance if up card is an Ace (side-bet)
            float insuranceCost = startingBet / 2;
            if (I && dealerHandValue == 11 && player.HasSufficientFunds(insuranceCost)) {
                SetInsuranceAvailability(true);   // true = offer insurance
                return;
            }
            RevealBlackjacks();
        }
        else {  // play on normally
            SetAvailableActions(handIndex);
        }
    }

    // Offer insurance based on given 'availability'
    // true: do offer insurance
    // false: do NOT offer insurance
    private void SetInsuranceAvailability(bool availability) {
        insuranceText.gameObject.SetActive(availability);
        yesInsuranceButton.gameObject.SetActive(availability);
        noInsuranceButton.gameObject.SetActive(availability);
    }

    // Conclude round if player or dealer has Blackjack
    private bool RevealBlackjacks() {
        string playerHandType = player.handTypes[0];
        string dealerHandType = dealer.handTypes[0];
        if (playerHandType == "BJ" || dealerHandType == "BJ") {
            StandClicked();
            return true;
        }
        SetAvailableActions(handIndex);
        return false;
    }

    // Clear table
    private void ClearTable() {
        // Remove in game text
        roundText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);

        // Remove buttons (agent)
        DisableActionButtons();

        // Reset Player Variables
        player.ResetHand();
        handIndex = 0;
        playerBets = new List<float> {0f};
        playerSideBets = new List<float> {0f};
        totalBet = 0f;
        playerTotalReward = 0f;

        // Reset DealerVariables
        dealer.ResetHand();

        // Player HUD Variables
        for (int i = 0; i < maxSplit; i++) {
            playerHandValuesText[i].text = "";
            playerHandResultsText[i].text = "";
            playerBetsText[i].text = "";

            playerHandIndicatorsText[i].gameObject.SetActive(false);
            playerHandValuesText[i].gameObject.SetActive(false);
            playerHandResultsText[i].gameObject.SetActive(false);
            playerBetsText[i].gameObject.SetActive(false);
        }
    
        // increase i as more side bets are incldued
        for (int i = 0; i < 1; i++) {
            playerSideValuesText[i].gameObject.SetActive(false);
            playerSideResultsText[i].gameObject.SetActive(false);
            playerSideBetsText[i].gameObject.SetActive(false);
        }
        playerTotalSideBetText.text = "Side Bets: " + playerSideBets[0].ToString();

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
        player.AdjustBalance(-betAmount);
        totalBet += betAmount;
        playerBets[handIndex] += betAmount;
        playerBetsText[handIndex].text = playerBets[handIndex].ToString();
        playerTotalBetText.text = "Total Bet: " + totalBet.ToString();
        playerBalanceText.text = "Balance: " + player.GetBalance().ToString();
    }

    // Enable appropriate action buttons on player's turn
    private void SetAvailableActions(int handIndex) {
        // Edge case of being called one too many
        if (handIndex >= player.handValues.Count) {
            return;
        }

        // Edge case if hand is 21
        if (handIndex < player.handValues.Count && player.handValues[handIndex] == 21) {
            StandClicked();
            return;
        }
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
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
    public void HitClicked() {
        // Hit current hand
        player.GetCard(handIndex);
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];
        playerHandValuesText[handIndex].text = playerHandType + playerHandValue.ToString();
        if (player.handValues[handIndex] > 21) {    // checks if player busted
            playerHandResultsText[handIndex].text = "Bust";
            playerHandResultsText[handIndex].gameObject.SetActive(true);
            StandClicked();
            return;
        }
        SetAvailableActions(handIndex);             // provides available actions

        // Check if no more space on table to keep hitting
        int cardsIndex = player.cardsIndexes[handIndex];
        if (cardsIndex == 14) {   // A,A,A,A,A,A,A,A,2,2,2,2,2,2
            StandClicked();
        }
    }

    /*
    // Update HUD for player's/dealer's hand value
    private void UpdateHandValueHUD(PlayerScript player) {
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];
        playerHandValuesText[handIndex].text = playerHandType + playerHandValue.ToString();
        playerHandTypesText[handIndex].text = player.handTypes[handIndex];
    }

    // Update HUD for player's/dealer's hand 'result' 
    private void UpdateHandResultHUD(string result) {
        
    }

    // Update HUD for player's bet
    private void UpdateHandBetHUD() {
        int handBet = bets[handIndex];

    }
    */

    // Player stands
    public void StandClicked() {
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
            HitClicked();
        }

        SetAvailableActions(handIndex);
    }

    // Player Spltis
    public void SplitClicked() {
        // Add bet for additional hand
        playerBets.Add(0f);
        int newHandIndex = player.handValues.Count;
        ProcessBet(newHandIndex, startingBet);

        // Split current hand
        player.SplitHand(handIndex);

        // Update HUD for current hand
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];
        playerHandValuesText[handIndex].text = playerHandType + playerHandValue.ToString();

        // Update HUD for new hand
        int playerNewHandValue = player.handValues[newHandIndex];
        string playerNewHandType = player.handTypes[newHandIndex];
        playerHandValuesText[newHandIndex].text = playerNewHandType + playerNewHandValue.ToString();
        playerHandValuesText[newHandIndex].gameObject.SetActive(true);

        // Show Bet of new hand
        float newHandBet = playerBets[newHandIndex];
        playerBetsText[newHandIndex].text = newHandBet.ToString();
        playerBetsText[newHandIndex].gameObject.SetActive(true);

        HitClicked();
        
        // Cannot resplit aces
        if (!RSA && playerNewHandValue == 11) {
            if (handIndex < newHandIndex){
                StandClicked(); // avoids extra StandClicked if first hand is 21
            }
            if (newHandIndex == handIndex) {
                StandClicked(); // avoids extra StandClicked if second hand is 21
            }
        }
    }

    // Player doubles
    public void DoubleClicked() {
        // Double bet of current hand
        ProcessBet(handIndex, startingBet);

        // Get one card
        int prevHandIndex = handIndex;
        HitClicked();
        if (prevHandIndex == handIndex) {   // avoids extra StandClicked if triggered in HitClicked
            StandClicked();
        }
    }

    // Player takes/refuses insurance
    public void InsuranceClicked(bool insured) {
        // Add side bet
        if (insured) {
            float insuranceBet = startingBet / 2;
            player.AdjustBalance(-insuranceBet);
            playerSideBets[0] = insuranceBet;    // 0th index for insurance
            totalBet += insuranceBet;
            // Display something on screen
            playerBalanceText.text = "Balance: " + player.GetBalance();
            playerTotalSideBetText.text = "Side Bets: " + insuranceBet.ToString();
            playerTotalBetText.text = "Total Bet: " + totalBet.ToString();
            
            playerSideValuesText[0].gameObject.SetActive(true);
            playerSideBetsText[0].text = insuranceBet.ToString();
            playerSideBetsText[0].gameObject.SetActive(true);
        }

        insuranceText.gameObject.SetActive(false);
        yesInsuranceButton.gameObject.SetActive(false);
        noInsuranceButton.gameObject.SetActive(false);

        ProcessInsuranceBet(insured);
        RevealBlackjacks();  // ORDER IMPORTANT
    }

    // Reward player appropriately for insurance bet
    private void ProcessInsuranceBet(bool insured) {
        if (!insured) {
            return;
        }

        bool dealerBJ = (dealer.handTypes[0] == "BJ");
        float sideBet = playerSideBets[0];
        float reward = 0f;
        playerTotalReward -= sideBet;
        // Process insurance side bet
        playerSideResultsText[0].text = "Lose";
        if (dealerBJ && insured) {
            reward = 3 * playerSideBets[0];
            playerSideResultsText[0].text = "Win";
        }
        playerTotalReward += reward;

        if (insured) {
            playerSideBetsText[0].gameObject.SetActive(true);
            playerSideResultsText[0].gameObject.SetActive(true);
        }

        player.AdjustBalance(reward);   
    }

    // Player's turn is over
    private void RoundOver() {
        DisableActionButtons();

        // Dealer plays their turn
        PlayDealer();        
        
        // Process player's rewards
        ProcessPlayerHands();

        /*
        // Adress side-bet (insurance)
        if (I && dealerBJ && hasInsurance) {
            totalReward += startingBet;
        }
        */

        // Display round results
        rewardText.text = playerTotalReward.ToString();
        if (playerTotalReward > 0) {
            roundText.text = "You Win";
        } 
        else if (playerTotalReward < 0) {
            roundText.text = "You Lose";
        }
        else if (playerTotalReward == 0) {
            roundText.text = "Push";
            rewardText.text = "";
        }
        roundText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        playerBalanceText.text = "Balance: " + player.GetBalance().ToString();

        // Set UI for next move / hand / turn
        dealButton.gameObject.SetActive(true);
        SetBettingAvailability(true);

        // Triger ML AGENT Round End
        OnRoundOver?.Invoke();
    }

    // Dealer player's their turn and return their hand value
    private void PlayDealer() {
        // Dealer reveals their hand
        blinder.GetComponent<Renderer>().enabled = false;               // Show hole card
        dealer.handValues[0] += dealer.GetHoleCard();                   // Add hole card to handValue

        // Dealer keeps hitting until stopping condition
        while (true) {
            // Get dealer hand value and handType
            int dealerHandValue = dealer.handValues[0];
            string dealerHandType = dealer.handTypes[0];
            dealerHandValueText.text = dealerHandType + dealerHandValue.ToString();  

            // Define stopping condition
            bool canHit = (dealerHandValue < 17 || dealerHandValue == 17 && dealerHandType == "S");
            if (!H17) {     // Game is S17
                canHit = (dealerHandValue < 17);
            }
            
            // Check if dealer should stop hitting
            if (!canHit) {
                break;
            }
            dealer.GetCard(0);  // hit dealer
        }
    }

    // Reward player appropriately for each of their hands
    private void ProcessPlayerHands() {
        // Define necessary dealer variables
        bool dealerBJ = (dealer.handTypes[0] == "BJ");              
        int dealerHandValue = dealer.handValues[0];
        bool dealerBust = 21 < dealerHandValue;

        // Pre-processing to address each hand
        handIndex = 0;
        bool playerBJ = (player.handTypes[0] == "BJ");
        while (handIndex < player.handValues.Count) {
            int playerHandValue = player.handValues[handIndex];
            bool playerBust = 21 < playerHandValue;
            float handBet = playerBets[handIndex];
            float reward = 0;
            playerTotalReward -= handBet;

            // player wins with BJ
            if (playerBJ && !dealerBJ) {
                roundText.text = "Blackjack";
                playerHandResultsText[handIndex].text = "Win";
                reward = (2.5f * handBet);
            }
            // player loses
            else if (playerBust ||  ((!dealerBust) && (playerHandValue < dealerHandValue))) {
                playerHandResultsText[handIndex].text = "Lose";
            }
            // player wins
            else if (dealerBust || ((!playerBust) && (dealerHandValue < playerHandValue))) {
                playerHandResultsText[handIndex].text = "Win";
                reward = (2 * handBet);
            }
            // player pushes
            else if (playerHandValue == dealerHandValue) {
                playerHandResultsText[handIndex].text = "Push";
                reward = (handBet);
            }

            player.AdjustBalance(reward);   // Adjusting real balance
            playerTotalReward += reward;
            playerHandResultsText[handIndex].gameObject.SetActive(true);
            handIndex++;
        }
    }

    // Disable action buttons
    private void DisableActionButtons() {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);
        insuranceText.gameObject.SetActive(false);
        yesInsuranceButton.gameObject.SetActive(false);
        noInsuranceButton.gameObject.SetActive(false);
    }
}