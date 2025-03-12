using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button dealButton;
    public Button hitButton;
    public Button standButton;
    public Button betButton;

    private int standClicks = 0;

    // Access the player and dealer's hand
    public PlayerScript player;
    public PlayerScript dealer;

    //public Text to access and update - hud
    public TMP_Text playerBalance;
    public TMP_Text playerBet; 
    int betAmount = 0;
    public TMP_Text playerHandValue;
    public TMP_Text dealerHandValue;
    public TMP_Text roundText;

    // Blinder hiding dealer's card
    public GameObject blinder;

    // Start is called before the first frame update
    void Start()
    {
        dealButton.onClick.AddListener(() => DealClicked());
        hitButton.onClick.AddListener(() => HitClicked());
        standButton.onClick.AddListener(() => StandClicked());
        betButton.onClick.AddListener(() => BetClicked());
    }

    private void DealClicked() 
    {
        // Reset Round
        player.ResetHand();
        dealer.ResetHand();

        // Hide dealer's hand value
        roundText.gameObject.SetActive(false);
        dealerHandValue.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
        player.DealHand();
        dealer.DealHand();

        // Update the hand values displayed
        playerHandValue.text = player.handValue.ToString();
        playerHandValue.gameObject.SetActive(true);
        dealerHandValue.text = dealer.handValue.ToString();
        dealerHandValue.gameObject.SetActive(true);

        // Enable to hid one of the dealer's cards
        blinder.GetComponent<Renderer>().enabled = true;

        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);

        // Player Money Details
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
        playerBet.text = "Bet: " + betAmount.ToString();

    }

    private void HitClicked() 
    {
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        if (player.cardIndex <= 10) 
        {
            player.GetCard();
            playerHandValue.text = player.handValue.ToString();
            if (player.handValue > 20) RoundOver();
        }
    }

    private void StandClicked() 
    {
        standClicks++;
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        HitDealer();
        RoundOver();
        
    }

    private void BetClicked()
    {
        Text newBet = betButton.GetComponentInChildren(typeof(TMP_Text)) as Text;
        int intBet = int.Parse(newBet.text.ToString());
        player.AdjustBalance(-intBet);
        playerBalance.text = player.GetBalance().ToString();
        betAmount += intBet;
        playerBet.text = betAmount.ToString();
    }

    private void HitDealer()
    {
        while (dealer.handValue < 16 && dealer.cardIndex < 10)
        {
            dealer.GetCard();
            dealerHandValue.text = dealer.handValue.ToString();
            if (dealer.handValue > 20) RoundOver();
        }
    }

    // Hand is over
    void RoundOver()
    {
        // Booleans for bust and blackjack
        bool playerBust = player.handValue > 21;
        bool dealerBust = dealer.handValue > 21;
        bool player21 = player.handValue == 21;
        bool dealer21 = dealer.handValue == 21;

        // If stand has been clicked less than twice, no 21s or busts, quit function
        if (standClicks < 2 && !playerBust && !dealerBust && !player21 && !dealer21) return;
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
            dealButton.gameObject.SetActive(true);
            roundText.gameObject.SetActive(true);
            dealerHandValue.gameObject.SetActive(true);

            blinder.GetComponent<Renderer>().enabled = false;
            playerBalance.text = "Balance: " + player.GetBalance().ToString();
            standClicks = 0;
        }

    }
}