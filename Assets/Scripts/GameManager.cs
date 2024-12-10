using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button dealButton;
    public Button hitButton;
    public Button standButton;
    public Button betButton;

    // Access the player and dealer's hand
    public PlayerScript playerScript;
    public PlayerScript dealer;

    // Start is called before the first frame update
    void Start()
    {
        dealButton.onClick.AddListener(() => DealClicked());
    }

    private void DealClicked() 
    {
        playerScript.StartHand();
    }
}