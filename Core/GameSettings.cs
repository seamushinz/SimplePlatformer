namespace SimplePlatformer.Core;

public static class GameSettings
{
    public const int virtualWidth = 400;
    public const int virtualHeight = 300;
    public static int renderScale = 4;
    public static float gravity = 0.0015f;
}

public static class GlobalFunctions
{
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }
}