﻿using UnityEngine;
using UnityEngine.UI;

/**
 * Handles a row within the highscore list
 */
public class HighscorePanelRow : MonoBehaviour
{
    /**
	 * The rank
	 */
    public Text rank;


    /**
	 * The players name
	 */
    public Text playerName;


    /**
	 * The score
	 */
    public Text score;


    /**
	 * Sets the row display data
	 */
    public void SetRowData(int rank, HighscoreManager.Score scoreStruct)
    {
        this.rank.text = rank + ".";
        this.playerName.text = scoreStruct.name;
        this.score.text = scoreStruct.score.ToString("n0");
    }
}