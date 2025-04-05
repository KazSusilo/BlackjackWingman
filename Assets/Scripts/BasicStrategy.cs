using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicStrategy : MonoBehaviour {
    // Multi-Deck (4+) Hard Hands Table
    static Dictionary<(int, int), string[]> MDHardHandsTable = new Dictionary<(int, int), string[]> 
    { 
        // H8- always hit

        {(9 , 2 ), new string[] {"H"     }},
        {(9 , 3 ), new string[] {"D", "H"}},
        {(9 , 4 ), new string[] {"D", "H"}},
        {(9 , 5 ), new string[] {"D", "H"}},
        {(9 , 6 ), new string[] {"D", "H"}},
        {(9 , 7 ), new string[] {"H"     }},
        {(9 , 8 ), new string[] {"H"     }},
        {(9 , 9 ), new string[] {"H"     }},
        {(9 , 10), new string[] {"H"     }},
        {(9 , 11), new string[] {"H"     }},

        {(10, 2 ), new string[] {"D", "H"}},
        {(10, 3 ), new string[] {"D", "H"}},
        {(10, 4 ), new string[] {"D", "H"}},
        {(10, 5 ), new string[] {"D", "H"}},
        {(10, 6 ), new string[] {"D", "H"}},
        {(10, 7 ), new string[] {"D", "H"}},
        {(10, 8 ), new string[] {"D", "H"}},
        {(10, 9 ), new string[] {"D", "H"}},
        {(10, 10), new string[] {"H"     }},
        {(10, 11), new string[] {"H"     }},

        {(11, 2 ), new string[] {"D", "H"}},
        {(11, 3 ), new string[] {"D", "H"}},
        {(11, 4 ), new string[] {"D", "H"}},
        {(11, 5 ), new string[] {"D", "H"}},
        {(11, 6 ), new string[] {"D", "H"}},
        {(11, 7 ), new string[] {"D", "H"}},
        {(11, 8 ), new string[] {"D", "H"}},
        {(11, 9 ), new string[] {"D", "H"}},
        {(11, 10), new string[] {"D", "H"}},
        {(11, 11), new string[] {"D", "H"}},

        {(12, 2 ), new string[] {"H"     }},
        {(12, 3 ), new string[] {"H"     }},
        {(12, 4 ), new string[] {"S"     }},
        {(12, 5 ), new string[] {"S"     }},
        {(12, 6 ), new string[] {"S"     }},
        {(12, 7 ), new string[] {"H"     }},
        {(12, 8 ), new string[] {"H"     }},
        {(12, 9 ), new string[] {"H"     }},
        {(12, 10), new string[] {"H"     }},
        {(12, 11), new string[] {"H"     }},

        {(13, 2 ), new string[] {"S"     }},
        {(13, 3 ), new string[] {"S"     }},
        {(13, 4 ), new string[] {"S"     }},
        {(13, 5 ), new string[] {"S"     }},
        {(13, 6 ), new string[] {"S"     }},
        {(13, 7 ), new string[] {"H"     }},
        {(13, 8 ), new string[] {"H"     }},
        {(13, 9 ), new string[] {"H"     }},
        {(13, 10), new string[] {"H"     }},
        {(13, 11), new string[] {"H"     }},

        {(14, 2 ), new string[] {"S"     }},
        {(14, 3 ), new string[] {"S"     }},
        {(14, 4 ), new string[] {"S"     }},
        {(14, 5 ), new string[] {"S"     }},
        {(14, 6 ), new string[] {"S"     }},
        {(14, 7 ), new string[] {"H"     }},
        {(14, 8 ), new string[] {"H"     }},
        {(14, 9 ), new string[] {"H"     }},
        {(14, 10), new string[] {"H"     }},
        {(14, 11), new string[] {"H"     }},

        {(15, 2 ), new string[] {"S"     }},
        {(15, 3 ), new string[] {"S"     }},
        {(15, 4 ), new string[] {"S"     }},
        {(15, 5 ), new string[] {"S"     }},
        {(15, 6 ), new string[] {"S"     }},
        {(15, 7 ), new string[] {"H"     }},
        {(15, 8 ), new string[] {"H"     }},
        {(15, 9 ), new string[] {"H"     }},
        {(15, 10), new string[] {"H", "R"}},
        {(15, 11), new string[] {"H", "R"}},

        {(16, 2 ), new string[] {"S"     }},
        {(16, 3 ), new string[] {"S"     }},
        {(16, 4 ), new string[] {"S"     }},
        {(16, 5 ), new string[] {"S"     }},
        {(16, 6 ), new string[] {"S"     }},
        {(16, 7 ), new string[] {"H"     }},
        {(16, 8 ), new string[] {"H"     }},
        {(16, 9 ), new string[] {"H", "R"}},
        {(16, 10), new string[] {"H", "R"}},
        {(16, 11), new string[] {"H", "R"}},

        {(17, 2 ), new string[] {"S"     }},
        {(17, 3 ), new string[] {"S"     }},
        {(17, 4 ), new string[] {"S"     }},
        {(17, 5 ), new string[] {"S"     }},
        {(17, 6 ), new string[] {"S"     }},
        {(17, 7 ), new string[] {"S"     }},
        {(17, 8 ), new string[] {"S"     }},
        {(17, 9 ), new string[] {"S"     }},
        {(17, 10), new string[] {"S"     }},
        {(17, 11), new string[] {"S", "R"}},

        //H18+ always stand
    };

    // Multi-Deck (4+) Soft Hands Table
    static Dictionary<(int, int), string[]> MDSoftHandsTable = new Dictionary<(int, int), string[]> 
    {
        {(13, 2 ), new string[] {"H"     }},
        {(13, 3 ), new string[] {"H"     }},
        {(13, 4 ), new string[] {"H"     }},
        {(13, 5 ), new string[] {"D", "H"}},
        {(13, 6 ), new string[] {"D", "H"}},
        {(13, 7 ), new string[] {"H"     }},
        {(13, 8 ), new string[] {"H"     }},
        {(13, 9 ), new string[] {"H"     }},
        {(13, 10), new string[] {"H"     }},
        {(13, 11), new string[] {"H"     }},

        {(14, 2 ), new string[] {"H"     }},
        {(14, 3 ), new string[] {"H"     }},
        {(14, 4 ), new string[] {"H"     }},
        {(14, 5 ), new string[] {"D", "H"}},
        {(14, 6 ), new string[] {"D", "H"}},
        {(14, 7 ), new string[] {"H"     }},
        {(14, 8 ), new string[] {"H"     }},
        {(14, 9 ), new string[] {"H"     }},
        {(14, 10), new string[] {"H"     }},
        {(14, 11), new string[] {"H"     }},

        {(15, 2 ), new string[] {"H"     }},
        {(15, 3 ), new string[] {"H"     }},
        {(15, 4 ), new string[] {"D", "H"}},
        {(15, 5 ), new string[] {"D", "H"}},
        {(15, 6 ), new string[] {"D", "H"}},
        {(15, 7 ), new string[] {"H"     }},
        {(15, 8 ), new string[] {"H"     }},
        {(15, 9 ), new string[] {"H"     }},
        {(15, 10), new string[] {"H"     }},
        {(15, 11), new string[] {"H"     }},

        {(16, 2 ), new string[] {"H"     }},
        {(16, 3 ), new string[] {"H"     }},
        {(16, 4 ), new string[] {"D", "H"}},
        {(16, 5 ), new string[] {"D", "H"}},
        {(16, 6 ), new string[] {"D", "H"}},
        {(16, 7 ), new string[] {"H"     }},
        {(16, 8 ), new string[] {"H"     }},
        {(16, 9 ), new string[] {"H"     }},
        {(16, 10), new string[] {"H"     }},
        {(16, 11), new string[] {"H"     }},

        {(17, 2 ), new string[] {"H"     }},
        {(17, 3 ), new string[] {"D", "H"}},
        {(17, 4 ), new string[] {"D", "H"}},
        {(17, 5 ), new string[] {"D", "H"}},
        {(17, 6 ), new string[] {"D", "H"}},
        {(17, 7 ), new string[] {"H"     }},
        {(17, 8 ), new string[] {"H"     }},
        {(17, 9 ), new string[] {"H"     }},
        {(17, 10), new string[] {"H"     }},
        {(17, 11), new string[] {"H"     }},

        {(18, 2 ), new string[] {"D", "S"}},
        {(18, 3 ), new string[] {"D", "S"}},
        {(18, 4 ), new string[] {"D", "S"}},
        {(18, 5 ), new string[] {"D", "S"}},
        {(18, 6 ), new string[] {"D", "S"}},
        {(18, 7 ), new string[] {"S"     }},
        {(18, 8 ), new string[] {"S"     }},
        {(18, 9 ), new string[] {"H"     }},
        {(18, 10), new string[] {"H"     }},
        {(18, 11), new string[] {"H"     }},
        
        {(19, 2 ), new string[] {"S"     }},
        {(19, 3 ), new string[] {"S"     }},
        {(19, 4 ), new string[] {"S"     }},
        {(19, 5 ), new string[] {"S"     }},
        {(19, 6 ), new string[] {"D", "S"}},
        {(19, 7 ), new string[] {"S"     }},
        {(19, 8 ), new string[] {"S"     }},
        {(19, 9 ), new string[] {"S"     }},
        {(19, 10), new string[] {"S"     }},
        {(19, 11), new string[] {"S"     }},

        {(20, 2 ), new string[] {"S"     }},
        {(20, 3 ), new string[] {"S"     }},
        {(20, 4 ), new string[] {"S"     }},
        {(20, 5 ), new string[] {"S"     }},
        {(20, 6 ), new string[] {"S"     }},
        {(20, 7 ), new string[] {"S"     }},
        {(20, 8 ), new string[] {"S"     }},
        {(20, 9 ), new string[] {"S"     }},
        {(20, 10), new string[] {"S"     }},
        {(20, 11), new string[] {"S"     }},
    };

    // Multi-Deck (4+) Pairs Table
    static Dictionary<(int, int), string[]> MDPairsTable = new Dictionary<(int, int), string[]> 
    {
        {(4, 2 ), new string[] {"P", "H"}},
        {(4, 3 ), new string[] {"P", "H"}},
        {(4, 4 ), new string[] {"P"     }},
        {(4, 5 ), new string[] {"P"     }},
        {(4, 6 ), new string[] {"P"     }},
        {(4, 7 ), new string[] {"P"     }},
        {(4, 8 ), new string[] {"H"     }},
        {(4, 9 ), new string[] {"H"     }},
        {(4, 10), new string[] {"H"     }},
        {(4, 11), new string[] {"H"     }},

        {(6, 2 ), new string[] {"P", "H"}},
        {(6, 3 ), new string[] {"P", "H"}},
        {(6, 4 ), new string[] {"P"     }},
        {(6, 5 ), new string[] {"P"     }},
        {(6, 6 ), new string[] {"P"     }},
        {(6, 7 ), new string[] {"P"     }},
        {(6, 8 ), new string[] {"H"     }},
        {(6, 9 ), new string[] {"H"     }},
        {(6, 10), new string[] {"H"     }},
        {(6, 11), new string[] {"H"     }},

        {(8, 2 ), new string[] {"H"     }},
        {(8, 3 ), new string[] {"H"     }},
        {(8, 4 ), new string[] {"H"     }},
        {(8, 5 ), new string[] {"P", "H"}},
        {(8, 6 ), new string[] {"P", "H"}},
        {(8, 7 ), new string[] {"H"     }},
        {(8, 8 ), new string[] {"H"     }},
        {(8, 9 ), new string[] {"H"     }},
        {(8, 10), new string[] {"H"     }},
        {(8, 11), new string[] {"H"     }},

        {(10, 2 ), new string[] {"D", "H"}},
        {(10, 3 ), new string[] {"D", "H"}},
        {(10, 4 ), new string[] {"D", "H"}},
        {(10, 5 ), new string[] {"D", "H"}},
        {(10, 6 ), new string[] {"D", "H"}},
        {(10, 7 ), new string[] {"D", "H"}},
        {(10, 8 ), new string[] {"D", "H"}},
        {(10, 9 ), new string[] {"D", "H"}},
        {(10, 10), new string[] {"H"     }},
        {(10, 11), new string[] {"H"     }},

        {(12, 2 ), new string[] {"P", "H"}},
        {(12, 3 ), new string[] {"P"     }},
        {(12, 4 ), new string[] {"P"     }},
        {(12, 5 ), new string[] {"P"     }},
        {(12, 6 ), new string[] {"P"     }},
        {(12, 7 ), new string[] {"H"     }},
        {(12, 8 ), new string[] {"H"     }},
        {(12, 9 ), new string[] {"H"     }},
        {(12, 10), new string[] {"H"     }},
        {(12, 11), new string[] {"H"     }},

        {(14, 2 ), new string[] {"P"     }},
        {(14, 3 ), new string[] {"P"     }},
        {(14, 4 ), new string[] {"P"     }},
        {(14, 5 ), new string[] {"P"     }},
        {(14, 6 ), new string[] {"P"     }},
        {(14, 7 ), new string[] {"P"     }},
        {(14, 8 ), new string[] {"H"     }},
        {(14, 9 ), new string[] {"H"     }},
        {(14, 10), new string[] {"H"     }},
        {(14, 11), new string[] {"H"     }},

        {(16, 2 ), new string[] {"P"     }},
        {(16, 3 ), new string[] {"P"     }},
        {(16, 4 ), new string[] {"P"     }},
        {(16, 5 ), new string[] {"P"     }},
        {(16, 6 ), new string[] {"P"     }},
        {(16, 7 ), new string[] {"P"     }},
        {(16, 8 ), new string[] {"P"     }},
        {(16, 9 ), new string[] {"P"     }},
        {(16, 10), new string[] {"P"     }},
        {(16, 11), new string[] {"P", "R"}},

        {(18, 2 ), new string[] {"P"     }},
        {(18, 3 ), new string[] {"P"     }},
        {(18, 4 ), new string[] {"P"     }},
        {(18, 5 ), new string[] {"P"     }},
        {(18, 6 ), new string[] {"P"     }},
        {(18, 7 ), new string[] {"S"     }},
        {(18, 8 ), new string[] {"P"     }},
        {(18, 9 ), new string[] {"P"     }},
        {(18, 10), new string[] {"S"     }},
        {(18, 11), new string[] {"S"     }},

        {(20, 2 ), new string[] {"S"     }},
        {(20, 3 ), new string[] {"S"     }},
        {(20, 4 ), new string[] {"S"     }},
        {(20, 5 ), new string[] {"S"     }},
        {(20, 6 ), new string[] {"S"     }},
        {(20, 7 ), new string[] {"S"     }},
        {(20, 8 ), new string[] {"S"     }},
        {(20, 9 ), new string[] {"S"     }},
        {(20, 10), new string[] {"S"     }},
        {(20, 11), new string[] {"S"     }},

        {(2, 2 ), new string[] {"P"     }},
        {(2, 3 ), new string[] {"P"     }},
        {(2, 4 ), new string[] {"P"     }},
        {(2, 5 ), new string[] {"P"     }},
        {(2, 6 ), new string[] {"P"     }},
        {(2, 7 ), new string[] {"P"     }},
        {(2, 8 ), new string[] {"P"     }},
        {(2, 9 ), new string[] {"P"     }},
        {(2, 10), new string[] {"P"     }},
        {(2, 11), new string[] {"P"     }},
    };

    // Convert Table Action(string) to Table Action(int)
    private int ConvertAction(string action) {
        int result = -1;
        switch (action) {
            case "S":   // stand
                result = 0;
                break;
            case "H":   // hit
                result = 1;
                break;
            case "D":   // double
                result = 2;
                break;
            case "P":   // split
                result = 3;
                break;
            case "R":   // surrender
                result = 6;
                break;
        }
        return result;
    }

    // Return a list of actions based on Hard Hands Table
    public List<int> CheckMDHardTable(int playerHandValue, int dealerUpCard) {
        // Edge cases
        if (playerHandValue <= 8) { // always hit
            return CheckTableHelper(new string[] {"H"});
        }
        else if (18 <= playerHandValue) { // always stand
            return CheckTableHelper(new string[] {"S"});
        }

        // Game state in table
        string[] actions = MDHardHandsTable[(playerHandValue, dealerUpCard)];
        return CheckTableHelper(actions);
    }

    // Return a list of actions based on Soft Hands Table
    public List<int> CheckMDSoftTable(int playerHandValue, int dealerUpCard) {
        string[] actions = MDSoftHandsTable[(playerHandValue, dealerUpCard)];
        return CheckTableHelper(actions);
    }

    // Return a list of actions based on Pairs Table
    public List<int> CheckMDPairsTable(int playerHandValue, int dealerUpCard) {
        string[] actions = MDPairsTable[(playerHandValue, dealerUpCard)];
        return CheckTableHelper(actions);
    }

    // Helper function for all Check_Table functions
    private List<int> CheckTableHelper(string[] actions) {
        List<int> convertedActions = new List<int>();
        foreach (string action in actions) {
            convertedActions.Add(ConvertAction(action));
        }
        return convertedActions;
    }
}
