using UnityEngine;

/// <summary>
/// Class to manage the UI panels
/// </summary>
public class UIManager : Singleton<UIManager>
{
	[SerializeField]
	private Panel[] _panels;

	private Panel.PanelState activePanel = Panel.PanelState.Ingame;

	private void Start()
	{
		Show(Panel.PanelState.MainMenu);
	}

    /// <summary>
    /// Actives the chosen panel and deactivates the previous one.
    /// </summary>
    /// <param name="panel">The new panel to activate.</param>
	public void Show(Panel.PanelState panel)
	{
		if (activePanel != panel)
		{
			GetPanel(activePanel).Disable();
		}

		GetPanel(panel).Enable();
		activePanel = panel;
	}

    /// <summary>
    /// Gets a specific panel.
    /// </summary>
    /// <param name="panel">The panel state.</param>
    /// <returns>The panel</returns>
	public Panel GetPanel(Panel.PanelState panel)
	{
           
		for (var index = 0; index < _panels.Length; index ++)
		{
			if (_panels[index].panel == panel)
			{
				return _panels[index];
			}
		}

		throw new UnityException("Panel not found!");
	}

    /// <summary>
    /// Gets the type of the panel
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    /// <param name="panel"></param>
    /// <returns></returns>
	public Type GetPanel<Type>(Panel.PanelState panel) where Type : Panel
	{
		return (Type) GetPanel(panel);
	}
}
