using System;
using System.Collections;
using System.Collections.Generic;
using TESTING;
using UnityEngine;

namespace Commands
{
    public class CMD_DataBaseExtension_General : cmd_DataBaseExtension
    {
        new public static void Extend(CommandDataBase dataBase)
        {
            dataBase.addCommand("wait", new Func<string, IEnumerator>(Wait));
        }

        private static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float time))
            {
                yield return new WaitForSeconds(time);
            }
        }
    }
}