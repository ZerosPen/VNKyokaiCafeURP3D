using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GraphicObject
{
    private const string Name_Format = "Graphic - [{0}]";
    private const string Material_Path = "Materials/layerTransitionMaterial";
    private const string Material_Field_Color = "_Color";
    private const string Material_Field_MainTex = "_MainTex";
    private const string Material_Field_BlendTexture = "_BlendTex";
    private const string Material_Field_Blend = "_Blend";
    private const string Material_Field_Alpha = "_Alpha";

    public RawImage renderer;

    private GraphicLayer layer;
            
    public bool isVideo { get { return video != null; } }
    public VideoPlayer video = null;
    public AudioSource audio = null;

    public string GraphicPath = "";
    public string graphicName { get; private set; }

    private Coroutine co_FadingIn = null;
    private Coroutine co_Fadingout = null;

    public GraphicObject(GraphicLayer layer, string graphicPath, Texture tex, bool immediate)
    {
        this.GraphicPath = graphicPath;
        this.layer = layer;

        GameObject ob = new GameObject();
        ob.transform.SetParent(layer.panel);
        renderer = ob.AddComponent<RawImage>();

        graphicName = tex.name;

        InitGraphic(immediate);
        
        renderer.name = string.Format(Name_Format, graphicName);
        renderer.material.SetTexture(Material_Field_MainTex, tex);
    }

    public GraphicObject(GraphicLayer layer, string graphicPath, VideoClip clip, bool useAudio, bool immediate)
    {
        this.GraphicPath = graphicPath;
        this.layer = layer;

        GameObject ob = new GameObject();
        ob.transform.SetParent(layer.panel);
        renderer = ob.AddComponent<RawImage>();

        graphicName = clip.name;
        renderer.name = string.Format(Name_Format, graphicName);

        InitGraphic(immediate);

        RenderTexture tex = new RenderTexture(Mathf.RoundToInt(clip.width), Mathf.RoundToInt(clip.height), 0);
        renderer.material.SetTexture(Material_Field_MainTex, tex);

        video = renderer.AddComponent<VideoPlayer>();
        video.playOnAwake = true;
        video.source = VideoSource.VideoClip;
        video.clip = clip;
        video.renderMode = VideoRenderMode.RenderTexture;
        video.targetTexture = tex;
        video.isLooping = true;

        video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        audio = video.AddComponent<AudioSource>();
    
        audio.volume = immediate ? 1 : 0;
        if(!useAudio)
            audio.mute = true;

        video.SetTargetAudioSource(0, audio);

        video.frame = 0;
        video.Prepare();
        video.Play();

        video.enabled = false;
        video.enabled = true;
    }

    private void InitGraphic(bool immediate)
    {
        renderer.transform.localPosition = Vector3.zero;
        renderer.transform.localScale = Vector3.one;

        RectTransform rect = renderer.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;

        renderer.material = GetTransitionMaterial();

        float startingOpacity = immediate ? 1.0f : 0.0f;

        renderer.material.SetFloat(Material_Field_Blend, startingOpacity);
        renderer.material.SetFloat(Material_Field_Alpha, startingOpacity);
    }

    private Material GetTransitionMaterial()
    {
        Material mat =  Resources.Load<Material>(Material_Path);

        if(mat != null)
            return new Material(mat);

        return null;
    }

    GraphicPanelManager panelManager => GraphicPanelManager.Instance;

    public Coroutine FadeIn(float speed =1f, Texture blend = null)
    {
        if(co_Fadingout != null)
        {
           panelManager.StopCoroutine(co_Fadingout);
        }
        if(co_FadingIn != null)
            return co_FadingIn;

        co_FadingIn = panelManager.StartCoroutine(Fading(1f, speed, blend));

        return co_FadingIn;
    }

    public Coroutine FadeOut(float speed = 1f, Texture blend = null)
    {
        if (co_FadingIn != null)
        {
            panelManager.StopCoroutine(co_FadingIn);
        }
        if (co_Fadingout != null)
            return co_Fadingout;

        co_Fadingout = panelManager.StartCoroutine(Fading(0f, speed, blend));

        return co_Fadingout;
    }

    private IEnumerator Fading(float target, float speed, Texture blend)
    {
        bool isBlending = blend != null;
        bool fadingIn = target > 0;

        renderer.material.SetTexture(Material_Field_BlendTexture, blend);
        renderer.material.SetFloat(Material_Field_Alpha, isBlending ? 1f : fadingIn ? 0 : 1);
        renderer.material.SetFloat(Material_Field_Blend, isBlending ? fadingIn ? 0 : 1 : 1);

        string opacityParam = isBlending ? Material_Field_Blend : Material_Field_Alpha;

        while (renderer.material.GetFloat(opacityParam) != target)
        {
            float opacity = Mathf.MoveTowards(renderer.material.GetFloat(opacityParam), target, speed * Time.deltaTime);
            renderer.material.SetFloat(opacityParam, opacity);

            if(isVideo)
                audio.volume = opacity;

            yield return null;
        }

        co_FadingIn = null;
        co_Fadingout = null;

        if (target == 0)
        {
            Destroy();
        }
        else
            DestroyBackGroundGraphicOnLayer();
    }

    public void Destroy()
    {
        if(layer.currentGraphic != null && layer.currentGraphic.renderer == renderer)
        {
            layer.currentGraphic = null;
        }

        Object.Destroy(renderer.gameObject);
    }

    private void DestroyBackGroundGraphicOnLayer()
    {
        layer.DestroyOldGraphic();
    }
}
