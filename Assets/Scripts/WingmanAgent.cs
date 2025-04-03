using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WingmanAgent : Agent {
    // Access other scripts
    public PlayerScript player;
    public PlayerScript dealer;
    public GameManager gameManager;
    public ShoeScript shoe;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    private float _beginningBalance;
    private bool _isRoundOver = false;

    public TMP_Text currentEpisodeText;
    public TMP_Text cumulativeRewardText;


    // Called when the Agent is first created 
    public override void Initialize() {
        _beginningBalance = player.GetBalance();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
        _isRoundOver = false;

        currentEpisodeText.text = "Episode: " + _currentEpisode.ToString();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();
    }

    // Reset the environment at the start of each episode
    public override void OnEpisodeBegin() {
        _currentEpisode++;
        _cumulativeReward = 0f;
        _beginningBalance = player.GetBalance();
        _isRoundOver = false;
        
        currentEpisodeText.text = "Episode: " + _currentEpisode.ToString();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();

        // Start a new round
        gameManager.DealClicked();
    }

    // Gather information about the current state of the environment
    public override void CollectObservations(VectorSensor sensor) {
        // potentially only get the local values to set up multiple tables for training at once
        // Edge case - don't collect ovservations if the round is over
        int handIndex = gameManager.GetHandIndex(); // specific hand
        if (handIndex >= player.handValues.Count) {
            return; 
        }

        // Player hand's
        int playerHandValue = player.handValues[handIndex];
        float playerHandValueNormalized = (playerHandValue - 2) / 19.0f;   // normalize for NN (0 to 1)
        bool isSoftHand = (player.handTypes[handIndex] == "S");
        sensor.AddObservation(playerHandValueNormalized);
        sensor.AddObservation(isSoftHand);

        // Dealer up card
        int dealerUpCard = dealer.handValues[0];
        float dealerUpCardNormalized = (dealerUpCard - 2) / 9.0f;      // normalize for NN (0 to 1)
        sensor.AddObservation(dealerUpCardNormalized);

        // Counts of the shoe
        int totalDecks = shoe.totalCards / 52;
        int maxRunningCount = 20 * totalDecks;                         // +- 20 highest RC with 1D
        int maxTrueCount = 52;                                         // +-52 highest TC  

        int runningCount = shoe.runningCount;
        int trueCount = shoe.trueCount;
        float runningCountNormalized = runningCount / maxRunningCount; // normalize for NN (-1 to 1)
        float trueCountNormalized = trueCount / maxTrueCount;          // normalize for NN (-1 to 1)
        sensor.AddObservation(runningCountNormalized);
        sensor.AddObservation(trueCountNormalized);

        // Actions available
        List<bool> actionAvailability = GetActionAvailability();
        sensor.AddObservation(actionAvailability[0]);   // canStand
        sensor.AddObservation(actionAvailability[1]);   // canHit
        sensor.AddObservation(actionAvailability[2]);   // canDouble
        sensor.AddObservation(actionAvailability[3]);   // canSplit
        sensor.AddObservation(actionAvailability[4]);   // canTakeInsurance
        sensor.AddObservation(actionAvailability[5]);   // canRefuseInsurance
    }

    // Perform an action based on the output of the NN
    public override void OnActionReceived(ActionBuffers actions) {
        // Edge case - don't collect ovservations if the round is over
        int handIndex = gameManager.GetHandIndex(); // specific hand
        if (handIndex >= player.handValues.Count) {
            return; 
        }

        List<bool> actionAvailability = GetActionAvailability();
        int playerHandValue = player.handValues[handIndex];
        bool illegalAction = true;
        var action = actions.DiscreteActions[0];
        print("Agent Action: " + action);
        switch (action) {
            case 0: // Stand
                if (actionAvailability[0]) {
                    gameManager.StandClicked();
                    illegalAction = false;
                } 
                break;
            case 1: // Hit 
                if (actionAvailability[1]) {
                    gameManager.HitClicked();
                    illegalAction = false;
                }
                break;
            case 2: // Double
                if (actionAvailability[2]) {
                    gameManager.DoubleClicked();
                    illegalAction = false;
                }
                break;
            case 3: // Split
                if (actionAvailability[3]) {
                    gameManager.SplitClicked();
                    illegalAction = false;
                }
                break;
            case 4: // Take Insurance
                if (actionAvailability[4]) {
                    gameManager.InsuranceClicked(true);
                    illegalAction = false;
                }
                break;
            case 5: // Refuse Insurance
                if (actionAvailability[5]) {
                    gameManager.InsuranceClicked(false);
                    illegalAction = false;
                }
                break;
        }

        // Penalize agent if performing an illegal action
        if (illegalAction) {
            RewardAgent(-1f);
        }
    }

    // Continuously check if the round is over
    private void Update() {
        if (gameManager.rewardText.gameObject.activeSelf) {
            RoundFinished();
        }
    }

    // Reward agent based on reward from round
    private void RewardAgent(float reward) {
        AddReward(reward);
        _cumulativeReward = GetCumulativeReward();
        cumulativeRewardText.text = "Reward: " + _cumulativeReward.ToString();
    }

    // Handle large reward
    private void RoundFinished() {
        _isRoundOver = true;
        float balanceDelta = player.GetBalance() - _beginningBalance;
        float balanceDeltaAdjusted= balanceDelta / 2.0f;    // 2$ initial bet
        RewardAgent(balanceDeltaAdjusted);
        EndEpisode();
    }

    // Returns a list of bools indicating which actions are available
    private List<bool> GetActionAvailability() {
        bool canStand = gameManager.standButton.gameObject.activeSelf;
        bool canHit = gameManager.hitButton.gameObject.activeSelf;
        bool canDouble = gameManager.doubleButton.gameObject.activeSelf;
        bool canSplit = gameManager.splitButton.gameObject.activeSelf;
        bool canTakeInsurance = gameManager.yesInsuranceButton.gameObject.activeSelf;
        bool canRefuseInsurance = gameManager.noInsuranceButton.gameObject.activeSelf;

        List<bool> actionAvailability = new List<bool> { 
                                                        canStand, canHit, canDouble, canSplit,
                                                        canTakeInsurance, canRefuseInsurance
                                                        };
        return actionAvailability;
    }
}
