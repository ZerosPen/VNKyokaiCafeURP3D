using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
 * 
 */
namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject MainGame;
        [SerializeField] private TextMeshProUGUI nameText;
        public void show(string nameToShow = "")
        {
            MainGame.SetActive(true);
            if (nameToShow != string.Empty)
            {
                nameText.text = nameToShow;
            }
        }

        public void hide()
        {
            MainGame.SetActive(false);
        }

        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
    }
}
