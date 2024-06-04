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

    [SerializeField] SoundContainer hoverItemSound;

    [SerializeField] SoundContainer playerLoseLifeSound;
    [SerializeField] SoundContainer dealerLoseLifeSound;

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = new GameObject($"Pooled Audio Source [{i}]", typeof(AudioSource)).GetComponent<AudioSource>();
            audioSources[i].transform.parent = transform;
        }
    }

    public void PlaySoundAtPosition(SoundContainer sound, Vector3 position = default)
    {
        if (sound == null) return;

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

    void HoverItem() => PlaySoundAtPosition(hoverItemSound);

    void OnPlayerDamaged(Player player, int health)
    {
        if (player == null) return;

        if (player.IsDealer) PlaySoundAtPosition(dealerLoseLifeSound);
        else PlaySoundAtPosition(playerLoseLifeSound);
    }


    void OnEnable()
    {
        EventManager.OnBellRung += OnRingBell;

        EventManager.OnHoverCard += OnHoverCard;
        EventManager.OnPlayCard += OnPlayCard;
        EventManager.OnDealCard += OnDealCard;

        EventManager.OnHoverItem += HoverItem;

        EventManager.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= OnRingBell;

        EventManager.OnHoverCard -= OnHoverCard;
        EventManager.OnPlayCard -= OnPlayCard;
        EventManager.OnDealCard -= OnDealCard;
        
        EventManager.OnHoverItem -= HoverItem;

        EventManager.OnPlayerDamaged -= OnPlayerDamaged;
    }
}
