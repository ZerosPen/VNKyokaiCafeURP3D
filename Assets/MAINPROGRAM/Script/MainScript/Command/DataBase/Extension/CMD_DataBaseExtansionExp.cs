using Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class CMD_DataBaseExtansionExp : cmd_DataBaseExtension
    {
        new public static void Extend(CommandDataBase dataBase)
        {
            // Add command with no parameters
            dataBase.addCommand("print", new Action(PrintDefaultMsg));
            dataBase.addCommand("print_1p", new Action<string>(PrintUserMsg));
            dataBase.addCommand("print_mp", new Action<string[]>(PrintLines));

            // Add Lambda with no parameters
            dataBase.addCommand("lambda", new Action(() => { Debug.Log("Printing a default message to console"); }));
            dataBase.addCommand("lambda_1p", new Action<string>((arg) => { Debug.Log($"Log User Lambda Message: '{arg}'"); })); // Corrected spelling from 'Massage' to 'Message'
            dataBase.addCommand("lambda_mp", new Action<string[]>((args) => { Debug.Log(string.Join(", ", args)); }));

            // Add coroutine with no parameters
            dataBase.addCommand("process", new Func<IEnumerator>(SimpelProcess));
            dataBase.addCommand("process_1p", new Func<string, IEnumerator>(LineProcess));
            dataBase.addCommand("process_mp", new Func<string[], IEnumerator>(MultiLineProcess));

            //speial command
            dataBase.addCommand("MoveCharacterDemo", new Func<string, IEnumerator>(characterMove));
        }

        private static void PrintDefaultMsg()
        {
            Debug.Log("Printing a default message to console"); // Corrected spelling
        }

        private static void PrintUserMsg(string message) // Corrected spelling from 'massage' to 'message'
        {
            Debug.Log($"User  Message: '{message}'"); // Corrected spelling from 'Massage' to 'Message'
        }

        private static void PrintLines(string[] lines)
        {
            int i = 1;
            foreach (string line in lines)
            {
                Debug.Log($"{i++}. '{line}'"); // Corrected to log the individual line instead of the entire array
            }
        }

        private static IEnumerator SimpelProcess()
        {
            for (int i = 1; i <= 5; i++)
            {
                Debug.Log($"Process Running... [{i}]");
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator LineProcess(string data)
        {
            if (int.TryParse(data, out int num))
            {
                for (int i = 1; i <= num; i++)
                {
                    Debug.Log($"Process Running... [{i}]");
                    yield return new WaitForSeconds(1);
                }
            }
        }

        private static IEnumerator MultiLineProcess(string[] data)
        {
            foreach (string line in data)
            {
                Debug.Log($"Process Message: '{line}'"); // Corrected spelling from 'Massage' to 'Message'
                yield return new WaitForSeconds(0.5f);
            }
        }

        private static IEnumerator characterMove(string direction)
        {
            bool left = direction.ToLower() == "left";

            // Find the character
            Transform character = GameObject.Find("Image").transform;
            float moveSpeed = 15f;

            // Char is left or right?
            float targetX = left ? -8 : 8;

            // Calculate
            float currentX = character.position.x;

            // Move smoothly to the position
            while (Mathf.Abs(targetX - currentX) > 0.1f) // Changed Math.Abs to Mathf.Abs
            {
                Debug.Log($"Moving char to {(left ? "left" : "right")} [{currentX}/ {targetX}]"); // Removed extra parenthesis
                currentX = Mathf.MoveTowards(currentX, targetX, moveSpeed * Time.deltaTime);
                character.position = new Vector3(currentX, character.position.y, character.position.z);
                yield return null; // Added yield return null to allow coroutine to yield
            }
        }
    }
}