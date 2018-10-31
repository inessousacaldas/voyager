using UnityEngine;


public abstract class Panel : MonoBehaviour
{
    public enum PanelState
    {
        MainMenu,
        Highscore,
        Ingame
    }

    public PanelState panel;

    protected abstract void Initialize();

    protected abstract void Reset();

    public void Enable()
    {
        Initialize();
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        Reset();
    }
}