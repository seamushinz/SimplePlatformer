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
    protected Sprite sprite { get; set; }
    protected String spriteAssetName = null;
    protected Point _hitboxOffset;
    private int _hitboxWidth;
    private int _hitboxHeight;
    
    public Rectangle Bounds => new((int)position.X + _hitboxOffset.X, (int)position.Y + _hitboxOffset.Y, _hitboxWidth, _hitboxHeight);
    
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        //do some default drawing of this sprite at position
        spriteBatch.Draw(sprite, new Vector2(position.X, position.Y));
    }
    
    public virtual void LoadContent(GraphicsDevice graphicsDevice)
    {
        AsepriteFile entityAsepriteFile;
        using Stream entityAsepriteStream = TitleContainer.OpenStream($"Assets/{spriteAssetName}.aseprite");
        {
            entityAsepriteFile = AsepriteFileLoader.FromStream(spriteAssetName, entityAsepriteStream);    
        }
        
        sprite = entityAsepriteFile.CreateSprite(graphicsDevice, 0, options: null);
        _hitboxWidth = (int)sprite.Width;
        _hitboxHeight = (int)sprite.Height;
    }
}