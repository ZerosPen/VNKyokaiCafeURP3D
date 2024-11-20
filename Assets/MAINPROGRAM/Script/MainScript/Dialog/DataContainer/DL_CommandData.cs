using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DIALOGUE
{
    public class DL_CommandData
    {
        public List<Command> commands;
        private const char CommandSplitter_Id = ',';
        private const char CommandArguments_Id = '(';
        private const string WaitCommand_Id = "[wait]";

        public struct Command
        {
            public string name;
            public string[] arguments;
            public bool waitForComplete;
        }

        public DL_CommandData(string rawCommands)
        {
            commands = RipCommands(rawCommands);
        }

        private List<Command> RipCommands(string rawCommands)
        {
            string[] data = rawCommands.Split(CommandSplitter_Id, System.StringSplitOptions.RemoveEmptyEntries);
            List<Command> result = new List<Command>();

            foreach (string cmd in data)
            {
                Command command = new Command();
                int index = cmd.IndexOf(CommandArguments_Id);
                command.name = cmd.Substring(0, index).Trim();

                if (command.name.ToLower().StartsWith(WaitCommand_Id))
                {
                    command.name = command.name.Substring(WaitCommand_Id.Length);
                    command.waitForComplete = true;
                }

                command.arguments = GetArgs(cmd.Substring(index + 1, cmd.Length - index - 2));
                result.Add(command);
            }
            return result;
        }

        private string[] GetArgs(string args)
        {
            List<string> argList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            bool InQoutes = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    InQoutes = !InQoutes;
                    continue;
                }

                if (!InQoutes && args[i] == ' ')
                {
                    argList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }
                currentArg.Append(args[i]);
            }
            if (currentArg.Length > 0)
            {
                argList.Add(currentArg.ToString());
            }
            return argList.ToArray();
        }
    }
}