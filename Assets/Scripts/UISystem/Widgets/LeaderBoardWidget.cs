using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardWidget : MonoBehaviour
{
    public Text textPosition;
    public Text textPlayer;
    public void Initialize(string text, int score, int position)
    {
        textPosition.text = position.ToString();
        this.textPlayer.text = $"{text}: {score}";
    }
}
