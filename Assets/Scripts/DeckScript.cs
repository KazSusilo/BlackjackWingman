using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] deck;
    int[] cardValues = new int[53];
    int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        SetCardValues();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Sets a value to each card in the deck
    void SetCardValues()
    {
        int value = 0;
        // Loop to assign values to each card in the deck
        for (int i = 0; i < deck.Length; i++) 
        {
            // Count up to the amount of cards, 52
            // if there is a remainder after x/13, then remainder
            // is used as the value, unless over 10, then use 10
            value = i;
            value %= 13;
            if (value > 10 || value == 0)
            {
                value = 10;
            }
            cardValues[i] = value;
            value++;
        }
        currentIndex = 1;
    }

    // Shuffles deck
    public void Shuffle()
    {
        // Standard array data swapping technique
        for (int i = deck.Length - 1; i > 0; --i)
        {
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * deck.Length - 1) + 1;
            
            // Swap sprite of cards
            Sprite tempCard = deck[i];
            deck[i] = deck[j];
            deck[j] = tempCard;

            // Swap value of cards
            int tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;
        }
    }

    public int DealCard(CardScript cardScript)
    {
        cardScript.SetSprite(deck[currentIndex]);
        cardScript.SetValue(cardValues[currentIndex]);
        currentIndex++;
        return cardScript.GetValue();
    }

    public Sprite GetCardBack()
    {
        return deck[0];
    }
}
