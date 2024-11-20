using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    public class DailogParser
    {
        // Adjusted regex pattern to match commands that start with a word character and are followed by an opening parenthesis
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";

        public static DailogLine Parse(string rawLine)
        {
            Debug.Log($"Parsing line = '{rawLine}'");

            (string speaker, string dialogue, string commands) = RipContent(rawLine);

            Debug.Log($"Speaker: '{speaker}'\nDialogue: '{dialogue}'\nCommands: '{commands}'");

            return new DailogLine(speaker, dialogue, commands);
        }

        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscape = false;

            // Loop through the raw line to find dialogue boundaries
            for (int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];
                if (current == '\\')
                {
                    isEscape = !isEscape; // Toggle escape state
                }
                else if (current == '"' && !isEscape) // Check for dialogue start/end
                {
                    if (dialogueStart == -1)
                        dialogueStart = i; // Start of dialogue
                    else if (dialogueEnd == -1)
                        dialogueEnd = i; // End of dialogue
                }
                else
                {
                    isEscape = false; // Reset escape state if not an escape character
                }
            }

            // Use regex to find commands in the raw line
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            int commandStart = -1; // Renamed for clarity
            foreach (Match match in matches)
            {
                if(match.Index < dialogueStart || match.Index > dialogueEnd)
                    {
                    commandStart = match.Index;
                    break;
                    }
            }
            // If no dialogue is found, return the entire line as commands
            if (commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1))
                return ("", "", rawLine.Trim());

            // Extract speaker, dialogue, and commands based on the found indices
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                // Valid dialogue found
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                {
                    commands = rawLine.Substring(commandStart).Trim();
                }
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
            {
                // Commands found before dialogue
                commands = rawLine;
            }
            else
            {
                // If no valid dialogue or commands, treat the whole line as speaker
                dialogue = rawLine;
            }

            return (speaker, dialogue, commands);
        }
    }
}