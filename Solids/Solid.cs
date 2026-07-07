using Microsoft.Xna.Framework;

namespace SimplePlatformer.Entities;

public class Solid : Entity
{
    public Solid(Point position, int width, int height)
    {
        //spriteAssetName = "block";
        this.position = position;
        _hitboxWidth = width;
        _hitboxHeight = height;
    }
}
