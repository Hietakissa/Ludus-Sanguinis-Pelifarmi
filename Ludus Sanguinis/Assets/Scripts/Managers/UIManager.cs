using System.Collections.Generic;
using HietakissaUtils.QOL;
using System.Collections;
using HietakissaUtils;
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

    [SerializeField] Animator contractPaperAnimator;

    [SerializeField] GameStarter gameStarter;
    [SerializeField] GameObject mainMenuUI;

    void Awake()
    {
        Instance = this;
    }

#if UNITY_EDITOR
    bool skipDialogue;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && dialogueDisplaying) skipDialogue = true;
    }
#endif

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
                        yield return QOL.GetWaitForSeconds(3);
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
                    yield return QOL.GetWaitForSeconds(wait);
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
        mainMenuUI.SetActive(false);
        gameStarter.StartIntroAnim();
    }
}
