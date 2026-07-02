using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace SimplePlatformer.Entities;

public abstract class Entity
{
    public Vector2 position;
    public Vector2 velocity;
    protected Sprite sprite { get; set; }

    public virtual void Update(float deltaTime)
    {
        position += velocity;
        // gravity? maybe add as optional/editable member somehow?
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        //do some default drawing of this sprite at position
        spriteBatch.Draw(sprite, new Vector2(position.X, position.Y));
    }
    
    public virtual void LoadContent(String assetName, GraphicsDevice graphicsDevice)
    {
        AsepriteFile playerAsepriteFile;
        using Stream playerAsepriteStream = TitleContainer.OpenStream($"Assets/{assetName}.aseprite");
        {
            playerAsepriteFile = AsepriteFileLoader.FromStream(assetName, playerAsepriteStream);    
        }
        
        sprite = playerAsepriteFile.CreateSprite(graphicsDevice, 0, options: null);
    }
}