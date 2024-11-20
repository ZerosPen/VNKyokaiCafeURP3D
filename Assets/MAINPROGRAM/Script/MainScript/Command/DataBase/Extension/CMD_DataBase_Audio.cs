using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Media;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace Commands
{
    public class CMD_DataBase_Audio : cmd_DataBaseExtension
    {
        private static string[] Param_Sfx = new string[] { "-s", "-sfx" };
        private static string[] Param_Volume = new string[] { "-v", "-vol" ,"-volume" };
        private static string[] Param_Pitch = new string[] { "-p", "-pitch" };
        private static string[] Param_Loop = new string[] { "-l", "-loop" };
        private static string[] Param_StartVolume = new string[] { "-sv", "startvolume" };

        private static string[] Param_Channel = new string[] { "-c", "-channel" };
        private static string[] Param_Immadiate = new string[] { "-i", "-immadiate" };
        private static string[] Param_Song = new string[] { "-s", "-song" };
        private static string[] Param_Ambience = new string[] { "-a", "-ambience" };

        new public static void Extend(CommandDataBase dataBase)
        {
            dataBase.addCommand("playsfx", new Action<string[]>(PlaySfx));
            dataBase.addCommand("stopsfx", new Action<string>(StopSfx));

            dataBase.addCommand("playvoice", new Action<string[]>(PlayVoice));
            dataBase.addCommand("stopvoice", new Action<string>(StopSfx));

            dataBase.addCommand("playsong", new Action<string[]>(PlaySong));
            dataBase.addCommand("playambience", new Action<string[]>(PlayAmbience));

            dataBase.addCommand("stopsong", new Action<string>(StopSong));
            dataBase.addCommand("stopambience", new Action<string>(StopAmbience));
        }

        public static void PlaySfx(string[] data)
        {
            string filePath;
            float Volume, pitch;
            bool loop;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Sfx, out filePath);

            parameters.TryGetValue(Param_Volume, out Volume, defaultValue: 1f);

            parameters.TryGetValue(Param_Pitch, out pitch, defaultValue: 1f);

            parameters.TryGetValue(Param_Loop, out loop, defaultValue: false);

            AudioClip sound = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_sfx, filePath));

            if(sound == null)
                return;

            AudioManager.instance.PlaySoundEffect(sound, volume: Volume, pitch: pitch, loop: loop);
        }

        public static void StopSfx(string data)
        {
            AudioManager.instance.StopSoundEffect(data);
        }

        public static void PlayVoice(string[] data)
        {
            string filePath;
            float Volume, pitch;
            bool loop;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Sfx, out filePath);

            parameters.TryGetValue(Param_Volume, out Volume, defaultValue: 1f);

            parameters.TryGetValue(Param_Pitch, out pitch, defaultValue: 1f);

            parameters.TryGetValue(Param_Loop, out loop, defaultValue: false);

            AudioClip sound = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_voices, filePath));

            if (sound == null)
                return;

            AudioManager.instance.PlayVoice(sound, volume: Volume, pitch: pitch, loop: loop);
        }

        public static void PlaySong(string[] data)
        {
            string filePath;
            int channel;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Song, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_music, filePath);

            parameters.TryGetValue(Param_Channel, out channel, defaultValue: 1);

            PlayTrack(filePath, channel, parameters);
        }

        public static void PlayAmbience(string[] data)
        {
            string filePath;
            int channel;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(Param_Ambience, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_ambience, filePath);

            parameters.TryGetValue(Param_Channel, out channel, defaultValue: 0);

            PlayTrack(filePath, channel, parameters);
        }

        private static void StopSong(string data)
        {
            if (data == string.Empty)
                StopTrack("1");
            else
                StopTrack(data);
        }

        private static void StopAmbience(string data)
        {
            if (data == string.Empty)
                StopTrack("0");
            else
                StopTrack(data);
        }

        private static void PlayTrack(string filepath, int channel, CommandParameters parameters)
        {
            bool loop;
            float volumeCap;
            float startVolume;
            float pitch;

            parameters.TryGetValue(Param_Volume, out volumeCap, defaultValue: 1f);

            parameters.TryGetValue(Param_StartVolume, out startVolume, defaultValue: 0f);

            parameters.TryGetValue(Param_Pitch, out pitch, defaultValue: 1f);

            parameters.TryGetValue(Param_Loop, out loop, defaultValue: true);

            //run logic
            AudioClip sound = Resources.Load<AudioClip>(filepath);

            if(sound == null)
            {
                Debug.LogError($"cannot load voice '{filepath}'");
                return;
            }
            AudioManager.instance.PlayTrack(sound, channel, loop, startVolume, volumeCap, pitch, filepath);
        } 

        public static void StopTrack(string data)
        {
            if(int.TryParse(data, out int channel))
            {
                AudioManager.instance.StopTrack(channel);
            }
            else
                AudioManager.instance.StopTrack(data);
        }

    }
}