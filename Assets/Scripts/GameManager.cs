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

    public int standClicks = 0;

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
    int betAmount = 0;
    public TMP_Text playerHandValue;
    public TMP_Text dealerHandValue;
    public TMP_Text roundText;

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

        deck.Shuffle();

        //GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
    }

    private void DealClicked() 
    {
        // Reset Round
        player.ResetHand();
        dealer.ResetHand();
        roundText.gameObject.SetActive(false);
        
        // Only re-shuffle when penetration reached


        // GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
        
        // Deal cards
        player.DealHand("player");
        playerHandValue.text = player.handValue.ToString();
        playerHandValue.gameObject.SetActive(true);
        
        dealer.DealHand("dealer");
        blinder.GetComponent<Renderer>().enabled = true;
        dealerHandValue.text = dealer.handValue.ToString();
        dealerHandValue.gameObject.SetActive(true);

        // Adjust Button Visibility
        dealButton.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);

        // Player Money Details
        playerBalance.text = "Balance: " + player.GetBalance().ToString();
        playerBet.text = "Bet: " + betAmount.ToString();
    }

    void MidRound()
    {
        // Check which buttons should be visible
        if 
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
        standClicks = 2;
        playerTurnOver();
        // Check that there is still room on the table
        // will modify later to dynamically adjust cards
        HitDealer();
        RoundOver();
        
    }

    private void SplitClicked()
    {

    }

    private void DoubleClicked()
    {

    }

    private void HitDealer()
    {
        dealer.handValue += dealer.holeCard;
        dealerHandValue.text = dealer.handValue.ToString();
        while (dealer.handValue < 17 && dealer.cardIndex < 10)
        {
            dealer.GetCard();
            dealerHandValue.text = dealer.handValue.ToString();
            if (dealer.handValue > 20) RoundOver();
        }
    }

    void playerTurnOver()
    {
        // Disable action buttons
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        splitButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);

        bool player21 = player.handValue == 21;
        bool playerBust = player.handValue > 21;

        // Check if player got Blackjack

        // Check if player busted

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
            splitButton.gameObject.SetActive(false);
            doubleButton.gameObject.SetActive(false);

            dealButton.gameObject.SetActive(true);
            roundText.gameObject.SetActive(true);
            dealerHandValue.gameObject.SetActive(true);

            blinder.GetComponent<Renderer>().enabled = false;
            playerBalance.text = "Balance: " + player.GetBalance().ToString();
            standClicks = 0;
        }

    }
}