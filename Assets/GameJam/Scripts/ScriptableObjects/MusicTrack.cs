using System;
using UnityEngine;

/// <summary>
/// Represents a music track with separate intro and loop parts
/// </summary>
[Serializable]
public class MusicTrack
{
    [Header("Track Info")]
    public string trackName = "New Track";
    
    [Header("Audio Clips")]
    [Tooltip("Audio clip that plays once at the beginning")]
    public AudioClip introClip;
    
    [Tooltip("Audio clip that loops continuously after intro")]
    public AudioClip loopClip;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume for this track")]
    public float volume = 1f;
    
    [Tooltip("Fade in duration when starting")]
    public float fadeInDuration = 1f;
    
    [Tooltip("Fade out duration when stopping")]
    public float fadeOutDuration = 1f;

    public bool HasIntro => introClip != null;
    public bool HasLoop => loopClip != null;
    public bool IsValid => HasIntro || HasLoop;
    
    public float IntroDuration => HasIntro ? introClip.length : 0f;
    public float LoopDuration => HasLoop ? loopClip.length : 0f;
}