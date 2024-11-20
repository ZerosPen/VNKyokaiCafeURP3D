using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    private const string Sfx_Parent_Name = "SFX";
    private const string Sfx_Name_Format = "SFX - [{0}]";
    public const float Track_Transition_Speed = 1f;

    public static AudioManager instance { get; private set; }

    public Dictionary <int, AudioChannel> channels = new Dictionary<int, AudioChannel>();

    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup voiceMixer;

    private Transform sfxRoot;

    private void Awake()
    {
        if(instance == null)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        sfxRoot = new GameObject(Sfx_Parent_Name).transform;
        sfxRoot.SetParent(transform);
    }

    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);

        if (clip == null)
        {
            Debug.LogError($"Could not load audio file '{filePath}'. Please ensure this audio clip is exists");
            return null;
        }

        return PlaySoundEffect(clip, mixer, volume, pitch, loop);
    }
    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        AudioSource effectSource = new GameObject(string.Format(Sfx_Name_Format, clip.name)).AddComponent<AudioSource>();
        effectSource.transform.SetParent(sfxRoot);
        effectSource.transform.position = sfxRoot.position;

        effectSource.clip = clip;

        if(mixer == null)
            mixer = sfxMixer;

        effectSource.outputAudioMixerGroup = mixer;
        effectSource.volume = volume;
        effectSource.spatialBlend = 0;
        effectSource.pitch = pitch;
        effectSource.loop = loop;

        effectSource.Play();

        if (!loop)
        {
            Destroy(effectSource.gameObject, (clip.length / pitch) + 1);
        }

        return effectSource;
    }

    public AudioSource PlayVoice(string filePath, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        return PlaySoundEffect(filePath, voiceMixer, volume, pitch, loop);
    }

    public AudioSource PlayVoice(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        return PlaySoundEffect(clip, voiceMixer, volume, pitch, loop);
    }

    public void StopSoundEffect(AudioClip clip) => StopSoundEffect(clip.name);

    public void StopSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();
        AudioSource[] sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach(var source in sources)
        {
            if(source.clip.name.ToLower() == soundName)
            {
                Destroy(source.gameObject);
                return;
            }
        }
    }

    public AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);

        if (clip == null)
        {
            Debug.LogError($"Could not load audio file '{filePath}'. Please ensure this audio clip is exists");
            return null;
        }

        return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch ,filePath);
    }

    public AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f ,string filePath = "")
    {
        AudioChannel audioChannel = TryGetChannel(channel, createIfdoesNotExist: true);
        AudioTrack track = audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, pitch , filePath);
        return track;
    }

    public void StopTrack(int channle)
    {
        AudioChannel c = TryGetChannel(channle, createIfdoesNotExist: false);

        if(c == null)
            return;

        c.StopTrack();
    }

    public void StopTrack(string trackName)
    {
        trackName = trackName.ToLower();

        foreach(var channel in channels.Values)
        {
            if(channel.activeTrack != null && channel.activeTrack.name.ToLower() == trackName)
            {
                channel.StopTrack();
                return;
            }
        }

    }

    public AudioChannel TryGetChannel(int channelNumber, bool createIfdoesNotExist = false)
    {
        AudioChannel channel = null;
        if(channels.TryGetValue(channelNumber, out channel))
        {
            return channel;
        }
        else if(createIfdoesNotExist)
        {
            channel =  new AudioChannel(channelNumber);
            channels.Add(channelNumber, channel);
            return channel;
        }

        return null;
    }
}
