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
    [SerializeField] TextCollectionSO testDialogue;

    Queue<TextCollectionSO> dialogueQueue = new Queue<TextCollectionSO>();
    bool dialogueDisplaying;

    [SerializeField] TMP_InputField input;


    void Awake()
    {
        Instance = this;
    }

    void RefocusInput()
    {
        input.ActivateInputField();
    }

    void OnEnable()
    {
        input.onFocusSelectAll = false;
        input.onDeselect.AddListener((x) => RefocusInput());
        input.ActivateInputField();
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
            int length = dialogue.Text.Length;
            float progressedCharacters = 0f;
            int lastCharacters = 0;
            int characters = 0;

            while (true)
            {
                progressedCharacters += typeSpeed * Time.deltaTime;

                characters = Mathf.Min(length, progressedCharacters.RoundDown());
                if (characters != lastCharacters) SoundManager.Instance.PlaySoundAtPosition(typeCharacterSound);


                int newCharacters = characters - lastCharacters;
                //Debug.Log($"last chars: {lastCharacters}, new chars: {newCharacters}");
                int tagStartIndex = 0;
                bool inTag = false;
                for (int i = 0; i < newCharacters; i++)
                {
                    int currentIndex = lastCharacters + i;

                    if (!inTag && dialogue.Text[currentIndex] == '<')
                    {
                        tagStartIndex = i;
                        inTag = true;

                        int charactersLeft = length - currentIndex;
                        Debug.Log($"starting tag end check, length: {length}, characters left: {charactersLeft}, currentindex: {currentIndex}, tagstartindex: {tagStartIndex}");
                        for (int j = 0; j < charactersLeft; j++)
                        {
                            Debug.Log($"checking {dialogue.Text[currentIndex + j]}");
                            if (dialogue.Text[currentIndex + j] == '>')
                            {
                                int index = currentIndex + j; 
                                int tagLength = index - currentIndex + 1;
                                progressedCharacters += tagLength;
                                characters += tagLength;
                                inTag = false;
                                Debug.Log($"found end of tag at index {index} {tagLength} characters long");
                                break;
                            }
                        }
                    }
                } 


                dialogueText.text = $"{dialogue.Text.Substring(0, characters)}";

                lastCharacters = characters;
                if (characters >= length) break;
                yield return null;
            }
        }
    }
}
