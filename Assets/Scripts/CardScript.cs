using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    // Value of card, 2 of spades = 2, etc
    private int value = 0;

    // Get the value of card
    public int GetValue() {
        return value;
    }

    // Set value of card 
    public void SetValue(int newValue) {
        value = newValue;
    }

    // Set sprite of card gameObject to 'newSprite'
    public void SetSprite(Sprite newSprite) {
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    // Reset sprite and value from card gameObject
    public void ResetCard() {
        Sprite backOfCard = GameObject.Find("Deck").GetComponent<DeckScript>().GetBackOfCard();
        gameObject.GetComponent<SpriteRenderer>().sprite = backOfCard;
        value = 0;
    }
}
