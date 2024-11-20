using Characters;
using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Test1());
    }

    Character CreatCharacter(string name) => CharacterManager.Instance.createChracter(name);

    IEnumerator Test()
    {
        yield return new WaitForSeconds(1);

        Character_Sprite Makima = CreatCharacter("Makima") as Character_Sprite;
        Makima.Show();

        yield return DialogController.Instance.Say("Narrator", "Can we see your ship?");

        GraphicPanelManager.Instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/5");
        AudioManager.instance.PlayTrack("Audio/Music/Upbeat", volumeCap: 0.5f);
        AudioManager.instance.PlayVoice("Audio/Voices/exclamation");

        Makima.SetSprite(Makima.GetSprite("Telling"));
        Makima.MoveToNewPosition(new Vector2(0.7f, 0), speed: 0.5f);
        yield return Makima.Say("Sure");

        yield return Makima.Say("Let's go to the beach >_<");

        GraphicPanelManager.Instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/BG Beach");
        AudioManager.instance.PlayTrack("Audio/Music/Calm", volumeCap: 0.5f);


        yield return null;
    }

    IEnumerator Test1()
    {
        Character_Sprite Makima = CreatCharacter("Makima") as Character_Sprite;
        Character Me = CreatCharacter("Me");
        Makima.Show();

        GraphicPanelManager.Instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/sleeping night");

        AudioManager.instance.PlayTrack("Audio/Ambience/RainyMood", 0);
        AudioManager.instance.PlayTrack("Audio/Music/Calm", 1, pitch: 0.7f);

        yield return Makima.Say("We can Have multipel channel");

        AudioManager.instance.StopTrack(1);
    }
}
