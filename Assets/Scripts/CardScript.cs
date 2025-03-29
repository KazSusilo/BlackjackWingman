using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour {
    // --- This script is specific to the 'card' object

    // Access other script
    public ShoeScript shoeScript;

    // Value of card, 2 of spades = 2, etc
    private int value = 0;


    // Set sprite of card gameObject to 'newSprite'
    public void SetSprite(Sprite newSprite) {
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    // Reset sprite and value from card gameObject
    public void ResetCard() {
        // Sprite backOfCard = GameObject.Find("Deck").GetComponent<DeckScript>().GetBackOfCard();
        Sprite backOfCard = shoeScript.GetBackOfCard();
        gameObject.GetComponent<SpriteRenderer>().sprite = backOfCard;
        gameObject.GetComponent<Renderer>().enabled = false;
        value = 0;
    }

    // Accessor and mutator functions
    public int GetValue() { return value; }
    public void SetValue(int newValue) { value = newValue; }
}
