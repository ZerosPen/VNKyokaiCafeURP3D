using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.Events;
using Characters;

namespace Commands
{
    public class CommandManager : MonoBehaviour
    {
        private const char Sub_Command_Identifier  = '.';
        public const string DataBase_characters_Base = "characters";
        public const string DataBase_characters_Sprite = "characters_sprite";
        public const string DataBase_characters_Live2D = "characters_Live2D";
        public const string DataBase_characters_Model3D = "characters_Model3D";

        public static CommandManager Instance { get; private set; }
        
        private CommandDataBase commandDB;
        private Dictionary<string, CommandDataBase> subDatabases = new Dictionary<string, CommandDataBase>();

        private List<CommandProcess> activeProcess = new List<CommandProcess>();
        private CommandProcess topProcess => activeProcess.Last();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                commandDB = new CommandDataBase();

                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(cmd_DataBaseExtension))).ToArray();

                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod.Invoke(null, new object[] { commandDB });
                }
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        public CoroutineWrapper Executed(string commandName, params string[] args)
        {
            if (commandName.Contains(Sub_Command_Identifier))
            {
                return ExecutedSubCommand(commandName, args);
            }
            
            Delegate command = commandDB.GetCommand(commandName);

            if (command == null) // Change this line to check if command is not null
            {
                return null;
            }

            return StartProcess(commandName, command, args);
        }

        private CoroutineWrapper ExecutedSubCommand(string commandName, params string[] args)
        {
            string[] parts = commandName.Split(Sub_Command_Identifier);
            string dataBaseName = string.Join(Sub_Command_Identifier, parts.Take(parts.Length - 1));
            string subCommandName = parts.Last();

            if (subDatabases.ContainsKey(dataBaseName))
            {
                Delegate command = subDatabases[dataBaseName].GetCommand(subCommandName);
                if (command != null)
                {
                    return StartProcess(commandName, command, args);
                }
                else
                {
                    Debug.LogError($"No command called '{subCommandName}' was found in database '{dataBaseName}'");
                    return null;
                }
            }

            string characterName = dataBaseName;
            //if we got to down this the we should try as a character command
            if (CharacterManager.Instance.HasChraracter(characterName))
            {
                List<string> newArgs = new List<string>(args);
                newArgs.Insert(0, dataBaseName);
                args = newArgs.ToArray();

                return ExcuteCharacterCommand(subCommandName, args);
            }

            Debug.LogError($"No sub dataBase called '{dataBaseName}' exists! Command '{subCommandName}' could not be run.");
            return null;
        }

        private CoroutineWrapper ExcuteCharacterCommand(string commandName, params string[] args)
        {
            Delegate command = null;

            CommandDataBase commandDB = subDatabases[DataBase_characters_Base];
            if (commandDB.hasCommand(commandName))
            {
                command = commandDB.GetCommand(commandName);
                return StartProcess(commandName, command, args);
            }

            CharacterConfigData characterConfigData = CharacterManager.Instance.GetCharacterConfig(args[0]);
            switch (characterConfigData.CharType)
            {
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    commandDB = subDatabases[DataBase_characters_Sprite];
                    break;
                case Character.CharacterType.Live2D:
                    commandDB = subDatabases[DataBase_characters_Live2D];
                    break;
                case Character.CharacterType.Model3D:
                    commandDB = subDatabases[DataBase_characters_Model3D];
                    break;
            }

            command = commandDB.GetCommand(commandName);

            if (command != null)
                return StartProcess(commandName, command, args);

            Debug.LogError($"command Manager was unable to execute command '{commandName}' on character {args[0]}.  The character name or command may be invalid");
            return null;
        }

        private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
        {
            System.Guid processID = System.Guid.NewGuid();
            CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
            activeProcess.Add(cmd);

            Coroutine co = StartCoroutine(RunningProcess(cmd));

            cmd.runningProcess = new CoroutineWrapper(this, co);

            return cmd.runningProcess;
        }

        public void StopCurrentProcess()
        {
            if (topProcess != null)
                KillProcess(topProcess);
        }

        public void StopAllProcesses()
        {
            foreach(var c in activeProcess)
            {
                if(c.runningProcess != null && !c.runningProcess.isDone)
                {
                    c.runningProcess.Stop();
                }
                c.OnTerminateAction?.Invoke();
            }
            activeProcess.Clear();
        }

        private IEnumerator RunningProcess(CommandProcess process)
        {
            yield return WaitForProcessToComplete(process.command, process.args);

            KillProcess(process);
        }

        public void KillProcess(CommandProcess cmd)
        {
            activeProcess.Remove(cmd);

            if(cmd.runningProcess != null && !cmd.runningProcess.isDone)
            {
                cmd.runningProcess.Stop();
            }

            cmd.OnTerminateAction?.Invoke();
        }

        private IEnumerator WaitForProcessToComplete(Delegate command, string[] args)
        {
            if (command is Action)
                command.DynamicInvoke();

            else if (command is Action<string>)
                command.DynamicInvoke(args[0]);

            else if (command is Action<string[]>)
                command.DynamicInvoke((object)args);

            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();

            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args[0]);

            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command)(args);
        }

        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            CommandProcess process = topProcess;

            if(topProcess == null)
                return;

            process.OnTerminateAction = new UnityEvent();
            process.OnTerminateAction.AddListener(action);
        }
    
        public CommandDataBase CreatSubDataBase(string name)
        {
            name = name.ToLower();
            
            if(subDatabases.TryGetValue(name, out CommandDataBase commandDB))
            {
                Debug.LogWarning($"A database by the name of '{name}' already exists!");
                return commandDB;
            }
            CommandDataBase newDataBase = new CommandDataBase();
            subDatabases.Add(name, newDataBase);

            return newDataBase;
        }

    }
}