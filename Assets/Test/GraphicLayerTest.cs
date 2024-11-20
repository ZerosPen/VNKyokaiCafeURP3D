using Characters;
using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicLayerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(test2());
    }

    IEnumerator test()
    {
        GraphicPanel panel = GraphicPanelManager.Instance.GetPanel("BackGround");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        //Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
        //layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);

        //yield return new WaitForSeconds(1);

        //layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", blendingTexture: blendTex);

        //yield return new WaitForSeconds(3);

        //layer.currentGraphic.FadeOut();

        //yield return new WaitForSeconds(1);

        //Debug.Log(layer.currentGraphic);

        yield return null;
    }

    IEnumerator test1()
    {
        GraphicPanel panel = GraphicPanelManager.Instance.GetPanel("BackGround");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        yield return null;
    }

    IEnumerator test2()
    {
        GraphicPanel panel = GraphicPanelManager.Instance.GetPanel("BackGround");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        GraphicPanel cinematic = GraphicPanelManager.Instance.GetPanel("Cinematic");
        GraphicLayer cinlayer = cinematic.GetLayer(0, true);

        Character Makira = CharacterManager.Instance.createChracter("Makira", true);

        yield return Makira.Say("What a lovely pic >_<");

        cinlayer.SetTexture("Graphics/Gallery/hiya");

        yield return DialogController.Instance.Say("", "What the heck is that!?");

        cinlayer.Clear();

        yield return new WaitForSeconds(1);

        panel.Clear();
    }
}
