using UnityEngine;

public class GUI_WingmanAgent : MonoBehaviour {
    // Access Wignman Script
    [SerializeField] private WingmanAgent _wingmanAgent;

    // GUI Styles
    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positiveStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // Define GUI Styles
        _defaultStyle.fontSize = 20;
        _defaultStyle.normal.textColor = Color.black;

        _positiveStyle.fontSize = 20;
        _positiveStyle.normal.textColor = Color.cyan;

        _negativeStyle.fontSize = 20;
        _negativeStyle.normal.textColor = Color.red;
    }

    private void OnGUI() {
        string debugEpisode = "Episode: " + _wingmanAgent.CurrentEpisode + " - Step: " + _wingmanAgent.StepCount;
        string debugReward = "Reward: " + _wingmanAgent.CumulativeReward.ToString();

        float edge = (_wingmanAgent.UnitsWon - _wingmanAgent.UnitsLost) / _wingmanAgent.UnitsWagered;

        string debugEdge = "Edge: " + edge.ToString("F2") + "%";

        // Select style based on values
        GUIStyle rewardStyle = (_wingmanAgent.CumulativeReward < 0) ? _negativeStyle : _positiveStyle;
        GUIStyle edgeStyle = (edge < 0) ? _negativeStyle : _positiveStyle;

        // Display the debug text
        GUI.Label(new Rect(30, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(30, 40, 500, 30), debugReward, rewardStyle);
        GUI.Label(new Rect(30, 60, 500, 30), debugEdge, edgeStyle);
    }
}
