using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TESTING
{
    public class TestConversation : MonoBehaviour
    {
        [SerializeField] private TextAsset FileToRead = null;

        // Start is called before the first frame update
        void Start()
        {
            StartConversation();
        }

        void StartConversation()
        {
            // Check if FileToRead is assigned before reading
            if (FileToRead == null)
            {
                Debug.LogError("FileToRead is not assigned.");
                return;
            }

            // Read lines from the TextAsset
            List<string> lines = FileManager.readTxtAsset(FileToRead.name); // Ensure the path is correct
            if (lines == null)
            {
                Debug.LogError("Failed to read lines from the file.");
                return;
            }

            DialogController.Instance.Say(lines);
        }
    }
}