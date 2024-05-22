using HietakissaUtils;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    AudioSource[] audioSources = new AudioSource[20];
    int sourceIndex = 0;

    [SerializeField] SoundContainer bellRingSound;
    [SerializeField] SoundContainer hoverCardSound;
    [SerializeField] SoundContainer playCardSound;
    [SerializeField] SoundContainer dealCardSound;

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = new GameObject($"Pooled Audio Source [{i}]", typeof(AudioSource)).GetComponent<AudioSource>();
            audioSources[i].transform.parent = transform;
        }
    }

    public void PlaySoundAtPosition(SoundContainer sound, Vector3 position = new Vector3())
    {
        AudioSource source = audioSources[sourceIndex];
        source.transform.position = position;
        sound.ApplyToAudioSource(source);
        source.Play();

        sourceIndex++;
        sourceIndex %= audioSources.Length;
    }


    void OnRingBell() => PlaySoundAtPosition(bellRingSound);
    void OnHoverCard() => PlaySoundAtPosition(hoverCardSound);
    void OnPlayCard() => PlaySoundAtPosition(playCardSound);
    void OnDealCard() => PlaySoundAtPosition(dealCardSound);



    void OnEnable()
    {
        EventManager.OnBellRung += OnRingBell;

        EventManager.OnHoverCard += OnHoverCard;
        EventManager.OnPlayCard += OnPlayCard;
        EventManager.OnDealCard += OnDealCard;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= OnRingBell;

        EventManager.OnHoverCard -= OnHoverCard;
        EventManager.OnPlayCard -= OnPlayCard;
        EventManager.OnDealCard -= OnDealCard;
    }
}
