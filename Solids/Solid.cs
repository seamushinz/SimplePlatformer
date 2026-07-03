using Microsoft.Xna.Framework;
using SimplePlatformer.Systems;

namespace SimplePlatformer.Entities;

//things that stop actors
public class Solid : Entity
{
    public Solid()
    {
        spriteAssetName = "block";
        CollisionSystem.Add(this);
    }
    
    public Solid(Point _position)
    {
        spriteAssetName = "block";
        position.X = _position.X;
        position.Y = _position.Y;
        CollisionSystem.Add(this);
    }
}
