using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Commands
{
    public class CMD_DataBase_GraphicPanel : cmd_DataBaseExtension
    {
        private static string[] Param_Panel = new string[] { "-p", "-panel" };
        private static string[] Param_Layer = new string[] { "-l", "-layer" };
        private static string[] Param_Media = new string[] { "-m", "-media" };
        private static string[] Param_Speed = new string[] { "-spd", "-speed" };
        private static string[] Param_Immediate = new string[] { "-i", "-immediate" };
        private static string[] Param_BlendTex = new string[] { "-b", "-blend" };
        private static string[] Param_UseVideoAudio = new string[] { "-aud", "-audio" };

        private static string HomeDirectory_Symbol = "~/";

        new public static void Extend(CommandDataBase dataBase)
        {
            dataBase.addCommand("setlayermedia", new Func<string[], IEnumerator>(SetLayerMedia));
            dataBase.addCommand("clearlayermedia", new Func<string[], IEnumerator>(ClearLayerMedia));
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            //parameter available to function
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            bool useAudio = false;

            string pathToGraphic = "";
            UnityEngine.Object graphic = null;
            Texture blendTex = null;

            var parameters = ConvertDataToParameters(data);

            //Try get the panel that this media is applied to
            parameters.TryGetValue(Param_Panel, out panelName);
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel(panelName);
            
            if(panel == null)
            {
                Debug.LogError($"Unable to grab panet '{panelName}' because it is not a valid panle. Please check the panel and adjust the command!");
                yield break;
            }

            //Try to get the layer to apply this graphic to
            parameters.TryGetValue(Param_Layer, out layer, defaultValue: 0);

            //Try to get the media or graphic
            parameters.TryGetValue(Param_Media, out mediaName);

            //Try to get this a immediate effect or not
            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);

            //try to get transitionSpeed
            if (!immediate)
                parameters.TryGetValue(Param_Speed, out transitionSpeed, defaultValue: 1f);

            //Try to get the layer to apply this graphic to
            parameters.TryGetValue(Param_BlendTex, out blendTexName);

            //Try to get the layer to apply this graphic to
            parameters.TryGetValue(Param_UseVideoAudio, out useAudio, defaultValue: false);

            pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_backgroundImages, mediaName);
            graphic = Resources.Load<Texture>(pathToGraphic);

            if(graphic == null)
            {
                pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_backgroundImages, mediaName);
                graphic = Resources.Load<VideoClip>(pathToGraphic);
            }

            if(graphic == null)
            {
                Debug.LogError($"Could not find media file called '{mediaName}' in the Resource directories. Please specify the full path within resource and make sure that the files exists!");
                yield break;
            }

            if(!immediate && blendTexName != string.Empty)
            {
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTexture + blendTexName);
            }

            //Try to get the layer to apply to the media
            GraphicLayer graphicLayer = panel.GetLayer(layer, createIfDoesNotExist: true);

            if(graphic is Texture)
            {
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTex, pathToGraphic, immediate);
            }
            else
            {
                yield return graphicLayer.SetVideo(graphic as VideoClip, transitionSpeed, useAudio, blendTex, pathToGraphic, immediate);
            }    
        }

        private static IEnumerator ClearLayerMedia(string[] data) 
        {
            //parameter available to function
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";

            Texture blendTex = null;

            //get the parameters
            var parameters = ConvertDataToParameters(data);

            //Try get the panel that this media is applied to
            parameters.TryGetValue(Param_Panel, out panelName);
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel(panelName);

            if (panel == null)
            {
                Debug.LogError($"Unable to grab panet '{panelName}' because it is not a valid panle. Please check the panel and adjust the command!");
                yield break;
            }

            // Try to get the layer to apply this graphic to
            parameters.TryGetValue(Param_Layer, out layer, defaultValue: -1);

            //Try to get the media or graphic
            parameters.TryGetValue(Param_Media, out mediaName);

            //Try to get this a immediate effect or not
            parameters.TryGetValue(Param_Immediate, out immediate, defaultValue: false);

            //try to get transitionSpeed
            if (!immediate)
                parameters.TryGetValue(Param_Speed, out transitionSpeed, defaultValue: 1f);

            //try to get the blending texture for the media if we are using one.
            parameters.TryGetValue(Param_BlendTex, out blendTexName);

            if(!immediate && blendTexName != string.Empty)
            {
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTexture + blendTexName);
            }

            if(layer == -1)
                panel.Clear(transitionSpeed, blendTex, immediate);
            else
            {
                GraphicLayer graphicLayer = panel.GetLayer(layer);
                if(graphicLayer == null)
                {
                    Debug.LogError($"Could not clear layer [{layer}] on panel '{panel.panelName}'!");
                    yield break;
                }
                graphicLayer.Clear(transitionSpeed, blendTex, immediate);
            }
        }

        private static string GetPathGraphic(string defaultPath, string graphicName)
        {
            if(graphicName.StartsWith(HomeDirectory_Symbol))
                return graphicName.Substring(HomeDirectory_Symbol.Length);

            return defaultPath + graphicName;
        }
    }
}