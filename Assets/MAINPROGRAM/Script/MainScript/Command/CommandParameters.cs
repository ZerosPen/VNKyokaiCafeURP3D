using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
    public class CommandParameters
    {
        private const char Parameter_Identifier = '-';

        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private List<string> unLabelParameterNames = new List<string>();


        public CommandParameters(string[] parameterArry, int startIndex = 0) 
        { 
            for(int i = startIndex; i < parameterArry.Length; i++)
            {
                if (parameterArry[i].StartsWith(Parameter_Identifier) && !float.TryParse(parameterArry[i], out _))
                {
                    string pName = parameterArry[i];
                    string pValue = "";

                    if(i + 1 < parameterArry.Length && !parameterArry[i + 1].StartsWith(Parameter_Identifier))
                    {
                        pValue = parameterArry[i + 1];
                        i++;
                    }

                    parameters.Add(pName, pValue);
                }
                else
                    unLabelParameterNames.Add(parameterArry[i]);
            }
        }

        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);

        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            foreach(string parameterName in parameterNames)
            {
                if(parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if(TryCastParameter(parameterValue, out value))
                    {
                        return true;
                    }
                }
            }

            //if we reach here, no match was found in the identified parameters so search the unlabeled ones if present

            foreach (string parameterName in unLabelParameterNames)
            {
                    if (TryCastParameter(parameterName, out value))
                    {
                        unLabelParameterNames.Remove(parameterName);
                        return true;
                    }
            }

            value = defaultValue;
            return false;
        }
        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if(bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true ;
            }
            value = default(T);
            return false;
        }
    }
}