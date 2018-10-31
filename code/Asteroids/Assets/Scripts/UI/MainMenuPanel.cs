using UnityEngine;
using UnityEngine.UI;
public class MainMenuPanel : Panel
{
    [SerializeField]
    private float delayChangeOption = 0.2f;
    [SerializeField]
    private Text[] optionsMenu;
    [SerializeField]
    private Color selectColor;
    [SerializeField]
    private Color unSelectColor;
    private int selectOption = 0;

    private float timer;

    private void Start()
    {
        //GameManager.Instance.InitializeAsteroids(10);
        timer = Time.time;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) && timer < Time.time)
        {
            changeOption(-1);
            timer = Time.time + delayChangeOption;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && timer < Time.time)
        {
            changeOption(1);
            timer = Time.time + delayChangeOption;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SelectOption();
        }
    }


    public void StartNewGame()
    {
        GameManager.Instance.StartNewGame();
    }


    public void ShowHighscore()
    {
        UIManager.Instance.Show(Panel.PanelState.Highscore);
    }


    protected override void Initialize() { }


    protected override void Reset() { }

    private void changeOption(int dir)
    {
        optionsMenu[selectOption].color = unSelectColor;

        selectOption += dir;

        if (selectOption < 0)
        {
            selectOption = optionsMenu.Length - 1;
        }
        else if (selectOption >= optionsMenu.Length)
        {
            selectOption = 0;
        }

        optionsMenu[selectOption].color = selectColor;

    }

    private void SelectOption()
    {
        switch (selectOption)
        {
            case 0:
                StartNewGame();
                break;

            default:
                break;
        }
    }
}