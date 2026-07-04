using Microsoft.Xna.Framework;
using SimplePlatformer.Core;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SimplePlatformer.Systems;

//support splitscreen in the future?
public class Camera
{
    private Vector2 position;
    private Matrix cameraMatrix;
    private float cameraSnappiness = 0.01f;
    private int deadzoneHalfWidth = 4;
    
    public void Update(float deltaTime, Point targetPixel, Vector2 targetExact)
    {
        Vector2 desiredPosition;
        desiredPosition.X = MathHelper.Clamp(position.X, targetExact.X - deadzoneHalfWidth,  targetExact.X + deadzoneHalfWidth);
        desiredPosition.Y = MathHelper.Clamp(position.Y, targetExact.Y - deadzoneHalfWidth,  targetExact.Y + deadzoneHalfWidth);
        
        float t = 1f - MathF.Exp(-cameraSnappiness * deltaTime);
        position = Vector2.Lerp(position, desiredPosition, t);
        
        // TODO(LDtk 12): clamp to level bounds (deferred from Milestone 6 until levels
        // exist). Using the level's pixel-bounds Rectangle exposed by LevelManager
        // (TODO(LDtk 4)), clamp the camera so the visible rect — renderPos ±
        // virtualWidth/2 and virtualHeight/2 — never leaves the level. Clamp the float
        // `position` too (not just renderPos), otherwise the smoothed position keeps
        // drifting past the edge and unsticks slowly on the way back.
        Vector2 offset = position - targetExact;
        Vector2 renderPos = new(
            targetPixel.X + MathF.Round(offset.X),
            targetPixel.Y + MathF.Round(offset.Y));

        cameraMatrix = Matrix.CreateTranslation(-renderPos.X, -renderPos.Y, 0)
                       * Matrix.CreateTranslation(GameSettings.virtualWidth / 2f, GameSettings.virtualHeight / 2f, 0);
    }

    public Matrix GetMatrix() => cameraMatrix;

    public void SnapTo(Vector2 _position)
    {
        position = _position;
    }
}