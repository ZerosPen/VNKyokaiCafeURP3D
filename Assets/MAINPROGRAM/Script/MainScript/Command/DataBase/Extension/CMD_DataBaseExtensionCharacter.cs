using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor;

namespace Commands
{
    public class CMD_DataBaseExtensionCharacter : cmd_DataBaseExtension
    {
        private static string[] Param_Enable => new string[] { "-e", "-enable" };
        private static string[] Param_Immediate => new string[] { "-i", "-immediate" };
        private static string[] Param_Speed => new string[] { "-spd", "-speed" };
        private static string[] Param_Smooth => new string[] { "-sm", "-smooth" };
        private static string Param_XPos => "-x";
        private static string Param_YPos => "-y";

        new public static void Extend(CommandDataBase dataBase)
        {
            dataBase.addCommand("creatcharacter", new Action<string[]>(CreatCharacter));
            dataBase.addCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            dataBase.addCommand("show", new Func<string[], IEnumerator>(ShowAll));
            dataBase.addCommand("hide", new Func<string[], IEnumerator>(HideAll));
            dataBase.addCommand("sort", new Action<string[]>(Sort));
            dataBase.addCommand("highlight", new Func<string[], IEnumerator>(HighLightAll));
            dataBase.addCommand("unhighlight", new Func<string[], IEnumerator>(UnHighLightAll));

            //Add commads to Characters
            CommandDataBase baseCommand = CommandManager.Instance.CreatSubDataBase(CommandManager.DataBase_characters_Base);
            baseCommand.addCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseCommand.addCommand("show", new Func<string[], IEnumerator>(Show));
            baseCommand.addCommand("hide", new Func<string[], IEnumerator>(Hide));
            baseCommand.addCommand("setpriority", new Action<string[]>(SetPriority));
            baseCommand.addCommand("setposition", new Action<string[]>(SetPosition));
            baseCommand.addCommand("setcolor", new Func<string[], IEnumerator>(SetColor));
            baseCommand.addCommand("highlight", new Func<string[], IEnumerator>(HighLight));
            baseCommand.addCommand("unhighlight", new Func<string[], IEnumerator>(UnHighLight));

            //add character specific dataBase
            CommandDataBase spriteCommands = CommandManager.Instance.CreatSubDataBase(CommandManager.DataBase_characters_Sprite);
            spriteCommands.addCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));
        }

        #region Global Commands
        public static void CreatCharacter(string[] data)
        {
            string characterName = data[0];
            bool enable = false;
            bool immediate = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Enable, out enable, defaultValue: false);
            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);

            Character character = CharacterManager.Instance.createChracter(characterName);

            if (!enable)
                return;

            if (immediate)
                character.isVisible = true;
            else if (enable)
                character.Show();
        }

        public static void Sort(string[] data)
        {
            CharacterManager.Instance.SortCharacters(data);
        }

        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.Instance.GetCharacter(characterName);
            if (character == null)
                yield break;

            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;

            var parameters = ConvertDataToParameters(data);

            //Try to get the X axis pos
            parameters.TryGetValue(Param_XPos, out x);

            //Try to get the Y axis pos
            parameters.TryGetValue(Param_YPos, out y);

            //Try to get the speed
            parameters.TryGetValue(Param_Speed, out speed, defaultValue: 1);

            //Try to get the smooth
            parameters.TryGetValue(Param_Smooth, out smooth, defaultValue: false);

            //Try to get the immedaite
            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);

            Vector2 position = new Vector2(x, y);

            if (immediate)
                character.SetPosition(position);
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(position); });
                yield return character.MoveToNewPosition(position, speed, smooth);
            }
        }

        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> chacarters = new List<Character>();
            bool immediate = false;
            float speed = 1f;

            foreach (string s in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(s, creatIfDoesNotAxist: false);
                if (character != null)
                    chacarters.Add(character);
            }

            if (chacarters.Count == 0)
                yield break;

            //Convert the data arry to a parameter container 
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);
            parameters.TryGetValue(Param_Speed, out speed, defaultValue: 1f);

            //call the logic on all the character
            foreach (Character character in chacarters)
            {
                if (immediate)
                    character.isVisible = true;
                else
                    character.Show(speed);
            }

            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in chacarters)
                        character.isVisible = true;
                });
                while (chacarters.Any(c => c.isShowing))
                    yield return null;
            }
        }

        public static IEnumerator HideAll(string[] data)
        {
            List<Character> chacarters = new List<Character>();
            bool immediate = false;
            float speed = 1f;

            foreach (string s in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(s, creatIfDoesNotAxist: false);
                if (character != null)
                    chacarters.Add(character);
            }

            if (chacarters.Count == 0)
                yield break;

            //Convert the data arry to a parameter container 
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);
            parameters.TryGetValue(Param_Speed, out speed, defaultValue: 1f);

            //call the logic on all the character
            foreach (Character character in chacarters)
            {
                if (immediate)
                    character.isVisible = false;
                else
                    character.Hide(speed);
            }

            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in chacarters)
                        character.isVisible = false;
                });

                while (chacarters.Any(c => c.isHidding))
                    yield return null;
            }
        }

        public static IEnumerator HighLightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handelUnspecifideCharacters = true;
            List<Character> unspescifiedCharacter = new List<Character>();
            
            //add and find any character that specified to Highlight
            for(int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.Instance.GetCharacter(data[i], creatIfDoesNotAxist: false);
                if (character != null)
                    characters.Add(character);
            }

            if(characters.Count == 0)
                yield break;
            //Try get the parameters
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handelUnspecifideCharacters, defaultValue: true);

            //make all character to perfom the logic
            foreach (Character character in characters)
                character.HighLight(immadiate: immediate);

            //if we are force any unspacified character to Highlight
            if (handelUnspecifideCharacters)
            {
                foreach (Character character in CharacterManager.Instance.AllCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspescifiedCharacter.Add(character);
                    character.UnHighLight(immadiate: immediate);
                }
            }

            //wait for all charcter to finish HighLight
            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach(var character in characters)
                    {
                        character.HighLight(immadiate: true);
                    }

                    if (!handelUnspecifideCharacters)
                        return;

                    foreach(var character in unspescifiedCharacter)
                    {
                        character.UnHighLight(immadiate: true);
                    }
                });

                while (characters.Any(c => c.isHighLighting) || (handelUnspecifideCharacters && unspescifiedCharacter.Any(uc => uc.isHighLighting)))
                    yield return null;
            }

        }

        public static IEnumerator UnHighLightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            bool handelUnspecifideCharacters = true;
            List<Character> unspescifiedCharacter = new List<Character>();

            //add and find any character that specified to Highlight
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.Instance.GetCharacter(data[i], creatIfDoesNotAxist: false);
                if (character != null)
                    characters.Add(character);
            }

            if (characters.Count == 0)
                yield break;
            //Try get the parameters
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handelUnspecifideCharacters, defaultValue: true);

            //make all character to perfom the logic
            foreach (Character character in characters)
                character.UnHighLight(immadiate: immediate);

            //if we are force any unspacified character to Highlight
            if (handelUnspecifideCharacters)
            {
                foreach (Character character in CharacterManager.Instance.AllCharacters)
                {
                    if (characters.Contains(character))
                        continue;

                    unspescifiedCharacter.Add(character);
                    character.HighLight(immadiate: immediate);
                }
            }

            //wait for all charcter to finish HighLight
            if (!immediate)
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                    {
                        character.UnHighLight(immadiate: true);
                    }

                    if (!handelUnspecifideCharacters)
                        return;

                    foreach (var character in unspescifiedCharacter)
                    {
                        character.HighLight(immadiate: true);
                    }
                });

                while (characters.Any(c => c.isUnHighLighting) || (handelUnspecifideCharacters && unspescifiedCharacter.Any(uc => uc.isUnHighLighting)))
                    yield return null;
            }
        }
        #endregion

        #region BASE CHARACTERS COMMANDS

        public static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0]);

            if (character == null)
                yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = true;
            else
            {
                //A long process should have a stop action to cancel out the coroutine and run logic that should complete this command  
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = true; });

                yield return character.Show();
            }

        }

        public static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0]);

            if (character == null)
                yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = false;
            else
            {
                //A long process should have a stop action to cancel out the coroutine and run logic that should complete this command  
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { if (character != null) character.isVisible = false; });

                yield return character.Hide();
            }
        }

        public static void SetPosition(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false);
            float x = 0, y = 0;

            if (character == null || data.Length < 2)
                return;

            var parameters = ConvertDataToParameters(data, 1);
            parameters.TryGetValue(Param_XPos, out x, defaultValue: 0);
            parameters.TryGetValue(Param_YPos, out y, defaultValue: 0);

            character.SetPosition(new Vector2(x, y));
        }

        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false);
            int priority;

            if (character == null || data.Length < 2)
            {
                return;
            }

            if (!int.TryParse(data[1], out priority))
                priority = 0;

            character.SetPriority(priority);
        }

        public static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false);
            string colorName;
            float speed;
            bool immadiate;

            if (character == null || data.Length < 2)
                yield break;

            var parameters = ConvertDataToParameters(data, startIndex: 1);

            //try to get color Name
            parameters.TryGetValue(new string[] { "-c", "-color" }, out colorName);

            //try to get speed
            bool specifiedSpeed = parameters.TryGetValue(Param_Speed, out speed, defaultValue: 1f);

            //try to get immadiate
            parameters.TryGetValue(Param_Immediate, out immadiate, defaultValue: false);

            //get color
            Color color = Color.white;
            color = color.GetColorFromName(colorName);

            if (immadiate)
                character.SetColor(color);
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.SetColor(color); });
                character.TransisitioColor(color, speed);
            }
            yield break;
        }

        public static IEnumerator HighLight(string[] data)
            {
            Character character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false);

            if(character == null)
                yield break;

            bool immadiate = false;

            var parameters = ConvertDataToParameters(data, startIndex: 1);

            parameters.TryGetValue(new string[] { "-i", "-immandiate" }, out immadiate, defaultValue: false);

            if (immadiate)
            {
                character.HighLight(immadiate: true);
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.HighLight(immadiate: true); }); 
                yield return character.HighLight();
            }
        }

        public static IEnumerator UnHighLight(string[] data)
        {
            Character character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false);

            if (character == null)
                yield break;

            bool immadiate = false;

            var parameters = ConvertDataToParameters(data, startIndex: 1);

            parameters.TryGetValue(new string[] { "-i", "-immandiate" }, out immadiate, defaultValue: false);

            if (immadiate)
            {
                character.UnHighLight(immadiate: true);
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.UnHighLight(immadiate: true); });
                yield return character.UnHighLight();
            }
        }

        #endregion

        #region Character Sprite
        public static IEnumerator SetSprite(string[] data)
        {
            //format SetSprite
            Character_Sprite character = CharacterManager.Instance.GetCharacter(data[0], creatIfDoesNotAxist: false) as Character_Sprite;
            int layer = 0;
            string spriteName;
            bool immadiate = false;
            float speed;

            if(character == null || data.Length < 2)
                yield break;

            //grab parameters
            var parameters = ConvertDataToParameters(data, startIndex: 1);
            
            //Try find parameter sprite
            parameters.TryGetValue(new string[] { "-s", "-sprite" }, out spriteName);

            //Try find parameter layer
            parameters.TryGetValue(new string[] { "-l", "-layer" }, out layer, defaultValue: 0);

            //Try find parameter speed
            bool specifedSpeed = parameters.TryGetValue(Param_Speed, out speed, defaultValue: 0.1f);

            if (!specifedSpeed)
            {
                parameters.TryGetValue(Param_Immediate, out immadiate, defaultValue: true);
            }

            Sprite sprite = character.GetSprite(spriteName);

            if(sprite == null)
                yield break;

            if(immadiate)
            {
                character.SetSprite(sprite, layer);
            }
            else
            {
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { character?.SetSprite(sprite, layer); });
                yield return character.TransitionSprite(sprite, layer, speed);
            }
        }

        #endregion
    }
}