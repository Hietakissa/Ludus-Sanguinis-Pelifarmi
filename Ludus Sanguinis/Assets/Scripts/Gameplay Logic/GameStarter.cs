using UnityEngine.Playables;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public static GameStarter Instance;

    [SerializeField] bool startOnInput;
    [SerializeField] PlayableDirector director;
    public bool IsGameRunning { get; private set; }

    [SerializeField] GameObject blinkText;


    void Awake()
    {
        Instance = this;

        director.time = 0;
        director.Evaluate();
    }

    void Update()
    {
        if (startOnInput && !IsGameRunning && Input.anyKeyDown)
        {
            StartIntroAnim();
        }

        //if (Input.GetKeyDown(KeyCode.G)) EventManager.EndGame();
    }

    public void StartIntroAnim()
    {
        IsGameRunning = true;
        director.Play();
        //blinkText.SetActive(false);
    }

    public void StartGame()
    {
        EventManager.StartGame();
    }

    void OnEndGame()
    {
        director.time = 0;
        director.Evaluate();
        IsGameRunning = false;
        //blinkText.SetActive(true);
    }


    void OnEnable()
    {
        EventManager.OnEndGame += OnEndGame;
    }

    void OnDisable()
    {
        EventManager.OnEndGame -= OnEndGame;
    }
}
