using System;

public static class EventManager
{
    public static event Action OnBellRung;
    public static void RingBell()
    {
        OnBellRung?.Invoke();
    }
}
