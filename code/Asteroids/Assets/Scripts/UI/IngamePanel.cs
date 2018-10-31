using UnityEngine;
using UnityEngine.UI;


public class IngamePanel : Panel
{
    [SerializeField]
    private Text _score;
    [SerializeField]
    private Text _life;
    [SerializeField]
    private Image _energy;

    [SerializeField]
    public GameObject gameOverScreen;
    [SerializeField]
    public Text gameOverScreenScore;
    [SerializeField]
    public InputField gameOverPlayerName;


    protected override void Initialize() { }


    protected override void Reset()
    {
       gameOverScreen.SetActive(false);
    }



    public void ShowGameOverScreen()
    {
        gameOverScreenScore.text = _score.text;
        gameOverScreen.SetActive(true);
    }


    /**
	 * Sets UI score text
	 */
    public void SetScore(int score)
    {
        _score.text = score.ToString("n0");
    }


    /**
	 * Saves the current score and shows the highscore list
	 * Is called by the ok button within the ingame game over UI
	 */
    public void SaveScore()
    {
        //HighscoreManager.Instance.AddScore(gameOverPlayerName.text, GameManager.Instance.player.Score);
        //UIManager.Instance.Show(Panel.PanelState.Highscore);
    }


    /**
	 * Sets the UI life amount
	 */
    public void SetLife(int lives)
    {
        _life.text = lives.ToString("n0");
    }

    /**
    * Sets the energy
    */
    public void SetEnergyPercentage(float energyPercentage)
    {
        _energy.fillAmount = energyPercentage;
    }
}