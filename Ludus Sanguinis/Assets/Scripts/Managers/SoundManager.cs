using HietakissaUtils;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    AudioSource[] audioSources = new AudioSource[20];
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


    void Awake()
    {
        Instance = this;

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = new GameObject($"Pooled Audio Source [{i}]", typeof(AudioSource)).GetComponent<AudioSource>();
            audioSources[i].transform.parent = transform;
        }
    }

    public void PlaySound(SoundContainer sound) => PlaySoundAtPosition(sound);
    public void PlaySoundAtPosition(SoundContainer sound, Vector3 position = default)
    {
        if (sound == null) return;

        AudioSource source = audioSources[sourceIndex];
        source.transform.position = position;
        SoundClip clip = sound.Sounds[sound.GetSoundIndex()];
        sound.ApplyClipToAudioSource(source, clip);
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
    void UseItem(Item item) => PlaySound(itemUseSound);

    public void Footstep() => PlaySound(stepSound);

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
