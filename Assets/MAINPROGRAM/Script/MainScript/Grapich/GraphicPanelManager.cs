using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicPanelManager : MonoBehaviour
{
   public static GraphicPanelManager Instance { get; private set; }

    [SerializeField] private GraphicPanel[] allPanels;

    public const float Default_Transition_Speed = 3f;

    private void Awake()
    {
        Instance = this;
    }

    public GraphicPanel GetPanel(string name)
    {
        name = name.ToLower();
        foreach (var panel in allPanels)
        {
            if (panel.panelName.ToLower() == name)
            {
                return panel;
            }
        }
        return null;
    }
}
