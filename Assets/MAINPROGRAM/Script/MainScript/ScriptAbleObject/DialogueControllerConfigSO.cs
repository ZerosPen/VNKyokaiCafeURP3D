using Characters;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialog Container Configuration", menuName = "Dialogue Contoller/Dialogue Configurantion Asset")]

    public class DialogueControllerConfigSO : ScriptableObject
    {
        public CharacterConfigSO characterConfigationAsset;

        public Color defaultTextColour =  Color.white; 
        public TMP_FontAsset defaulFont;
    }
}