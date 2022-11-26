using UnityEditor;

public static class DebugCheatTools
{
    [MenuItem("DebugToolsAndCheats/Debug Tools/Heal To Max")]
    public static void Invincibility()
    {
        PlayerPolishManager.OnHealToMax();
    }

    [MenuItem("DebugToolsAndCheats/Debug Tools/Kill Player")]
    public static void KillPlayer()
    {
        PlayerPolishManager.OnTakeDamage(1000);
    }
}
