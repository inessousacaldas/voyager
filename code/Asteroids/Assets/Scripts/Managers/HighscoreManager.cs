using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class to manage the highscore list
/// </summary>
public class HighscoreManager : Singleton<HighscoreManager>
{
   /// <summary>
   /// A score entry with the name of the player and his score
   /// </summary>
    public struct Score
    {
       
        public int score;
        public string name;

        public Score(string name_, int score_)
        {
            name = name_;
            score = score_;
        }
    }

    [SerializeField]
    private int _highscoreListLength = 10;
	private List<Score> _highscoreList = new List<Score>();

    /// <summary>
    /// The highscore list
    /// </summary>
    public List<Score> HighscoreList
    {
        get
        {
            return _highscoreList;
        }

        set
        {
            _highscoreList = value;
        }
    }

    private void Start()
	{
        LoadHighscoreList();

    }

    /// <summary>
    /// Loads the highscore list from the player prefs
    /// </summary>
    private void LoadHighscoreList()
    {
        for (var rank = 1; rank <= _highscoreListLength; rank++)
        {
            if (PlayerPrefs.HasKey("Rank" + rank + "Name"))
            {
                var name = PlayerPrefs.GetString("Rank" + rank + "Name");
                var score = PlayerPrefs.GetInt("Rank" + rank + "Score");
                var scoreStruct = new Score(name, score);

                HighscoreList.Add(scoreStruct);
            }
        }
    }

    /// <summary>
    /// Add a new entry to the highscore list if the score is high enough
    /// </summary>
    /// <param name="name">Player's name</param>
    /// <param name="score">Player's score</param>
	public void AddScore(string name, int score)
	{
		if (NewHighscoreEntry(score))
		{
			var scoreStruct = new Score(name, score);
			HighscoreList.Add(scoreStruct);
			HighscoreList = HighscoreList.OrderByDescending(orderScoreStruct => orderScoreStruct.score).ToList();

            // Removes the the last entry if the highscore list is bigger than _highscoreListLength
            if (HighscoreList.Count > _highscoreListLength)
			{
				HighscoreList.RemoveAt(_highscoreListLength);
			}

			Save();
		}
	}


	/// <summary>
    /// Saves to the disk the highscore list
    /// </summary>
	private void Save()
	{
		for (var rank = 1; rank <= HighscoreList.Count; rank ++)
		{
			PlayerPrefs.SetString("Rank" + rank + "Name", HighscoreList[rank - 1].name);
			PlayerPrefs.SetInt("Rank" + rank + "Score", HighscoreList[rank - 1].score);
		}
	}


	/// <summary>
    /// Checks if the score is enough to enter the highscore list
    /// </summary>
    /// <param name="score"></param>
    /// <returns>True if the score can enter the highscore list. False otherwise</returns>
	public bool NewHighscoreEntry(int score)
	{
		return (HighscoreList.Count < _highscoreListLength || score > HighscoreList.Last().score);
	}
}
