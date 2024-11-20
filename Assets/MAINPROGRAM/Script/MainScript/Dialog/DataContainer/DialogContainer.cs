using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DIALOGUE;

[System.Serializable]// able to see all privet variable
public class DialogContainer
{
    public GameObject root;
    public NameContainer nameContainer;
    public TextMeshProUGUI DialogText;

    public void SetDialogueColor(Color color) => DialogText.color = color;
    public void SetDialogueFont(TMP_FontAsset font) => DialogText.font = font;
}
