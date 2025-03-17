using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Action Buttons
    public Button hitButton;
    public Button standButton;
    public Button splitButton;
    public Button doubleButton;

    public int standClicks = 2;

    // Prephase buttons
    public Button dealButton;
    public Button betButton;

    // Access the player and dealer's hand
    public PlayerScript player;
    public PlayerScript dealer;
    public DeckScript deck;

    // Player HUD
    public TMP_Text playerBalance;
    public TMP_Text playerBet; 
    public TMP_Text playerHandValue;
    public TMP_Text dealerHandValue;
    public TMP_Text roundText;
    int betAmount = 0;
    int handIndex = 0;

    // Blinder hiding dealer's card
    public GameObject blinder;

    // Start is called before the first frame update
    void Start()
    {
        // Listener for Buttons
        dealButton.onClick.AddListener(() => DealClicked());
        betButton.onClick.AddListener(() => BetClicked());

        hitButton.onClick.AddListener(() => HitClicked());
        standButton.onClick.AddListener(() => StandClicked());
        splitButton.onClick.AddListener(() => SplitClicked());
        doubleButton.onClick.AddListener(() => DoubleClicked());

        player.playerType = "player";
        dealer.playerType = "dealer";
        deck.Shuffle();
        //GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
    }

    private void DealClicked() {
        // Reset Round
        roundText.gameObject.SetActive(false);
        // Only re-shuffle when penetration reached
        if (deck.currentIndex >= 42)
        {
            deck.Shuffle();
        }
        DisableBetButtons();

        player.ResetHand();
        dealer.ResetHand();
             
        // Deal cards
        player.DealHand(handIndex);
        playerHandValue.text = player.handValue.ToString();
        playerHandValue.gameObject.SetActive(true);
        
        dealer.DealHand(handIndex);
        blinder.GetComponent<Renderer>().enabled = true;
        dealerHandValue.text = dealer.handValue.ToString();
        dealerHandValue.gameObject.SetActive(true);

        ActionPhase();

        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);

        // Player Money Details
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
        playerBet.text = "Bet: " + betAmount.ToString();
    }

    private void ActionPhase() {
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
    }

    private void BetClicked() {
        Text newBet = betButton.GetComponentInChildren(typeof(TMP_Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString());
        player.AdjustBalance(-intBet);
        playerBalance.text = player.GetBalance().ToString();
        betAmount += intBet;
        playerBet.text = betAmount.ToString();
    }

    private void HitClicked() {
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        if (player.cardIndex <= 10) 
        {
            player.GetCard();
            playerHandValue.text = player.handValue.ToString();
            if (player.handValue > 20) {
                playerBust();
            }
        }
    }

    private void StandClicked() { 
        handIndex++;
        if (handIndex == player.hands.Length) {
            playerTurnOver();
        }
    }

    private void SplitClicked() {
        player.SpltHand(handIndex)
    }

    private void DoubleClicked()
    {

    }

    private void PlayDealer()
    {
        blinder.GetComponent<Renderer>().enabled = false;       // Show hole card
        dealer.handValue += dealer.holeCard;                    // Add hole card to handValue
        dealerHandValue.text = dealer.handValue.ToString();     // Update text
        StartCoroutine(HitDealer());                            
    }

    private IEnumerator HitDealer()
    {
        while (dealer.handValue < 17 && dealer.cardIndex < 10) {
            yield return new WaitForSeconds(.5f);
            dealer.GetCard();
            dealerHandValue.text = dealer.handValue.ToString();
        }
        yield return new WaitForSeconds(.5f);
        print("Over");
        RoundOver();
    }



    private void DisableActionButtons() {
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);
    }

    private void DisableBetButtons() {
        betButton.gameObject.SetActive(false);
        // Disable various amounts 
    }

    // Hand is over
    private void RoundOver()
    {
        DisableActionButtons();

        // Booleans for bust and blackjack
        bool playerBust = player.handValue > 21;
        bool dealerBust = dealer.handValue > 21;
        bool player21 = player.handValue == 21;
        bool dealer21 = dealer.handValue == 21;
        bool roundOver = true;

        // if player busts or player hand < dealer hand, player loses
        if (playerBust ||  (!dealerBust && player.handValue < dealer.handValue))
        {
            roundText.text = "You Lose";
            player.AdjustBalance(-betAmount);
        }
        // if dealer busts or player hand > dealer hand, player wins
        else if (dealerBust || (!playerBust && dealer.handValue < player.handValue))
        {
            roundText.text = "You Win";
            player.AdjustBalance(2 * betAmount);
        }
        else if (player21 && !dealer21)
        {
            roundText.text = "Blackjack";
            player.AdjustBalance((int) (2.5 * betAmount));
        }
        else if (player.handValue == dealer.handValue)
        {
            roundText.text = "Push";
        }
        else
        {
            roundOver = false;
        }

        // Set UI for next move / hand / turn
        if (roundOver)
        {
            hitButton.gameObject.SetActive(false);
            standButton.gameObject.SetActive(false);
            splitButton.gameObject.SetActive(false);
            doubleButton.gameObject.SetActive(false);

            dealButton.gameObject.SetActive(true);
            roundText.gameObject.SetActive(true);
            dealerHandValue.gameObject.SetActive(true);

            playerBalance.text = "Balance: " + player.GetBalance().ToString();
            standClicks = 0;
            handIndex = 0;
        }

    }
}