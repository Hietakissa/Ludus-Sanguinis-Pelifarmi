using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using UnityEngine.Audio;
using HietakissaUtils;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] float typeSpeed;

    [SerializeField] SoundContainer typeCharacterSound;

    Queue<TextCollectionSO> dialogueQueue = new Queue<TextCollectionSO>();
    bool dialogueDisplaying;

    [SerializeField] TMP_InputField input;
    bool givingName;

    [SerializeField] TextCollectionSO preContractTutorialText;
    [SerializeField] TextCollectionSO postContractTutorialText;

    [SerializeField] TextCollectionSO dealerWinDialogue;
    [SerializeField] TextCollectionSO dealerLoseDialogue;
    [SerializeField] TextCollectionSO introDealerDialogue;
    [SerializeField] TextCollectionSO introDealerDialogue2;

    [SerializeField] Animator contractPaperAnimator;

    [SerializeField] GameStarter gameStarter;
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] Slider slider;

    [SerializeField] SoundContainer playerLoseSound;

    [SerializeField] CanvasGroup fadeToBlackPanel;

    [SerializeField] AudioMixer mixer;

    [SerializeField] GameObject descriptionPanel;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] ItemInfoSO scaleDescription;
    [SerializeField] ItemInfoSO mirrorDescription;
    [SerializeField] ItemInfoSO unoDescription;
    [SerializeField] ItemInfoSO couponDescription;
    [SerializeField] ItemInfoSO hookDescription;
    [SerializeField] ItemInfoSO heartDescription;
    [SerializeField] ItemInfoSO invalidDescription;

    void Awake()
    {
        Instance = this;

        mixer.GetFloat("MasterVolume", out float volume);
        slider.SetValueWithoutNotify(Maf.ReMap(-60, 10, 0, 1, volume));
    }

#if UNITY_EDITOR
    bool skipDialogue;
#endif
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.UpArrow) && dialogueDisplaying) skipDialogue = true;
#endif
        if (Input.GetKeyDown(KeyCode.Tab) && GameStarter.Instance.IsGameRunning)
        {
            if (GameManager.Instance.IsPaused)
            {
                GameManager.Instance.IsPaused = false;
                Time.timeScale = 1f;
                mainMenuUI.SetActive(false);
                Cursor.visible = false;
            }
            else 
            {
                GameManager.Instance.IsPaused = true;
                Time.timeScale = float.Epsilon;
                mainMenuUI.SetActive(true);
                Cursor.visible = true;
            }
        }
    }


    void OnEnable()
    {
        input.onFocusSelectAll = false;
        input.onDeselect.AddListener((x) => RefocusInput());
        input.onValueChanged.AddListener((x) => NameInputChanged());
        //input.onEndEdit.AddListener((x) => EndEdit());

        EventManager.OnBellRung += EndEdit;
        EventManager.OnEndGame += OnGameEnd;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= EndEdit;
        EventManager.OnEndGame -= OnGameEnd;
    }


    void RefocusInput()
    {
        if (givingName) input.ActivateInputField();
    }

    void EndEdit()
    {
        if (givingName && input.text.Length > 0)
        {
            givingName = false;
            contractPaperAnimator.Play("ContractExit");

            EventManager.SubmitPlayerName(input.text);
        }
    }

    void NameInputChanged()
    {
        SoundManager.Instance.PlaySoundAtPosition(typeCharacterSound);
    }

    public IEnumerator GiveNameSequenceCor()
    {
        givingName = true;
        input.ActivateInputField();
        contractPaperAnimator.Play("ContractEnter");

        while (givingName) yield return null;
    }

    public IEnumerator TutorialSequenceCor()
    {
        PlayDialogue(preContractTutorialText);
        while (dialogueDisplaying) yield return null;

        // Move contract, ask for name, continue
        yield return GiveNameSequenceCor();

        PlayDialogue(postContractTutorialText);
        while (dialogueDisplaying) yield return null;
    }

    public IEnumerator DealerWinSequenceCor()
    {
        //dealer win dialogue
        dialogueQueue.Clear();
        PlayDialogue(dealerWinDialogue);
        while (dialogueDisplaying) yield return null;

        //fade to black
        yield return FadeToBlackCor();

        //player lose sound
        SoundManager.Instance.PlaySound(playerLoseSound);
        yield return QOL.WaitForSeconds.Get(2f);

        //fade back in menu
        EventManager.EndGame();
        yield return FadeToNoneCor();
    }

    public IEnumerator PlayerWinSequenceCor()
    {
        //dealer lose dialogue
        //> (simultaenously)pot ding sound
        //> (simultaenously)throw chips
        GameManager.Instance.Pot.ThrowChips();
        dialogueQueue.Clear();
        PlayDialogue(dealerLoseDialogue);
        while (dialogueDisplaying || GameManager.Instance.Pot.IsThrowing) yield return null;

        //fade to black
        yield return FadeToBlackCor();

        //fade back in menu
        EventManager.EndGame();
        yield return QOL.WaitForSeconds.Get(2f);
        yield return FadeToNoneCor();
    }

    public IEnumerator FadeToBlackFastCor()
    {
        float time = 0f;
        while (true)
        {
            // 0.5 seconds to 1
            time += 2f * Time.deltaTime;
            fadeToBlackPanel.alpha = time;
            if (time >= 1f) break;
            else yield return null;
        }
    }

    IEnumerator FadeToBlackCor()
    {
        float time = 0f;
        while (true)
        {
            // 2 seconds to 1
            time += 0.5f * Time.deltaTime;
            fadeToBlackPanel.alpha = time;
            if (time >= 1f) break;
            else yield return null;
        }
    }

    public IEnumerator FadeToNoneCor()
    {
        float time = 1f;
        while (true)
        {
            // 2 seconds to 0
            time -= 0.5f * Time.deltaTime;
            fadeToBlackPanel.alpha = time;
            if (time <= 0f) break;
            else yield return null;
        }
    }


    public void PlayDialogue(TextCollectionSO dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
        if (!dialogueDisplaying) StartCoroutine(DisplayDialogue());


        IEnumerator DisplayDialogue()
        {
            dialogueDisplaying = true;
            dialogueUI.SetActive(dialogueDisplaying);

            Debug.Log($"Starting to display dialogue...");

            while (dialogueQueue.Count > 0)
            {
                TextCollectionSO dialogue = dialogueQueue.Dequeue();

                if (dialogue.Mode == TextCollectionMode.Sequential)
                {
                    for (int i = 0; i < dialogue.Dialogue.Length; i++)
                    {
                        yield return TypeDialogueElement(dialogue.Dialogue[i]);
                        yield return QOL.WaitForSeconds.Get(3);
                    }
                }
                else yield return TypeDialogueElement(dialogue.Dialogue.RandomElement());
            }

            dialogueDisplaying = false;
            dialogueUI.SetActive(dialogueDisplaying);
        }


        IEnumerator TypeDialogueElement(DialogueElement dialogue)
        {
            string text = dialogue.Text.Replace("NAME", GameManager.Instance.PlayerName);
            int length = text.Length;
            float progressedCharacters = 0f;
            int lastCharacters = 0;
            int soundIndex = 0;

            while (true)
            {
                progressedCharacters += typeSpeed * Time.deltaTime;
                int characters = Mathf.Min(length, progressedCharacters.RoundDown());
                int newCharacters = characters - lastCharacters;

                float wait = 0f;
                bool inTag = false;
                for (int i = 0; i < newCharacters; i++)
                {
                    int currentIndex = lastCharacters + i;
                    char currentChar = text[currentIndex];

                    // should make some dictionary of characters with their respective wait times instead, but this also works
                    if (currentChar == ',') wait = 0.25f;
                    else if (currentChar == '.') wait = 0.5f;
                    else if (currentChar == '?') wait = 0.7f;
                    else if (currentChar == '!') wait = 0.7f;
                    else if (!inTag && currentChar == '<')
                    {
                        inTag = true;

                        int charactersLeft = length - currentIndex;
                        //Debug.Log($"starting tag end check, length: {length}, characters left: {charactersLeft}, currentindex: {currentIndex}, tagstartindex: {tagStartIndex}");
                        for (int j = 0; j < charactersLeft; j++)
                        {
                            //Debug.Log($"checking {text[currentIndex + j]}");
                            if (text[currentIndex + j] == '>')
                            {
                                int index = currentIndex + j; 
                                int tagLength = index - currentIndex;
                                progressedCharacters += tagLength;
                                characters += tagLength;
                                inTag = false;
                                //Debug.Log($"found end of tag at index {index} {tagLength} characters long");
                                break;
                            }
                        }
                    }
                    else
                    {
                        soundIndex++;
                        if (soundIndex >= 2) // Every other character play a sound
                        {
                            SoundManager.Instance.PlaySoundAtPosition(typeCharacterSound);
                            soundIndex = 0;
                        }
                    }
                }

#if UNITY_EDITOR
                if (skipDialogue)
                {
                    dialogueText.text = text;
                    skipDialogue = false;
                    yield break;
                }
#endif

                dialogueText.text = $"{text.Substring(0, characters)}";
                if (wait != 0f)
                {
                    // Play sound if we are waiting, i.e. hit a punctuation
                    soundIndex = 0;
                    SoundManager.Instance.PlaySoundAtPosition(typeCharacterSound);
                    yield return QOL.WaitForSeconds.Get(wait);
                }

                lastCharacters = characters;
                if (characters >= length) break;
                yield return null;
            }
        }
    }


    void OnGameEnd()
    {
        mainMenuUI.SetActive(true);
    }

    public void PlayButtonPress()
    {
        settingsPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuUI.SetActive(false);
        GameManager.Instance.IsPaused = false;
        Cursor.visible = false;
        Time.timeScale = 1f;

        if (!GameStarter.Instance.IsGameRunning) gameStarter.StartIntroAnim();
    }

    public void Quit() => QOL.Quit();

    public void PlayIntroDialogue() => PlayDialogue(introDealerDialogue);
    public void PlayIntroDialogue2() => PlayDialogue(introDealerDialogue2);

    public void SetVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Maf.ReMap(0, 1, -60, 10, volume));
    }

    public void StartItemHover(Item item)
    {
        descriptionPanel.SetActive(true);

        ItemCollection collection = item.Owner == PlayerType.Player ? GameManager.Instance.Table.PlayerItemCollection : GameManager.Instance.Table.DealerItemCollection;
        int itemCount = collection.GetItemCountForItem(item);
        ItemInfoSO itemInfo = GetItemInfoForItem(item);

        descriptionText.text = $"<color=#3671fb>{itemInfo.Name}</color><color=#ffa724> ({itemCount})</color>\n {itemInfo.Description}";
    }

    public void EndItemHover()
    {
        descriptionPanel.SetActive(false);
    }

    ItemInfoSO GetItemInfoForItem(Item item)
    {
        return item.Type switch
        {
            ItemType.Scale => scaleDescription,
            ItemType.Mirror => mirrorDescription,
            ItemType.UnoCard => unoDescription,
            ItemType.Coupon => couponDescription,
            ItemType.Hook => hookDescription,
            ItemType.Heart => heartDescription,
            _ => invalidDescription
        };
    }
}
