using UnityEngine.Playables;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    bool gameRunning;

    [SerializeField] GameObject blinkText;


    void Awake()
    {
        director.time = 0;
        director.Evaluate();
    }

    void Update()
    {
        if (!gameRunning && Input.anyKeyDown)
        {
            gameRunning = true;
            director.Play();
            blinkText.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.G)) EventManager.EndGame();
    }

    public void StartGame()
    {
        EventManager.StartGame();
    }

    void OnEndGame()
    {
        director.time = 0;
        director.Evaluate();
        gameRunning = false;
        blinkText.SetActive(true);
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
