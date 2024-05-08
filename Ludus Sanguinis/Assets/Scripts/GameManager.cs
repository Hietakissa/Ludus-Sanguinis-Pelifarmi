using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Table table;


    void TryEndTurn()
    {
        Debug.Log($"Tried to end turn, would have been successful: {table.PlayerCards.GetCards().Length > 0}");
    }


    void OnEnable()
    {
        EventManager.OnBellRung += TryEndTurn;
    }

    void OnDisable()
    {
        EventManager.OnBellRung -= TryEndTurn;
    }
}
