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


    public void UpdateState(Player player, int health)
    {
        if ((player != null && player.GetPlayerType() != playerType) || (player == null && playerType != PlayerType.None)) return;

        for (int i = 0; i < environmentObjects.Length; i++)
        {
            if (matchType == MatchType.Equals) environmentObjects[i].SetActive(this.health == health);
            else if (matchType == MatchType.GreaterThan) environmentObjects[i].SetActive(this.health < health);
            else if (matchType == MatchType.LessThan) environmentObjects[i].SetActive(this.health > health);
        }
    }
}

enum MatchType
{
    Equals,
    GreaterThan,
    LessThan
}