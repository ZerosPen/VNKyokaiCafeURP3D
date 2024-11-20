using System.Collections;
using UnityEngine;
using TMPro;
using System;
using System.Globalization;

/*
    This script is able to make new text and restore existing text
*/

public class TextArchitech
{
    private TextMeshProUGUI Tmpro_GUI;
    private TextMeshPro Tmpro_World;
    public TMP_Text Tmpros => Tmpro_GUI != null ? Tmpro_GUI : Tmpro_World;
    /*
        public TMP_Text TMPros => Tmpro_GUI != null ? Tmpro_GUI : Tmpro_World;
        if ( Tmpro_GUI != null ) if true will using Tmpro_GUI else Tmpro_World
    */

    public string currentText => Tmpros.text;
    public string targetText { get; private set; } = "";
    /*
        public string targettext { get; private set; } = "";
        this text is retrievable but only can privet assignable
    */
    public string pretext { get; private set; } = "";
    /*
        public string prettext { get; private set; } = "";
        this all text been generet by TextArchitech it will be stored in here
        before the new text is tacked on top of
    */
    private int preTextlen = 0; // for cache of the pretext

    public string fullTargetText => pretext + targetText; 
    //this funtion for full string for targtText

    public enum BuildMethod { instant, typewritter, fade } // multiple buildmethod for writting
    public BuildMethod buildMetdod = BuildMethod.typewritter; // the default is typewritter

    public Color textColour { get { return Tmpros.color; } set {Tmpros.color = value ;} }
    /*
        public Color textColour { get { return Tmpros.color; } set {Tmpros.color = value ;} }
        this can make text have different colour
    */

    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    /*
        public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
        this a standar speed to come in screen, because baseSpeed cannot change so the value will be pass in speedMultiplier
    */
    private const float baseSpeed = 1; // how fast to type a text
    private float speedMultiplier = 1; // to multiply speed typing

    public int charPerCycle { get {return speed <= 2f ? charMultiplier : speed <= 2.5f ? charMultiplier * 2 : charMultiplier * 3;} }
    /*
        public int charPerCycle { get {return speed <= 2f ? charMultiplier : speed <= 2.5f ? charMultiplier * 2 : charMultiplier * 3;} }
        this check when speed si below or same 2 will be default if, speed  less the equel to 2.5 it will be multiply by 2, if more than 3 it will be multiply by 3
    */
    private int charMultiplier = 1;
    public bool speedUP = false;

    public TextArchitech(TextMeshProUGUI Tmpro_GUI)
    {
        this.Tmpro_GUI = Tmpro_GUI;
    }

    public TextArchitech(TextMeshPro Tmpro_World)
    {
        this.Tmpro_World = Tmpro_World;
    }

    public Coroutine Build(string text)
    {
        pretext = "";
        targetText = text;

        Stop();

        BuildProcess = Tmpros.StartCoroutine(Building()); // to start building process
        return BuildProcess; //to monitor building process
    }

    public Coroutine Append(string text)
    {
        /*
         public Coroutine Append(string text)
         able to build already in the text architech
        */
        pretext = Tmpros.text;
        targetText = text;

        Stop();

        BuildProcess = Tmpros.StartCoroutine(Building()); // to start building process
        return BuildProcess; //to monitor building process
    }

    private Coroutine BuildProcess = null;
    public bool isBuildingText => BuildProcess != null;
    /*
        public bool isBuildingText => BuildProcess != null;
        is for check building is on progress 
    */
    public void Stop()
    {
        if (!isBuildingText)
        {
            return;
        }

        Tmpros.StopCoroutine(BuildProcess);
        BuildProcess = null;
    }


    IEnumerator Building()
    {
        Prepare();

        switch (buildMetdod)
        {
            case BuildMethod.typewritter:
                yield return Building_TypeWritter();
                onComplete();
                break;
            case BuildMethod.fade:
                yield return Building_Fade();
                onComplete();
                break;
        }

        yield return null;
    }
    private void onComplete()
    {
        BuildProcess = null;
        speedUP = false;
    }

    public void forceComplete()
    {
        switch(buildMetdod) 
        {
            case BuildMethod.typewritter:
                Tmpros.maxVisibleCharacters = Tmpros.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                Tmpros.ForceMeshUpdate();
                break;
        }
        Stop();
        onComplete();
    }

    private void Prepare()
    {
        /*
        private void Prepare()
        prepare build text base on whichever build method, to able TextMashPro without any issue
        */
        switch (buildMetdod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewritter:
                Prepare_TypeWritter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }

    }
    private void Prepare_Instant()
    {
        /*
        private void Prepare_Instant()
        prepare build text base on whichever build method, to able TextMashPro without any issue
        Tmpros.color = Tmpros.color; // to re-intialize to default colour or reset the color
        Tmpros.text = fullTargetText; // make sure have text ready
        Tmpros.ForceMeshUpdate(); // Force to update
        Tmpros.maxVisibleCharacters = Tmpros.textInfo.characterCount; // make sure every char is visible
        */

        Tmpros.color = Tmpros.color;
        Tmpros.text = fullTargetText;
        Tmpros.ForceMeshUpdate();
        Tmpros.maxVisibleCharacters = Tmpros.textInfo.characterCount;
    }

    private void Prepare_TypeWritter()
    {
        /*
        private void Prepare_TypeWritter()
        prepare build text base on whichever build method, to able TextMashPro without any issue
        */
        //reset to default
        Tmpros.color = Tmpros.color;
        Tmpros.maxVisibleCharacters = 0;
        Tmpros.text = pretext;

        // check if pretext is ready
        if(pretext != "")
        {
            Tmpros.ForceMeshUpdate();
            Tmpros.maxVisibleCharacters = Tmpros.textInfo.characterCount;
        }

        // add to target
        Tmpros.text += targetText;
        Tmpros.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        /*
        private Prepare_Fade()
        prepare build text base on whichever build method, to able TextMashPro without any issue
        */

        Tmpros.text = pretext;
        if (pretext != "")
        {
            Tmpros.ForceMeshUpdate();
            preTextlen = Tmpros.textInfo.characterCount;
        }
        else 
            preTextlen = 0;

        Tmpros.text += targetText;
        Tmpros.maxVisibleCharacters = int.MaxValue;
        Tmpros.ForceMeshUpdate();

        TMP_TextInfo textInfo = Tmpros.textInfo;

        Color colorVisbel = new Color(textColour.r, textColour.g, textColour.b, 1);
        Color colorHide = new Color(textColour.r, textColour.g, textColour.b, 0);

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if(!charInfo.isVisible) continue;

            if (i < preTextlen)
            {
                for (int v = 0; v < 4; v++)
                {
                    vertexColors[charInfo.vertexIndex + v] = colorVisbel;
                }
            }
            else
            {
                for (int v = 0; v < 4; v++)
                {
                    vertexColors[charInfo.vertexIndex + v] = colorHide;
                }
            }
        }
        Tmpros.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    private IEnumerator Building_TypeWritter()
    {
        while(Tmpros.maxVisibleCharacters < Tmpros.textInfo.characterCount)
        {
            Tmpros.maxVisibleCharacters += speedUP ? charPerCycle * 5 : charPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
    }

    private IEnumerator Building_Fade()
    {
        int minRange = preTextlen;
        int maxRange = minRange + 1;

        byte alpaTherhold = 15;

        TMP_TextInfo txtInfo = Tmpros.textInfo;

        Color32[] vertexColours = txtInfo.meshInfo[txtInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alpha = new float[txtInfo.characterCount];

        while (true)
        {
            float fadeSpeed = ((speedUP ? charPerCycle * 5 : charPerCycle) * speed) * 4f;
            for (int i = minRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = txtInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                int vertexIndex = txtInfo.characterInfo[i].vertexIndex;
                alpha[i] = Mathf.MoveTowards(alpha[i], 255, fadeSpeed);

                for (int v = 0; v < 4; v++)
                {
                    vertexColours[charInfo.vertexIndex + v].a = (byte)alpha[i];
                    if (alpha[i] >= 255)
                    {
                        minRange++;
                    }
                }
                Tmpros.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                bool lasCharIsVis = !txtInfo.characterInfo[maxRange - 1].isVisible;
                if (alpha[maxRange -1] > alpaTherhold || lasCharIsVis)
                {
                    if(maxRange < txtInfo.characterCount)
                    {
                        maxRange++;
                    }
                    else if(alpha[maxRange - 1] >= 255 || lasCharIsVis)
                        break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
