using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WingmanAgent : Agent {
    // Access other scripts
    public PlayerScript player;
    public PlayerScript dealer;
    public GameManager gameManager;
    public ShoeScript Shoe;
    public BasicStrategy basicStrategy;

    // Variables regarding episode
    [HideInInspector] public int CurrentEpisode;
    [HideInInspector] public float CumulativeReward;
    [HideInInspector] public TMP_Text currentEpisodeText;

    // Variables regarding training
    private bool _environmentReset;
    private bool _terminalPhase;
    private bool _insuranceOffered;
    private int? _trueCountTraining;    // trueCount to train on

    // Variables regarding performance
    [HideInInspector] public float BetUnit;
    [HideInInspector] public float UnitsWagered;
    [HideInInspector] public float UnitsWon;
    [HideInInspector] public float UnitsPushed;
    [HideInInspector] public float UnitsLost;


    // Called when the Agent is first created 
    public override void Initialize() {
        Debug.Log("Initialize Agent");

        // Turn off automatic stepping
        //Academy.Instance.AutomaticSteppingEnabled = false;

        // Initialize episode variables
        CurrentEpisode = 0;
        CumulativeReward = 0;
        
        // Initialize training variables
        _environmentReset = false;
        _terminalPhase = false;
        _insuranceOffered = false;
        _trueCountTraining = null;  // null: train across all counts

        // Initialize performance variables
        BetUnit = 10.0f;
        UnitsWagered = 0.0f;
        UnitsWon = 0.0f;
        UnitsLost = 0.0f;
        UnitsPushed = 0.0f;

        // Listeners for request decision and round end
        gameManager.OnRoundOver += OnRoundOver;
    }

    // Reset the environment at the start of each episode
    public override void OnEpisodeBegin() {
        Debug.Log("Begin New Episode");

        // Reset episode variables
        CurrentEpisode++;
        CumulativeReward = 0f;
        
        // Reset training variables
        _terminalPhase = false;
        _insuranceOffered = false;
        
        // Generate a Blackjack round
        _environmentReset = false;
        GenerateRound();
        void GenerateRound() {
            while (true) {
                // Reset any game state to betting/deal state
                ResetRound();
                void ResetRound() {
                    // Handle insurance offer
                    if (gameManager.noInsuranceButton.gameObject.activeSelf) {
                        gameManager.InsuranceClicked(false);
                    }
            
                    // Handle as many hands as necessary
                    while (!gameManager.dealButton.gameObject.activeSelf) {
                        gameManager.StandClicked();
                    }
                }

                // Reset player balance to 100
                player.AdjustBalance(100.0f - player.GetBalance());
                gameManager.DealClicked();

                // Check if valid shoe to play
                if (_trueCountTraining == null ||  Shoe.trueCount == _trueCountTraining) {
                    // valid shoe, add wager
                    UnitsWagered += 1.0f;   // valid shoe, add wager
                    // Edge case - don't train on 'frame 1' terminating round states
                    // frame1 terminating rounds still used for 'edge' calculation in OnRoundOver()
                    if (!CheckFrame1TerminalState()) {
                        break;
                    }
                }
            }
        }
        _environmentReset = true;
    }

    // Gather information about the current state of the environment
    public override void CollectObservations(VectorSensor sensor) {
        Debug.Log("Collecting Observations");

        // Flag if in deicision mode (0) or terminal mode (1)
        float terminalPhase_encoded = (_terminalPhase == true) ? 1.0f : 0.0f;
        sensor.AddObservation(_terminalPhase);  // [1]
        
        // Collect observations regarding the player's hand(s) [24]
        CollectHandObservations(sensor);
        void CollectHandObservations(VectorSensor sensor) {
            // Hand Observations (encoded / normalized)
            float[] handExistence = new float[] {0.0f, 0.0f, 0.0f, 0.0f};   // h1, h2, h3, h4
            float[] handActivity = new float[] {0.0f, 0.0f, 0.0f, 0.0f};    // h1, h2, h3, h4
            float[] hand1Details = new float[] {-1.0f, -1.0f, 0.0f, -2.0f}; // type, value, bet, result
            float[] hand2Details = new float[] {-1.0f, -1.0f, 0.0f, -2.0f}; // type, value, bet, result
            float[] hand3Details = new float[] {-1.0f, -1.0f, 0.0f, -2.0f}; // type, value, bet, result
            float[] hand4Details = new float[] {-1.0f, -1.0f, 0.0f, -2.0f}; // type, value, bet, result
            for (int i = 0; i < 4; i++) {
                // Hand Flags (0 or 1)
                handExistence[i] = (i + 1 <= player.handValues.Count) ? 1.0f : 0.0f;
                handActivity[i] = (i == gameManager.handIndex) ? 1.0f : 0.0f;

                // Hand[i] Details
                float handType_encoded = -1.0f;
                float handValue_normalized = -1.0f;
                float handBet_normalized = 0.0f;
                float handResult_encoded = -2.0f;
                if (handExistence[i] == 1.0f) {
                    // Hand Type (0 to 2) | -1 Invald
                    string handType = player.handTypes[i];
                    if (handType == "H") {
                        handType_encoded = 0.0f;
                    }
                    else if (handType == "S") {
                        handType_encoded = 1.0f;
                    } 
                    else if (handType == "BJ") {
                        handType_encoded = 2.0f;
                    }
                    
                    // Hand Value (0 to 1) | -1 Invalid
                    int handValue = player.handValues[i];
                    handValue_normalized = ((float)handValue - 2.0f) / 19.0f; // 2 to 21

                    // Hand Bets (0 to 1) | -1 Invalid 
                    if (gameManager.playerBetsText[i].gameObject.activeSelf) {
                        float bet = -4;
                        float.TryParse(gameManager.playerBetsText[i].text, out bet);
                        handBet_normalized = (bet - BetUnit) / 2f;      // BetUnit to 2xBetUnit
                    }

                    // Hand Results (-1 to 1) | -2 Invalid
                    if (_terminalPhase) {
                        string result = gameManager.playerHandResultsText[i].text.ToString();
                        if (result == "Win") {
                            handResult_encoded = 1.0f;
                        }
                        else if (result == "Push") {
                            handResult_encoded = 0.0f;
                        }
                        else if (result == "Lose") {
                            handResult_encoded = -1.0f;
                        }
                    }
                }

                // Populate Hand[i]
                switch (i) {
                    case 0:
                        hand1Details[0] = handType_encoded;
                        hand1Details[1] = handValue_normalized;
                        hand1Details[2] = handBet_normalized;
                        hand1Details[3] = handResult_encoded;
                        break;
                    case 1:
                        hand2Details[0] = handType_encoded;
                        hand2Details[1] = handValue_normalized;
                        hand2Details[2] = handBet_normalized;
                        hand2Details[3] = handResult_encoded;
                        break;
                    case 2:
                        hand3Details[0] = handType_encoded;
                        hand3Details[1] = handValue_normalized;
                        hand3Details[2] = handBet_normalized;
                        hand3Details[3] = handResult_encoded;
                        break;
                    case 3:
                        hand4Details[0] = handType_encoded;
                        hand4Details[1] = handValue_normalized;
                        hand4Details[2] = handBet_normalized;
                        hand4Details[3] = handResult_encoded;
                        break;
                }
            }

            // Add 24 observations
            sensor.AddObservation(handExistence);   // [4]
            sensor.AddObservation(handActivity);    // [4]
            sensor.AddObservation(hand1Details);    // [4]
            sensor.AddObservation(hand2Details);    // [4]
            sensor.AddObservation(hand3Details);    // [4]
            sensor.AddObservation(hand4Details);    // [4]
        }

        // Collect observations regarding the dealer [2]
        CollectDealerObservations(sensor);
        void CollectDealerObservations(VectorSensor sensor) {
            // Dealer observations (encoded)
            float upCard_normalized = -1.0f;
            float handValue_normalized = -1.0f;
            
            // Dealer up card (0 to 1) | -1 Invalid
            if (!_terminalPhase) {
                int upCard = dealer.GetHoleCard();
                upCard_normalized = ((float)upCard - 2.0f) / 9.0f;         // 2 to 11
            }
            
            // Dealer hand value (0 to 1) | -1 Invalid
            if (_terminalPhase) {
                int handValue = dealer.handValues[0];
                handValue_normalized = ((float)handValue - 17.0f) / 9.0f;  // 17 to 26
            }

            // Add 2 observations
            sensor.AddObservation(upCard_normalized);
            sensor.AddObservation(handValue_normalized);
        }

        // Collect observatiosn regarding the counts [2]
        CollectCountObservations(sensor);
        void CollectCountObservations(VectorSensor sensor) {
            // Count observations (normalized)
            // Running count (-1 to 1)
            int totalDecks = Shoe.totalCards / 52;
            int maxRunningCount = 20 * totalDecks;
            int runningCount = Shoe.runningCount;
            float runningCount_normalized = (float)runningCount / (float)maxRunningCount;

            // True count (-1 to 1)
            int maxTrueCount = 52;
            int trueCount = Shoe.trueCount;
            float trueCount_normalized = (float)trueCount / (float)maxTrueCount;

            // Add 2 observations
            sensor.AddObservation(runningCount_normalized);
            sensor.AddObservation(trueCount_normalized);
        }

        // Collect observations regarding action availability [7]
        CollectActionAvailabilityObservations(sensor);
        void CollectActionAvailabilityObservations(VectorSensor sensor) {
            // Action Availability (encoded) (0 or 1)
            float[] actionAvailability = new float[] {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f}; // S, H, D, P, Y, N, R
            List<bool> availabilities = GetActionAvailability();
            for (int i = 0; i < availabilities.Count; i++) {
                float availability_encoded = 0.0f;
                if (availabilities[i] == true) {
                    availability_encoded = 1.0f;
                }
                actionAvailability[i] = availability_encoded;
            }

            // Add 7 observations
            sensor.AddObservation(actionAvailability);  // [7]
        }

        // Collect obervations regarding the player's side bet(s) [1]
        CollectSideBetObservations(sensor);
        void CollectSideBetObservations(VectorSensor sensor) {
            // Side bet observations (encoded / normalized)
            float insuranceTaken_encoded = -1.0f;
            for (int i = 0; i < gameManager.playerSideValuesText.Length; i++) {
                // Insurance Taken (0 or 1) | -1 Invalid
                if (_insuranceOffered) {
                    if (gameManager.playerSideValuesText[0].gameObject.activeSelf) {
                        insuranceTaken_encoded = 1.0f;
                    }
                    else {
                        insuranceTaken_encoded = 0.0f;
                    }
                }
            }

            // Add 1 observation
            sensor.AddObservation(insuranceTaken_encoded);
        }
    }

    // Manually supply OnActionsReceived() input
    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = -1;     // do nothing
        if (Input.GetKey(KeyCode.S)) {
            discreteActionsOut[0] = 0;  // stand
        }
        else if (Input.GetKey(KeyCode.H)) {
            discreteActionsOut[0] = 1;  // hit
        }
        else if (Input.GetKey(KeyCode.D)) {
            discreteActionsOut[0] = 2;  // double
        }
        else if (Input.GetKey(KeyCode.P)) {
            discreteActionsOut[0] = 3;  // split
        }
        else if (Input.GetKey(KeyCode.Y)) {
            discreteActionsOut[0] = 4;  // take insurance
        }
        else if (Input.GetKey(KeyCode.N)) {
            discreteActionsOut[0] = 5;  // refuse insurance
        }
    }

    // Utilize output from NN
    public override void OnActionReceived(ActionBuffers actions) {
        Debug.Log("ActionReceived");
        // Edge case - don't try to do actions when round is already over
        if (_terminalPhase) {
            return;
        }

        int action = actions.DiscreteActions[0];
        print("Action: " + action);

        // Reward shaping with basic strategy
        RewardShape(action);  // eventually remove
        
        // The agent attempts to perform given action (stand, hit, etc)
        bool performed = PerformAction(action);
        bool PerformAction(int action) {
            bool performedAction = false;
            List<bool> actionAvailability = GetActionAvailability();
            if (action != -1 && actionAvailability[action]) {
                performedAction = true;
                switch (action) {
                    case 0: // Stand
                        gameManager.StandClicked();
                        break;
                    case 1: // Hit               
                        gameManager.HitClicked();
                        break;
                    case 2: // Double
                        UnitsWagered += 1.0f;
                        gameManager.DoubleClicked();
                        break;
                    case 3: // Split
                        UnitsWagered += 1.0f;
                        gameManager.SplitClicked();
                        break;
                    case 4: // Take Insurance
                        UnitsWagered += 0.5f;
                        gameManager.InsuranceClicked(true); 
                        break;
                    case 5: // Refuse Insurance
                        gameManager.InsuranceClicked(false);
                        break;
                }
            }

            return performedAction;
        }

        // Penalty given for attempting an illegal action
        if (!performed) {
            RewardAgent((-2.0f * 2f) / MaxStep);   // Simplify (2 / 37) ~ -0.054
        }
    }

    // Called when GameManager reaches RoundOver()
    public void OnRoundOver() {
        Debug.Log("Ending episode");

        // Edge case - don't process 'GenerateRound()' events 
        if (!_environmentReset) {
            if (CheckFrame1TerminalState()) {
                TallyFrame1TerminalResults();   // include for 'edge' calculation
            }
            return;
        }

        // Calculate reward based on the outcome of the round
        float roundReward = 0.0f;
        float.TryParse(gameManager.rewardText.text, out roundReward);
        float roundReward_normalzied = roundReward / (2 * BetUnit);
        RewardAgent(roundReward_normalzied);

        // Update Performance Variables (insight purposes)
        UpdatePerformanceVariables(roundReward / BetUnit);     // divided by betting units
        void UpdatePerformanceVariables(float rewardUnits) {
            UnitsWon += (0.0f < rewardUnits) ? rewardUnits : 0.0f;
            UnitsPushed += (rewardUnits == 0.0f) ? 1.0f : 0.0f;
            UnitsLost += (rewardUnits < 0.0f) ? -rewardUnits : 0.0f;
        }

        Debug.Log("Win: " + UnitsWon);
        Debug.Log("Push: " + UnitsPushed);
        Debug.Log("Lost: " + UnitsLost);

        // Flag terminal phase
        if (!_terminalPhase) {
            _terminalPhase = true;
            StartCoroutine(DelayedEndEpisode());
        }
    }

    private IEnumerator DelayedEndEpisode() {
        yield return null;
        EndEpisode();
    }

    // Reward or penalize agent with given amount
    private void RewardAgent(float amount) {
        AddReward(amount);
        CumulativeReward = GetCumulativeReward();
    }

    // Provide small reward if given action aligns with Basic Strategy
    private void RewardShape(int action) {
        // The player's current hand
        int handIndex = gameManager.handIndex;
        print("HandIndex: " + handIndex);
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];

        // The dealer's up card
        int dealerUpCard = dealer.handValues[0];

        // Basic strategy action(s)
        List<int> strategies = new List<int>();
        if (gameManager.noInsuranceButton.gameObject.activeSelf) {
            // Insurance
            strategies.Add(5);  // BS never takes insurance
        }
        else if (gameManager.splitButton.gameObject.activeSelf) {
            // Pairs
            int cardValue = player.hands[handIndex][1].GetValue();
            strategies = basicStrategy.CheckMDPairsTable(cardValue, dealerUpCard);
        } 
        else if (playerHandType == "S" && playerHandValue != 12) {
            // Soft hands
            strategies = basicStrategy.CheckMDSoftTable(playerHandValue, dealerUpCard);
        }
        else if (playerHandType == "H") {    
            // Hard hands
            strategies = basicStrategy.CheckMDHardTable(playerHandValue, dealerUpCard);
        }


        // Reward if action aligns with basic strategy
        List<bool> actionAvailability = GetActionAvailability();
        for (int i = 0; i < strategies.Count; i++){
            int strategy = strategies[i];

            if (actionAvailability[strategy] && action != strategy) {
                // Don't reward if preferred action was available
                break;
            }
            else if (action == strategy) {
                AddReward(.075f);
            }
        }
    }

    // Check if terminal state was reached by a 'frame1' terminating round state
    private bool CheckFrame1TerminalState() {
        bool frame1 = false;

        bool playerBJ = player.handTypes[0] == "BJ";
        bool dealerBJ = dealer.handTypes[0] == "BJ";
        GetActionAvailability();
        // Skip BJ wins
        if (playerBJ && !dealerBJ) {
            frame1 = true;
        }
        // Skip BJ pushes
        else if (playerBJ && dealerBJ && !_insuranceOffered) {
            frame1 = true;
        }
        // Skip BJ losses
        else if (!playerBJ && dealerBJ && !_insuranceOffered) {
            frame1 = true;
        }
        
        return frame1;
    }

    // Tally BJ wins/losses/pushes into Units Won/Pushed/Lost
    private void TallyFrame1TerminalResults() {
        bool playerBJ = player.handTypes[0] == "BJ";
        bool dealerBJ = dealer.handTypes[0] == "BJ";
        GetActionAvailability();
        // BJ wins
        if (playerBJ && !dealerBJ) {
            Debug.Log("BJ WIN");
            UnitsWon += 1.5f;
        }
        // BJ pushes
        else if (playerBJ && dealerBJ && !_insuranceOffered) {
            Debug.Log("BJ Push");
            UnitsPushed += 1.0f;
        }
        // BJ losses
        else if (!playerBJ && dealerBJ && !_insuranceOffered) {
            Debug.Log("BJ Loss");
            UnitsLost += 1.0f;
        }

        Debug.Log("Win: " + UnitsWon);
        Debug.Log("Push: " + UnitsPushed);
        Debug.Log("Lost: " + UnitsLost);
    }

    // Returns a list of bools indicating which actions are available
    private List<bool> GetActionAvailability() {
        // Check if each action is available
        bool canStand = gameManager.standButton.gameObject.activeSelf;
        bool canHit = gameManager.hitButton.gameObject.activeSelf;
        bool canDouble = gameManager.doubleButton.gameObject.activeSelf;
        bool canSplit = gameManager.splitButton.gameObject.activeSelf;
        bool canTakeInsurance = gameManager.yesInsuranceButton.gameObject.activeSelf;
        bool canRefuseInsurance = gameManager.noInsuranceButton.gameObject.activeSelf;
        bool canSurrender = false;  // support future implementation of surrender

        // Flag insurance offered
        if (!_insuranceOffered && canTakeInsurance) {
            _insuranceOffered = true;
        }

        // Add actions to list and return 
        List<bool> actionAvailability = new List<bool>();
        actionAvailability.Add(canStand);
        actionAvailability.Add(canHit);
        actionAvailability.Add(canDouble);
        actionAvailability.Add(canSplit);
        actionAvailability.Add(canTakeInsurance);
        actionAvailability.Add(canRefuseInsurance);
        actionAvailability.Add(canSurrender);
        return actionAvailability;
    }
}