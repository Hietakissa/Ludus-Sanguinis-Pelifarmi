using HietakissaUtils;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    const int CONST_AUDIOSOURCE_COUNT = 20;
    AudioSource[] audioSources = new AudioSource[CONST_AUDIOSOURCE_COUNT];
    int sourceIndex = 0;

    [Header("Card Sounds")]
    [SerializeField] SoundContainer bellRingSound;
    [SerializeField] SoundContainer hoverCardSound;
    [SerializeField] SoundContainer playCardSound;
    [SerializeField] SoundContainer dealCardSound;

    [Header("Item Sounds")]
    [SerializeField] SoundContainer scaleHoverSound;
    [SerializeField] SoundContainer mirrorHoverSound;
    [SerializeField] SoundContainer unoHoverSound;
    [SerializeField] SoundContainer couponHoverSound;
    [SerializeField] SoundContainer hookHoverSound;
    [SerializeField] SoundContainer heartHoverSound;
    [SerializeField] SoundContainer itemUseSound;

    [Header("Player Sounds")]
    [SerializeField] SoundContainer stepSound;
    [SerializeField] SoundContainer playerLoseLifeSound;
    [SerializeField] SoundContainer dealerLoseLifeSound;

    [SerializeField] AudioClip clip;


    void Awake()
    {
        Instance = this;

        for (int i = 0; i < CONST_AUDIOSOURCE_COUNT; i++)
        {
            audioSources[i] = new GameObject($"Pooled Audio Source [{i}]", typeof(AudioSource)).GetComponent<AudioSource>();
            audioSources[i].transform.parent = transform;
        }
    }


    public void PlaySound(SoundContainer sound) => PlaySoundAtPosition(sound);
    public void PlaySoundAtPosition(SoundContainer sound, Vector3 position = default)
    {
        if (sound == null)
        {
            Debug.Log($"play null sound");
            return;
        }
        Debug.Log($"play sound");

        AudioSource source = audioSources[sourceIndex];
        source.transform.position = position;
        Debug.Log($"getting index");
        int index = sound.GetSoundIndex();
        Debug.Log($"getting clip");
        SoundClip clip = sound.Sounds[index];
        Debug.Log($"applying to source");
        sound.ApplyClipToAudioSource(source, clip);
        Debug.Log($"playing with volume {source.volume}");
        source.Play();

        //foreach (SoundContainer nextSound in clip.Next)
        //{
        //    PlaySound(nextSound);
        //    Debug.Log($"playing next sound");
        //}

        sourceIndex++;
        sourceIndex %= audioSources.Length;
    }


    void OnRingBell() => PlaySoundAtPosition(bellRingSound);
    void OnHoverCard() => PlaySoundAtPosition(hoverCardSound);
    void OnPlayCard() => PlaySoundAtPosition(playCardSound);
    void OnDealCard() => PlaySoundAtPosition(dealCardSound);

    void HoverItem(Item item)
    {
        switch (item.Type)
        {
            case ItemType.Scale: PlaySoundAtPosition(scaleHoverSound); break;
            case ItemType.Mirror: PlaySoundAtPosition(mirrorHoverSound); break;
            case ItemType.UnoCard: PlaySoundAtPosition(unoHoverSound); break;
            case ItemType.Coupon: PlaySoundAtPosition(couponHoverSound); break;
            case ItemType.Hook: PlaySoundAtPosition(hookHoverSound); break;
            case ItemType.Heart: PlaySoundAtPosition(heartHoverSound); break;
        }
    }
    void UseItem(ItemType itemType) => PlaySound(itemUseSound);

    public void Footstep() => PlaySound(stepSound);

    void OnPlayerDamaged(Player player, int health, bool initializing)
    {
        if (player == null || initializing) return;

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
        EventManager.OnUseItem += UseItem;

        EventManager.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= OnRingBell;

        EventManager.OnHoverCard -= OnHoverCard;
        EventManager.OnPlayCard -= OnPlayCard;
        EventManager.OnDealCard -= OnDealCard;
        
        EventManager.OnHoverItem -= HoverItem;
        EventManager.OnUseItem -= UseItem;

        EventManager.OnPlayerDamaged -= OnPlayerDamaged;
    }
}
