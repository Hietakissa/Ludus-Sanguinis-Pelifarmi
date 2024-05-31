using UnityEngine.Events;
using UnityEngine;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] Environment[] environments;

    void OnPlayerDamaged(Player player, int health)
    {
        for (int i = 0; i < environments.Length; i++)
        {
            environments[i].UpdateState(player, health);
        }
    }

    void OnEnable() => EventManager.OnPlayerDamaged += OnPlayerDamaged;
    void OnDisable() => EventManager.OnPlayerDamaged -= OnPlayerDamaged;
}

[System.Serializable]
class Environment
{
    [SerializeField] int health = 3;
    [HorizontalGroup(2)]
    [SerializeField] MatchType matchType;
    [SerializeField, HideInInspector] PlayerType playerType = PlayerType.None;
    [SerializeField] GameObject[] environmentObjects;
    [SerializeField] UnityEvent OnActivate;
    [SerializeField] UnityEvent OnDeactivate;


    public void UpdateState(Player player, int newHealth)
    {
        if ((player != null && player.GetPlayerType() != playerType) || (player == null && playerType != PlayerType.None)) return;

        bool match = IsMatch();
        for (int i = 0; i < environmentObjects.Length; i++)
        {
            environmentObjects[i].SetActive(match);
        }

        if (match) OnActivate?.Invoke();
        else OnDeactivate?.Invoke();



        bool IsMatch()
        {
            if (matchType == MatchType.Equals) return newHealth == health;
            else if (matchType == MatchType.GreaterThan) return newHealth > health;
            else if (matchType == MatchType.LessThan) return newHealth < health;
            else return false;
        }
    }
}

enum MatchType
{
    Equals,
    GreaterThan,
    LessThan
}