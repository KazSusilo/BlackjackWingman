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
    public ShoeScript shoe;
    public BasicStrategy basicStrategy;

    // Variables regarding training
    private int _currentEpisode;
    private float _cumulativeReward;
    public TMP_Text currentEpisodeText;
    public TMP_Text currentStepText;
    public TMP_Text cumulativeRewardText;
    
    // True Count we want the agent to train on 
    [SerializeField] private int _trueCount;


    // Called when the Agent is first created 
    public override void Initialize() {
        Debug.Log("Initialize Agent");

        _currentEpisode = 0;
        _cumulativeReward = 0;
        currentEpisodeText.text = "Episode: " + _currentEpisode.ToString();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();
    }

    // Reset the environment at the start of each episode
    public override void OnEpisodeBegin() {
        Debug.Log("Begin New Episode");

        _currentEpisode++;
        _cumulativeReward = 0f;
        currentEpisodeText.text = "Episode: " + _currentEpisode.ToString();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();

        GenerateRound();
    }

    // Generate a Blackjack round with appropriate TC
    private void GenerateRound() {
        while (true) {
            ResetRound();
            gameManager.DealClicked();
            if (shoe.trueCount == _trueCount) {
                break;
            }
        }

        
    }

    // Reset any game state to betting/deal state
    private void ResetRound() {
        // Handle insurance offer
        if (gameManager.noInsuranceButton.gameObject.activeSelf) {
            gameManager.InsuranceClicked(false);
        }

        // Handle as many hands as necessary
        while (!gameManager.dealButton.gameObject.activeSelf) {
            gameManager.StandClicked();
        }

        // Reset player balance to 100
        player.AdjustBalance(100.0f - player.GetBalance());
    }

    // Gather information about the current state of the environment
    public override void CollectObservations(VectorSensor sensor) {
        Debug.Log("Collecting Observations");
        
        // The player's current hand
        int handIndex = gameManager.GetHandIndex();
        int playerHandValue = player.handValues[handIndex];
        float playerHandValue_normalized = (playerHandValue - 2.0f) / 19.0f;    // (0 to 1)
        bool isSoftHand = (player.handTypes[handIndex] == "S");

        // The dealer's up card
        int dealerUpCard = dealer.handValues[0];
        float dealerUpCard_normalized = (dealerUpCard - 2.0f) / 9.0f;           // (0 to 1)

        // The shoe's running count
        int totalDecks = shoe.totalCards / 52;
        int maxRunningCount = 20 * totalDecks;
        int runningCount = shoe.runningCount;
        float runningCount_normalized = runningCount / maxRunningCount;         // (-1 to 1)

        // The shoe's true count
        int maxTrueCount = 52;
        int trueCount = shoe.trueCount;
        float trueCount_normalized = trueCount / maxTrueCount;                  // (-1 to 1)

        // The actions available
        List<bool> actionAvailability = GetActionAvailability();

        // Add 11 observations 
        sensor.AddObservation(isSoftHand);
        sensor.AddObservation(playerHandValue_normalized);
        sensor.AddObservation(dealerUpCard_normalized);
        sensor.AddObservation(runningCount_normalized);
        sensor.AddObservation(trueCount_normalized);
        sensor.AddObservation(actionAvailability[0]);   // canStand
        sensor.AddObservation(actionAvailability[1]);   // canHit
        sensor.AddObservation(actionAvailability[2]);   // canDouble
        sensor.AddObservation(actionAvailability[3]);   // canSplit
        sensor.AddObservation(actionAvailability[4]);   // canTakeInsurance
        sensor.AddObservation(actionAvailability[5]);   // canRefuseInsurance
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

        // Add actions to list and return 
        List<bool> actionAvailability = new List<bool>();
        actionAvailability.Add(canStand);
        actionAvailability.Add(canHit);
        actionAvailability.Add(canDouble);
        actionAvailability.Add(canSplit);
        actionAvailability.Add(canTakeInsurance);
        actionAvailability.Add(canRefuseInsurance);
        return actionAvailability;
    }

    // Manually supply OnActionsReceived() input
    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
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

        // The agent attempts to perform given action (stand, hit, etc)
        int action = actions.DiscreteActions[0];
        bool performed = PerformAction(action);

        // Reward shaping with basic strategy
        RewardShape(action);  // eventually remove

        // Penalty given for attempting an illegal action
        if (!performed) {
            RewardAgent(-0.1f);
        }

        // Penalty given each step to encourage agent to finish task quickly
        RewardAgent(-2.0f / MaxStep);   // (-2.0 / 37) ~ -0.05
    }

    // Attempt to perform action (hit, stand, etc) and return if successful
    public bool PerformAction(int action) {
        bool performedAction = false;
        List<bool> actionAvailability = GetActionAvailability();

        if (actionAvailability[action]) {
            performedAction = true;
            switch (action) {
                case 0: // Stand
                    gameManager.StandClicked();
                    break;
                case 1: // Hit               
                    gameManager.HitClicked();
                    break;
                case 2: // Double
                    gameManager.DoubleClicked();
                    break;
                case 3: // Split
                    gameManager.SplitClicked();
                    break;
                case 4: // Take Insurance
                    gameManager.InsuranceClicked(true); 
                    break;
                case 5: // Refuse Insurance
                    gameManager.InsuranceClicked(false);
                    break;
            }
        }

        return performedAction;
    }

    // Called when GameManager reaches RoundOver()
    public void OnRoundOver() {
        // Calculate reward based on the outcome of the round
        float roundResult = 0;
        float.TryParse(gameManager.rewardText.text, out roundResult);
        RewardAgent(roundResult);

        EndEpisode();
    }

    // Reward or penalize agent with given amount
    private void RewardAgent(float amount) {
        AddReward(amount);
        _cumulativeReward = GetCumulativeReward();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();
    }

    // Provide small reward if given action aligns with Basic Strategy
    private void RewardShape(int action) {
        // The player's current hand
        int handIndex = gameManager.GetHandIndex();
        int playerHandValue = player.handValues[handIndex];
        string playerHandType = player.handTypes[handIndex];

        // The dealer's up card
        int dealerUpCard = dealer.handValues[0];

        // Basic strategy action(s)
        List<int> strategies = new List<int>();
        if (playerHandType == "H") {    
            // Hard hands
            strategies = basicStrategy.CheckMDHardTable(playerHandValue, dealerUpCard);
        }
        else if (playerHandType == "S") {
            // Soft hands
            strategies = basicStrategy.CheckMDSoftTable(playerHandValue, dealerUpCard);
        }
        else if (gameManager.splitButton.gameObject.activeSelf) {
            // Pairs
            strategies = basicStrategy.CheckMDPairsTable(playerHandValue, dealerUpCard);

        } else if (gameManager.noInsuranceButton.gameObject.activeSelf) {
            // Insurance
            strategies.Add(5);  // BS never takes insurance
        }

        // Reward if action aligns with basic strategy
        foreach (int strategy in strategies) {
            if (action == strategy) {
                AddReward(.075f);
            }
        }
    }

    private void Update() {
        currentStepText.text = "Step: " + StepCount.ToString();
    }
}