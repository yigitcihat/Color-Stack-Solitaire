using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource source;
    // private static readonly bool isAudioOn = true;
    internal readonly Dictionary<AudioClips, AudioClip> sounds = new();
    
    private void Start()
    {
        foreach (var sound in Sound.allCases) sounds.Add(sound, Resources.Load<AudioClip>($"Sounds/{sound}"));
    }
    
    internal void Play(AudioClips clip)
    {
        if(!Setting.sound.isOn) return;
        AudioSource.PlayClipAtPoint(sounds[clip], transform.position);
    }
}

public enum AudioClips { flip, move, success, tirt, cardShuffle, ding, goodMove, wrong, levelCompleted }

public class Sound
{
    public static readonly Sound flip = new(AudioClips.flip);
    public static readonly Sound move = new(AudioClips.move);
    public static readonly Sound success = new(AudioClips.success);
    public static readonly Sound tirt = new(AudioClips.tirt);
    public static readonly Sound cardShuffle = new(AudioClips.cardShuffle);
    public static readonly Sound ding = new(AudioClips.ding);
    public static readonly Sound goodmove = new(AudioClips.goodMove);
    public static readonly Sound wrong = new(AudioClips.wrong);
    public static readonly Sound levelCompleted = new(AudioClips.levelCompleted);

    internal static readonly List<AudioClips> allCases = new() { AudioClips.flip, AudioClips.move, AudioClips.success, AudioClips.tirt, AudioClips.cardShuffle, AudioClips.ding,AudioClips.goodMove, AudioClips.wrong , AudioClips.levelCompleted};

    private readonly AudioClips clip;
    private Sound(AudioClips clip) => this.clip = clip;
    public void Play() => AudioManager.Instance.Play(clip);
}