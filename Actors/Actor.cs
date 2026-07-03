using Microsoft.Xna.Framework;
using SimplePlatformer.Systems;

namespace SimplePlatformer.Entities;

//things that move and are stopped by solids
//from: https://maddymakesgames.com/articles/celeste_and_towerfall_physics/index.html
public abstract class Actor : Entity
{
    private Vector2 _remainder;
    
    /// <summary>
    /// Checks if Actor collides with a Solid at the shifted coordinates using Bounds
    /// </summary>
    /// <param name="dx">Shift in X</param>
    /// <param name="dy">Shift in Y</param>
    /// <returns>Whether a collision is found</returns>
    protected bool CollidesAt(int dx, int dy) => CollisionSystem.OverlapsSolid(new Rectangle(Bounds.X + dx, Bounds.Y + dy, Bounds.Width, Bounds.Height));
    
    /// <summary>
    /// Moves this entity on the X-axis by the specified amount, checking for collisions with solids.
    /// </summary>
    /// <param name="amount">amount to move, should be input as multiplied by deltaTime</param>
    protected virtual void MoveX(float amount, Action? onCollide = null)
    {
        _remainder.X += amount; //sub-pixel movement accumulates
        int move = (int)MathF.Round(_remainder.X);
        if (move == 0) return;
        _remainder.X -= move;
        int sign = Math.Sign(move);
        while (move != 0)
        {
            if (!CollidesAt(sign, 0))
            {
                //no solid immediately to the side
                position.X += sign; move -= sign;
            }
            else
            {
                if (onCollide != null) onCollide();
                break;
            }
        }
    }
    
    /// <summary>
    /// Moves this entity on the Y-axis by the specified amount, checking for collisions with solids.
    /// </summary>
    /// <param name="amount">amount to move, should be input as multiplied by deltaTime</param>
    protected virtual void MoveY(float amount, Action? onCollide = null)
    {
        _remainder.Y += amount;
        int move = (int)MathF.Round(_remainder.Y);
        if (move == 0) return;
        _remainder.Y -= move;
        int sign = Math.Sign(move);
        while (move != 0)
        {
            //shift bounds one piexsl in direction 'sign'
            if (!CollidesAt(0, sign))
            {
                position.Y += sign; move -= sign;
            }
            else
            {
                if (onCollide != null) onCollide();
                break;
            }
            
        }
    }
}