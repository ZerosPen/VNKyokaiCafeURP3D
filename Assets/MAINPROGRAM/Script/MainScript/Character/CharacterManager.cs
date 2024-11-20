using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        public Character[] AllCharacters => characters.Values.ToArray();
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        private CharacterConfigSO config => DialogController.Instance.config.characterConfigationAsset;

        //private const string Char
        private const string Character_Casting_ID = " as ";
        private const string CharacterNameID = "<charactername>";
        public string characterRootPathFormat => $"Characters/{CharacterNameID}";
        public  string CharacterPerfabNameFormat => $"Character - [{CharacterNameID}]";
        public  string characterPrefabPathFormat => $"{characterRootPathFormat}/{CharacterPerfabNameFormat}";

        [SerializeField] private RectTransform _characterPanel = null;
        public RectTransform characterPanel => _characterPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                DestroyImmediate(gameObject);
        }

        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return config.GetConfig(characterName);
        }

        public Character GetCharacter(string charaterName, bool creatIfDoesNotAxist = false)
        {
            if (characters.ContainsKey(charaterName.ToLower()))
            {
                return characters[charaterName.ToLower()];
            }
            else if (creatIfDoesNotAxist)
            {
                return createChracter(charaterName);
            }

            return null;
        }

        public bool HasChraracter(string characterName) => characters.ContainsKey(characterName.ToLower());

        public Character createChracter(string characterName, bool revealAfterCreation = false)
        {
            //check you are making duplic character
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exist.  Do not create the character");
                return null;
            }

            CharacterInfo info = getCharacterInfo(characterName);

            Character character = CreaterCharFromInfo(info);

            characters.Add(info.name.ToLower(), character);

            if(revealAfterCreation)
                character.Show();

            return character;
        }

        private CharacterInfo getCharacterInfo(string characterName)
        {
            CharacterInfo result = new CharacterInfo();

            string[] nameData = characterName.Split(Character_Casting_ID, System.StringSplitOptions.RemoveEmptyEntries);
            result.name = nameData[0];
            result.castingName = nameData.Length > 1 ? nameData[1] : result.name;

            result.config = config.GetConfig(result.castingName);

            result.prefab = getPrefabForCharacter(result.castingName);

            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);
            if (string.IsNullOrEmpty(result.rootCharacterFolder))
            {
                Debug.LogError("Formatted character path is empty or invalid.");
            }

            return result;
        }

        private  GameObject getPrefabForCharacter(string characterName)
        {
           string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        public  string FormatCharacterPath(string path, string characterName) => path.Replace(CharacterNameID, characterName);


        private Character CreaterCharFromInfo(CharacterInfo info)
        {
            CharacterConfigData config = info.config;

            switch (config.CharType)
            {
                case Character.CharacterType.text:
                    return new Character_Text(info.name, config);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config, info.prefab, info.rootCharacterFolder);

                default:
                    return null;
            }
        }

        public void SortCharacters()
        {
            List<Character> activeCharacters = characters.Values.Where(c => c.mainGame.gameObject.activeInHierarchy && c.isVisible).ToList();
            List<Character> inactiveCharacters = characters.Values.Except(activeCharacters).ToList();
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority)); activeCharacters.Concat(inactiveCharacters);
            SortCharacters(activeCharacters);
        }

        public void SortCharacters(string[] characterNames)
        {
            List<Character> sortedCharacters = new List<Character>();

            sortedCharacters = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacters)
                .OrderBy(character => character.priority)
                .ToList();

            sortedCharacters.Reverse();

            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(c => c.priority) : 0;
            for(int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharacterOnUI: false);
            }

            List<Character> allCharacter = remainingCharacters.Concat(sortedCharacters).ToList();
            SortCharacters(allCharacter);
        }

        private void SortCharacters(List<Character> characterSortingOrder)
        {
            int i = 0;
            foreach (Character character in characterSortingOrder)
            {
                Debug.Log($"{character.name} priority is {character.priority}");
                character.mainGame.SetSiblingIndex(i++);
            }
        }

        private class CharacterInfo
        {
            public string name = "";
            public string castingName = "";

            public string rootCharacterFolder = "";
            public CharacterConfigData config = null;
            public GameObject prefab = null;
        }
    }
}