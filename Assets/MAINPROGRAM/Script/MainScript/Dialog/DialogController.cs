using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Characters;

namespace DIALOGUE
{
    public class DialogController : MonoBehaviour
    {
        [SerializeField]private DialogueControllerConfigSO _config;
        public DialogueControllerConfigSO config => _config;
        public DialogContainer DialogContainer = new DialogContainer(); // Reference to DialogContainer
        private ConversationManager conversationManager;
        private TextArchitech TxtArch;

        public static DialogController Instance { get; private set; } // Singleton instance

        public delegate void DialogueControllerEvent();
        public event DialogueControllerEvent onUserPrompt_Next;

        public bool IsRunningConversation => conversationManager.isRunning; // Property for checking conversation status

        private void Awake() // Singleton initialization
        {
            if (Instance == null)
            {
                Instance = this;
                initialized();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        bool _initialized = false;

        private void initialized()
        {
            if (_initialized)
                return;

            TxtArch = new TextArchitech(DialogContainer.DialogText);
            conversationManager = new ConversationManager(TxtArch);
        }

        public void OnUserPromt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        public void ApplySpeakerDataToDialogContainer(string speakerName)
        {
            Character character = CharacterManager.Instance.GetCharacter(speakerName);
            CharacterConfigData config = character != null ? character.config : CharacterManager.Instance.GetCharacterConfig(speakerName);

            ApplySpeakerDataToDialogContainer(config);
        }

        public void ApplySpeakerDataToDialogContainer(CharacterConfigData config)
        {
            DialogContainer.SetDialogueColor(config.dialogueColor);
            DialogContainer.SetDialogueFont(config.dialogueFont);
            DialogContainer.nameContainer.SetNameColor(config.nameColor);
            DialogContainer.nameContainer.SetNameFont(config.nameFont);
        }

        public void showSpeakerName(string speakerName = "") 
        {
            if(speakerName.ToLower() != "narrator")
            {
                DialogContainer.nameContainer.show(speakerName);
            }
            else
                hideSpeakerName();
        }
        public void hideSpeakerName() => DialogContainer.nameContainer.hide();

        public Coroutine Say(string speaker, string dialogue) // Fixed typo in parameter name
        {
            List<string> conversation = new List<string>() { $"{speaker}, \"{dialogue}\"" }; // Fixed typo in variable name
            return Say(conversation);
        }

        public Coroutine Say(List<string> conversation)
        {
            return conversationManager.startConversation(conversation);
        }
    }
}