using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace SimplePlatformer.Entities;

public abstract class Entity
{
    public Point position;
    protected Sprite? sprite { get; set; }
    protected String spriteAssetName = null;
    protected Point _hitboxOffset;
    protected int _hitboxWidth;
    protected int _hitboxHeight;
    
    public Rectangle Bounds => new((int)position.X + _hitboxOffset.X, (int)position.Y + _hitboxOffset.Y, _hitboxWidth, _hitboxHeight);
    
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        //do some default drawing of this sprite at position
        if (sprite == null) return;
        spriteBatch.Draw(sprite, new Vector2(position.X, position.Y));
    }
    
    public virtual void LoadContent(GraphicsDevice graphicsDevice)
    {
        if (spriteAssetName == null) return;
        AsepriteFile entityAsepriteFile;
        using Stream entityAsepriteStream = TitleContainer.OpenStream($"Assets/{spriteAssetName}.aseprite");
        {
            entityAsepriteFile = AsepriteFileLoader.FromStream(spriteAssetName, entityAsepriteStream);    
        }
        
        sprite = entityAsepriteFile.CreateSprite(graphicsDevice, 0, options: null);

        // Default the hitbox to the sprite size, but don't stomp a hitbox
        // that was set explicitly (e.g. Solid gets its size from the LDtk
        // IntGrid tile size, which shouldn't depend on the debug sprite).
        if (_hitboxWidth == 0) _hitboxWidth = (int)sprite.Width;
        if (_hitboxHeight == 0) _hitboxHeight = (int)sprite.Height;
    }
}