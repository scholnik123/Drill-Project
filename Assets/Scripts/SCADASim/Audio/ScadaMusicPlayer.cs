using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SCADASim.Audio
{
    public sealed class ScadaMusicPlayer : MonoBehaviour
    {
        private readonly List<string> trackPaths = new List<string>();
        private AudioSource audioSource;
        private int currentTrackIndex;
        private bool isLoading;

        public int TrackCount => trackPaths.Count;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public float Volume => audioSource != null ? audioSource.volume : 0f;

        public string CurrentTrackName
        {
            get
            {
                if (trackPaths.Count == 0)
                {
                    return "треки не найдены";
                }

                return Path.GetFileNameWithoutExtension(trackPaths[currentTrackIndex]);
            }
        }

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.32f;
            DontDestroyOnLoad(gameObject);
            LoadTrackList();
        }

        private void Start()
        {
            if (trackPaths.Count > 0)
            {
                PlayTrack(0);
            }
        }

        private void Update()
        {
            if (trackPaths.Count == 0 || isLoading || audioSource == null || audioSource.clip == null)
            {
                return;
            }

            if (!audioSource.isPlaying && audioSource.time <= 0.02f)
            {
                return;
            }

            if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.1f)
            {
                NextTrack();
            }
        }

        public void TogglePlayback()
        {
            if (trackPaths.Count == 0 || audioSource == null)
            {
                return;
            }

            if (audioSource.clip == null)
            {
                PlayTrack(currentTrackIndex);
                return;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.UnPause();
            }
        }

        public void NextTrack()
        {
            if (trackPaths.Count == 0)
            {
                return;
            }

            PlayTrack((currentTrackIndex + 1) % trackPaths.Count);
        }

        public void PreviousTrack()
        {
            if (trackPaths.Count == 0)
            {
                return;
            }

            PlayTrack((currentTrackIndex - 1 + trackPaths.Count) % trackPaths.Count);
        }

        public void SetVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
            }
        }

        private void LoadTrackList()
        {
            trackPaths.Clear();
            string musicPath = Path.Combine(Application.streamingAssetsPath, "Music");
            if (!Directory.Exists(musicPath))
            {
                Debug.Log("SCADA music folder not found in StreamingAssets/Music.");
                return;
            }

            string[] supportedExtensions = { ".mp3", ".wav", ".ogg", ".aif", ".aiff" };
            string[] files = Directory.GetFiles(musicPath, "*.*", SearchOption.AllDirectories);
            System.Array.Sort(files, System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < files.Length; i++)
            {
                string extension = Path.GetExtension(files[i]).ToLowerInvariant();
                for (int j = 0; j < supportedExtensions.Length; j++)
                {
                    if (extension == supportedExtensions[j])
                    {
                        trackPaths.Add(files[i]);
                        break;
                    }
                }
            }

            Debug.Log($"SCADA music tracks loaded: {trackPaths.Count}");
        }

        private void PlayTrack(int index)
        {
            if (trackPaths.Count == 0 || isLoading)
            {
                return;
            }

            currentTrackIndex = Mathf.Clamp(index, 0, trackPaths.Count - 1);
            StartCoroutine(LoadAndPlay(trackPaths[currentTrackIndex]));
        }

        private IEnumerator LoadAndPlay(string path)
        {
            isLoading = true;
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(ToFileUrl(path), ResolveAudioType(path)))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to load SCADA music track: {path}. {request.error}");
                    isLoading = false;
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null && audioSource != null)
                {
                    clip.name = Path.GetFileNameWithoutExtension(path);
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }

            isLoading = false;
        }

        private static string ToFileUrl(string path)
        {
            return new System.Uri(path).AbsoluteUri;
        }

        private static AudioType ResolveAudioType(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            switch (extension)
            {
                case ".wav":
                    return AudioType.WAV;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aif":
                case ".aiff":
                    return AudioType.AIFF;
                default:
                    return AudioType.MPEG;
            }
        }
    }
}
