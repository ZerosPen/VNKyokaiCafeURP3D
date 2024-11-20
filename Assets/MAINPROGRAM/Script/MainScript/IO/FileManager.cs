using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    public static List<string> readTxtFiles(string filepath, bool includeBlankLines = true)
    {
        if (!filepath.StartsWith('/'))
            filepath = FilePaths.root + filepath;

        Debug.Log($"Attempting to read file at: {filepath}"); // Debugging line

        List<string> lines = new List<string>();

        // Check if the file exists before trying to read it
        if (!File.Exists(filepath))
        {
            Debug.LogError($"File does not exist at: {filepath}");
            return lines; // Return early if the file doesn't exist
        }

        try
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (includeBlankLines || !string.IsNullOrEmpty(line))
                        lines.Add(line);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"File not found: '{ex.FileName}'");
        }
        catch (Exception ex) // Catch other potential exceptions
        {
            Debug.LogError($"An error occurred while reading the file: {ex.Message}");
        }

        return lines;
    }

    public static List<string> readTxtAsset(string filepath, bool includeBlankLines = true)
    {
        TextAsset asset = Resources.Load<TextAsset>(filepath);
        if (asset == null)
        {
            Debug.LogError($"Asset Not Found: '{filepath}'");
            return null;
        }

        return readTxtAsset(asset, includeBlankLines);
    }

    public static List<string> readTxtAsset(TextAsset asset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(asset.text))
        {
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlankLines || !string.IsNullOrEmpty(line))
                    lines.Add(line);
            }
        }
        return lines;
    }
}