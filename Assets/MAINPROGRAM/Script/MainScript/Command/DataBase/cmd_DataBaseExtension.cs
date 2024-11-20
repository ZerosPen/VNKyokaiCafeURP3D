using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
    public abstract class cmd_DataBaseExtension
    {
        public static void Extend(CommandDataBase dataBase)
        {
            // Implementation goes here
        }

        public static CommandParameters ConvertDataToParameters(string[] data, int startIndex = 0) => new CommandParameters(data, startIndex);
    }
}