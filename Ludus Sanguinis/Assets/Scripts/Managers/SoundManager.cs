using HietakissaUtils;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    AudioSource[] audioSources = new AudioSource[20];
    int sourceIndex = 0;

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = new GameObject($"Pooled Audio Source [{i}]", typeof(AudioSource)).GetComponent<AudioSource>();
            audioSources[i].transform.parent = transform;
        }
    }

    public void PlaySoundAtPosition(HKSoundContainer sound, Vector3 position)
    {
        AudioSource source = audioSources[sourceIndex];
        source.transform.position = position;
        sound.ApplyToAudioSource(source);
        source.Play();

        sourceIndex++;
        sourceIndex %= audioSources.Length;
    }
}
