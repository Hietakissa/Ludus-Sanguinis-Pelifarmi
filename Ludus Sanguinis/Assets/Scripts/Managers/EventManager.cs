using System;

public static class EventManager
{
    public static event Action OnBellRung;
    public static void RingBell()
    {
        OnBellRung?.Invoke();
    }


    public static event Action OnStartGame;
    public static void StartGame()
    {
        OnStartGame?.Invoke();
    }

    public static event Action OnEndGame;
    public static void EndGame()
    {
        OnEndGame?.Invoke();
    }

    public static event Action<int> OnPotOverflow;
    public static void PotOverflow(int times)
    {
        OnPotOverflow?.Invoke(times);
    }
}
