namespace SimplePlatformer.Core;

public class GameSettings
{
    public const int VirtualWidth = 400;
    public const int VirtualHeight = 300;
}

public class GlobalFunctions
{
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }
}