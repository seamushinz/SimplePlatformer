using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimplePlatformer.Systems;

namespace SimplePlatformer.Entities;

//things that stop actors
public class Solid : Entity
{
    // TODO(LDtk 5): add a sprite-less constructor for level geometry, e.g.
    // Solid(Point position, int width, int height) — sets position and the hitbox
    // size directly, no spriteAssetName. LDtk-built solids are invisible collision
    // rectangles (the tile art is drawn by LevelManager's prerendered level), so
    // they never load a sprite. Two prerequisites in Entity.cs (flagged there too):
    //   - hitbox width/height are private and only set from the sprite in
    //     LoadContent — they need to be settable by subclasses (protected setter
    //     or constructor path).
    //   - Draw/LoadContent assume `sprite` exists — make them skip when there is
    //     no sprite (good excuse to nullable-annotate `sprite` while in there).
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

    public override void LoadContent(GraphicsDevice GraphicsDevice)
    {
        base.LoadContent(GraphicsDevice);
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
       base.Draw(spriteBatch);
    }
}
