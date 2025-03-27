using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour {
    // --- This script is specific to the 'card' object

    // Access other script
    public DeckScript deckScript;
    public ShoeScript shoeScript;

    // Value of card, 2 of spades = 2, etc
    private int value = 0;


    // cardValue acessor functions
    public int GetValue() { return value; }
    public void SetValue(int newValue) { value = newValue; }

    // Set sprite of card gameObject to 'newSprite'
    public void SetSprite(Sprite newSprite) {
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    // Reset sprite and value from card gameObject
    public void ResetCard() {
        // Sprite backOfCard = GameObject.Find("Deck").GetComponent<DeckScript>().GetBackOfCard();
        Sprite backOfCard = deckScript.GetBackOfCard();
        // Sprite backOfCard = shoeScript.GetBackOfCard();
        gameObject.GetComponent<SpriteRenderer>().sprite = backOfCard;
        value = 0;
    }
}
