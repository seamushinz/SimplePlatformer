using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using Track = Apos.Input.Track;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using MonoGame.Aseprite;
using SimplePlatformer.Core;
using SimplePlatformer.Entities;
using System.Collections.Generic;

namespace SimplePlatformer;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private RenderTarget2D _virtualRenderTarget;
    private Rectangle _screenScaleRectangle;
    private Player _player;
    private List<Solid> _solids = new();
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false; 
        _graphics.PreferredBackBufferWidth = GameSettings.virtualWidth*GameSettings.renderScale;
        _graphics.PreferredBackBufferHeight = GameSettings.virtualHeight*GameSettings.renderScale;
        _graphics.ApplyChanges();
        
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChanged;
    }
    private void OnWindowSizeChanged(object sender, EventArgs e)
    {
        UpdateScreenScale();
    }
    
    private void UpdateScreenScale()
    {
        int windowWidth = GraphicsDevice.Viewport.Width;
        int windowHeight = GraphicsDevice.Viewport.Height;

        float scaleX = (float)windowWidth / GameSettings.virtualWidth;
        float scaleY = (float)windowHeight / GameSettings.virtualHeight;
        float scale = Math.Min(scaleX, scaleY);

        int finalWidth = (int)(GameSettings.virtualWidth * scale);
        int finalHeight = (int)(GameSettings.virtualHeight * scale);
        int posX = (windowWidth - finalWidth) / 2;
        int posY = (windowHeight - finalHeight) / 2;

        _screenScaleRectangle = new Rectangle(posX, posY, finalWidth, finalHeight);
    }
    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _virtualRenderTarget = new RenderTarget2D(GraphicsDevice, GameSettings.virtualWidth, GameSettings.virtualHeight);
        UpdateScreenScale();
        _player = new Player();
        // solids will be created and positioned in LoadContent where sprite sizes are available
        base.Initialize();
    }

    protected override void LoadContent()
    {
        InputHelper.Setup(this);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here. load all the entity and stuff sprites here programatically somehow?
        _player.LoadContent(GraphicsDevice);

        // Create a horizontal row of solids at the bottom of the virtual screen.
        // We create the first solid, load its sprite to determine tile size, then create the rest in a loop.
        _solids = new List<Solid>();

        // Create first solid at x=0; we'll adjust its Y after we know its height
        var first = new Solid(new Point(0, 0));
        first.LoadContent(GraphicsDevice);
        int tileWidth = first.Bounds.Width;
        int tileHeight = first.Bounds.Height;
        
        var anotherOne = new Solid(new Point(GameSettings.virtualWidth-tileHeight*2, GameSettings.virtualHeight-tileHeight*2));
        anotherOne.LoadContent(GraphicsDevice);
        _solids.Add(anotherOne);

        // How many tiles needed to span the virtual width (ceiling)
        int count = (GameSettings.virtualWidth + tileWidth - 1) / tileWidth;

        // position solids side-by-side along bottom
        for (int i = 0; i < count; i++)
        {
            int x = i * tileWidth;
            int y = GameSettings.virtualHeight - tileHeight;
            // first already created for i==0
            if (i == 0)
            {
                first.position = new Point(x, y);
                _solids.Add(first);
                continue;
            }

            var s = new Solid(new Point(x, y));
            s.LoadContent(GraphicsDevice);
            _solids.Add(s);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        InputHelper.UpdateSetup();
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _player.Update(deltaTime);
        base.Update(gameTime);
        InputHelper.UpdateCleanup();
    }

    protected override void Draw(GameTime gameTime)
    {
        // PASS 1: Render the game world to the virtual texture
        GraphicsDevice.SetRenderTarget(_virtualRenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _player.Draw(_spriteBatch);
        // draw all solids
        foreach (var s in _solids)
        {
            s.Draw(_spriteBatch);
        }
        _spriteBatch.End();
        
        // PASS 2: Render that texture stretched onto the physical OS window
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_virtualRenderTarget, _screenScaleRectangle, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}