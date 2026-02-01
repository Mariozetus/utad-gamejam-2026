using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
        [Header("Pitch Variation")]
        public bool randomizePitch = false;
        [Range(0f, 1f)] public float pitchVariation = 0.1f;
        [HideInInspector] public AudioSource source;
    }

    #region Fields

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource musicIntroSource;
    [SerializeField] private AudioSource musicLoopSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int poolSize = 10;

    [Header("Sound Library")]
    [SerializeField] private SoundLibrary[] soundLibraries;
    [SerializeField] private Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary;
    private List<AudioSource> audioSourcePool;
    private Queue<AudioSource> availableSources;

    private Sound _currentMusicSound;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundManager()
    {
        soundDictionary = new Dictionary<string, Sound>(StringComparer.Ordinal);

        LoadSoundLibraries();

        audioSourcePool = new List<AudioSource>(Mathf.Max(1, poolSize));
        availableSources = new Queue<AudioSource>(Mathf.Max(1, poolSize));

        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.playOnAwake = false;
            audioSourcePool.Add(src);
            availableSources.Enqueue(src);
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        if (musicIntroSource == null)
        {
            musicIntroSource = gameObject.AddComponent<AudioSource>();
            musicIntroSource.playOnAwake = false;
            musicIntroSource.loop = false;
        }

        if (musicLoopSource == null)
        {
            musicLoopSource = gameObject.AddComponent<AudioSource>();
            musicLoopSource.playOnAwake = false;
            musicLoopSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        musicSource.outputAudioMixerGroup = musicMixerGroup;
        musicIntroSource.outputAudioMixerGroup = musicMixerGroup;
        musicLoopSource.outputAudioMixerGroup = musicMixerGroup;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    private void LoadSoundLibraries()
    {
        if (soundLibraries != null)
        {
            foreach (var library in soundLibraries)
            {
                if (library == null || library.Sounds == null) continue;

                foreach (var libSound in library.Sounds)
                {
                    if (libSound == null) continue;
                    if (string.IsNullOrWhiteSpace(libSound.name)) continue;

                    // Convertir SoundLibrary.Sound a SoundManager.Sound
                    var sound = new Sound
                    {
                        name = libSound.name,
                        clip = libSound.clip,
                        volume = libSound.volume,
                        pitch = libSound.pitch,
                        loop = libSound.loop,
                        randomizePitch = libSound.randomizePitch,
                        pitchVariation = libSound.pitchVariation
                    };

                    if (!soundDictionary.ContainsKey(sound.name))
                        soundDictionary.Add(sound.name, sound);
                    else
                        Debug.LogWarning($"[SoundManager] Sonido duplicado encontrado: '{sound.name}' - usando el primero");
                }
            }
        }

        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s == null) continue;
                if (string.IsNullOrWhiteSpace(s.name)) continue;
                if (!soundDictionary.ContainsKey(s.name))
                    soundDictionary.Add(s.name, s);
            }
        }
    }

    #endregion

    #region Playing Methods

    /// <summary>
    /// Reproduce un sonido por su nombre. Si el sonido está configurado para repetirse, se reproducirá en bucle.
    /// Si no, se reproducirá como un sonido único.
    /// </summary>
    /// <param name="soundName"></param>
    public void PlaySound(string soundName)
    {
            if (!soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                Debug.LogWarning("Sound " + soundName + " not found!");
                return;
            }

            if (sound.loop) PlayLoopingSound(soundName);
            else PlayOneShotSound(soundName);
    }

    /// <summary>
    /// Reproduce un sonido por su nombre una única vez independientemente de si está configurado para repetirse o no.
    /// </summary>
    /// <param name="soundName"></param>
    public void PlayOneShotSound(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Logger.Warning("Sound " + soundName + " not found!", LogType.Audio, this);
            return;
        }

        if (sound.clip == null) return;

        if (sfxSource != null)
        {
            sfxSource.pitch = GetPitch(sound);
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
    }

    /// <summary>
    /// Reproduce un sonido en bucle por su nombre indepedientemente de si está configurado para repetirse o no.
    /// </summary>
    /// <param name="soundName"></param>
    public void PlayLoopingSound(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Logger.Warning("Sound " + soundName + " not found!", LogType.Audio, this);
            return;
        }

        if (sound.clip == null || sound.source != null && sound.source.isPlaying) return;

        AudioSource source = GetAvailableSource();
        if (source == null)
        {
            Logger.Warning("No available audio sources in pool!", LogType.Audio, this);
            return;
        }

        sound.source = source;

        source.clip = sound.clip;
        source.loop = true;
        source.playOnAwake = false;

        source.pitch = GetPitch(sound);
        source.volume = sound.volume;

        source.Play();
    }
    
    /// <summary>
    /// Reproduce un sonido en una posición específica del mundo.
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="position"></param>
    public void PlaySoundAtPoint(string soundName, Vector3 position)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Logger.Warning("Sound " + soundName + " not found!", LogType.Audio, this);
            return;
        }

        if (sound.clip == null) return;

        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
    }

    /// <summary>
    /// Reproduce música de fondo por su nombre. Puede desvanecerse al iniciarse.
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="fadeIn"></param>
    /// <param name="fadeDuration"></param>
    public void PlayMusic(string soundName, bool fadeIn = false, float fadeDuration = 1f)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Logger.Warning("Music " + soundName + " not found!", LogType.Audio, this);
            return;
        }

        if (sound.clip == null) return;

        if (fadeIn)
        {
            StopAllCoroutines();
            StartCoroutine(FadeToMusic(sound, fadeDuration));
            return;
        }

        _currentMusicSound = sound;

        musicSource.clip = sound.clip;
        musicSource.loop = true;

        musicSource.pitch = GetPitch(sound);
        musicSource.volume = sound.volume;

        musicSource.Play();
    }

    #endregion

    #region Stopping Methods

    /// <summary>
    /// Detiene la reproducción de un sonido por su nombre.
    /// </summary>
    /// <param name="soundName"></param>
    public void StopSound(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
            return;
        }

        if (sound.source != null)
        {
            sound.source.Stop();
            ReturnSourceToPool(sound.source);
            sound.source = null;
        }
    }

    /// <summary>
    /// Detiene la reproducción de todos los sonidos y música.
    /// </summary>
    public void StopAllSounds()
    {
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s == null) continue;
                if (s.source != null)
                {
                    s.source.Stop();
                    ReturnSourceToPool(s.source);
                    s.source = null;
                }
            }
        }

        if (sfxSource != null) sfxSource.Stop();

        RebuildAvailableQueue();
    }

    /// <summary>
    /// Detiene la reproducción de la música de fondo. Puede desvanecerse al detenerse.
    /// </summary>
    /// <param name="fadeOut"></param>
    /// <param name="fadeDuration"></param>
    public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
    {
        if (!musicSource) return;

        if (!fadeOut)
        {
            musicSource.Stop();
            _currentMusicSound = null;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(FadeOutMusic(fadeDuration));
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Establece el volumen maestro.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMasterVolume(float volume)
    {
        SetMixerVolume("MasterVol", volume);
    }

    /// <summary>
    /// Establece el volumen de la música.
    /// </summary>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        SetMixerVolume("MusicVol", volume);
    }

    /// <summary>
    /// Establece el volumen de los efectos de sonido.
    /// </summary>
    /// <param name="volume"></param>
    public void SetSFXVolume(float volume)
    {
        SetMixerVolume("SFXVol", volume);
    }

    private void SetMixerVolume(string parameterName, float sliderVolume)
    {
        float dB = (sliderVolume <= 0f) ? -80f :  Mathf.Log10(sliderVolume) * 20f;
        mainMixer.SetFloat(parameterName, dB);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calcula el pitch de un sonido, aplicando variaciones si es necesario.
    /// </summary>
    /// <param name="sound"></param>
    private float GetPitch(Sound sound)
    {
        float p = sound.pitch;
    
        if (sound.randomizePitch)
            p += UnityEngine.Random.Range(-sound.pitchVariation, sound.pitchVariation);
    
        return Mathf.Clamp(p, 0.1f, 3f);
    }

    /// <summary>
    /// Obtiene una fuente de audio disponible del pool.
    /// </summary>
    private AudioSource GetAvailableSource()
    {
        int guard = availableSources.Count;
        while (guard-- > 0 && availableSources.Count > 0)
        {
            var src = availableSources.Dequeue();
            if (src != null && !src.isPlaying)
                return src;

            if (src != null)
                availableSources.Enqueue(src);
        }

        foreach (var src in audioSourcePool)
        {
            if (src != null && !src.isPlaying)
                return src;
        }

        return null;
    }

    /// <summary>
    /// Devuelve una fuente de audio al pool.
    /// </summary>
    /// <param name="source"></param>
    private void ReturnSourceToPool(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        source.clip = null;
        source.loop = false;
        source.pitch = 1f;
        source.volume = 1f;

        if (!availableSources.Contains(source))
            availableSources.Enqueue(source);
    }

    /// <summary>
    /// Reconstruye la cola de fuentes de audio disponibles.
    /// </summary>
    private void RebuildAvailableQueue()
    {
        availableSources.Clear();
        foreach (var src in audioSourcePool)
        {
            if (src != null && !src.isPlaying)
                availableSources.Enqueue(src);
        }
    }

    /// <summary>
    /// Desvanece la música actual y reproduce una nueva música con desvanecimiento
    /// </summary>
    /// <param name="newSound"></param>
    /// <param name="duration"></param>
    private IEnumerator FadeToMusic(Sound newSound, float duration)
    {
        if (!musicSource) yield break;

        float startVolume = musicSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        _currentMusicSound = newSound;
        musicSource.clip = newSound.clip;
        musicSource.volume = 0f;
        musicSource.pitch = GetPitch(newSound);
        musicSource.loop = true;
        musicSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, newSound.volume, t / duration);
            yield return null;
        }
        musicSource.volume = newSound.volume;
    }

    /// <summary>
    /// Desvanece la música actual y la detiene.
    /// </summary>
    /// <param name="duration"></param>
    private IEnumerator FadeOutMusic(float duration)
    {
        if (!musicSource) yield break;

        float startVolume = musicSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
    
        musicSource.volume = 0f;
        musicSource.Stop();
        _currentMusicSound = null;
    }

    /// <summary>
    /// Verifica si un sonido específico está reproduciéndose.
    /// </summary>
    /// <param name="soundName"></param>
    public bool IsSoundPlaying(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound))
            return false;

        return sound.source != null && sound.source.isPlaying;
    }

    /// <summary>
    /// Verifica si la música de fondo está reproduciéndose.
    /// </summary>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    #endregion

    #region MusicTrack System (Intro + Loop)

    private MusicTrack _currentMusicTrack;
    private bool _isPlayingIntro = false;
    private Coroutine _musicTrackCoroutine;

    /// <summary>
    /// Plays a music track with intro and loop system
    /// </summary>
    /// <param name="musicTrack">The MusicTrack to play</param>
    public void PlayMusicTrack(MusicTrack musicTrack)
    {
        if (musicTrack == null || !musicTrack.IsValid)
        {
            Logger.Warning("Attempted to play invalid music track", LogType.Audio, this);
            return;
        }

        Logger.Log($"Playing music track: {musicTrack.trackName}", LogType.Audio, this);
        
        // Stop current music
        StopMusicTrack(false);
        
        _currentMusicTrack = musicTrack;

        if (musicTrack.HasIntro)
        {
            // Play intro first, then loop
            _musicTrackCoroutine = StartCoroutine(PlayIntroThenLoop(musicTrack));
        }
        else if (musicTrack.HasLoop)
        {
            // No intro, just play loop
            PlayLoopOnly(musicTrack);
        }
    }

    private IEnumerator PlayIntroThenLoop(MusicTrack track)
    {
        _isPlayingIntro = true;
        
        // Setup and play intro
        musicIntroSource.clip = track.introClip;
        musicIntroSource.volume = 0f;
        musicIntroSource.Play();
        
        Logger.Log($"Playing intro for '{track.trackName}' ({track.IntroDuration:F1}s)", LogType.Audio, this);
        
        // Fade in intro
        yield return StartCoroutine(FadeAudioSource(musicIntroSource, track.volume, track.fadeInDuration));
        
        // Wait for intro to finish (minus a small buffer for seamless transition)
        float waitTime = track.IntroDuration - track.fadeInDuration - 0.1f;
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        
        // Prepare loop if available
        if (track.HasLoop)
        {
            musicLoopSource.clip = track.loopClip;
            musicLoopSource.volume = track.volume;
            musicLoopSource.Play();
            
            Logger.Log($"Starting loop for '{track.trackName}'", LogType.Audio, this);
        }
        
        // Wait for intro to completely finish
        yield return new WaitForSeconds(0.1f);
        
        // Stop intro
        musicIntroSource.Stop();
        _isPlayingIntro = false;
        
        Logger.Log($"Intro finished, loop active for '{track.trackName}'", LogType.Audio, this);
    }

    private void PlayLoopOnly(MusicTrack track)
    {
        musicLoopSource.clip = track.loopClip;
        musicLoopSource.volume = 0f;
        musicLoopSource.Play();
        
        Logger.Log($"Playing loop only for '{track.trackName}'", LogType.Audio, this);
        
        // Fade in loop
        StartCoroutine(FadeAudioSource(musicLoopSource, track.volume, track.fadeInDuration));
    }

    public void StopMusicTrack(bool fadeOut = true)
    {
        if (_currentMusicTrack == null) return;
        
        Logger.Log($"Stopping music track: {_currentMusicTrack.trackName}", LogType.Audio, this);
        
        if (_musicTrackCoroutine != null)
        {
            StopCoroutine(_musicTrackCoroutine);
            _musicTrackCoroutine = null;
        }

        if (fadeOut)
        {
            StartCoroutine(FadeOutMusicTrack(_currentMusicTrack.fadeOutDuration));
        }
        else
        {
            musicIntroSource.Stop();
            musicLoopSource.Stop();
            _currentMusicTrack = null;
            _isPlayingIntro = false;
        }
    }

    private IEnumerator FadeOutMusicTrack(float fadeTime)
    {
        yield return StartCoroutine(FadeAudioSource(musicIntroSource, 0f, fadeTime));
        yield return StartCoroutine(FadeAudioSource(musicLoopSource, 0f, fadeTime));
        
        musicIntroSource.Stop();
        musicLoopSource.Stop();
        _currentMusicTrack = null;
        _isPlayingIntro = false;
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null || duration <= 0f)
        {
            if (source != null) source.volume = targetVolume;
            yield break;
        }

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;
    }

    public bool IsMusicTrackPlaying => _currentMusicTrack != null && (musicIntroSource.isPlaying || musicLoopSource.isPlaying);
    public string CurrentMusicTrackName => _currentMusicTrack?.trackName ?? "None";
    public bool IsPlayingMusicIntro => _isPlayingIntro;

    #endregion
}