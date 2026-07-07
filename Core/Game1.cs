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
using SimplePlatformer.Managers;
using SimplePlatformer.Systems;

namespace SimplePlatformer;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private RenderTarget2D _virtualRenderTarget;
    private Rectangle _screenScaleRectangle;
    private Player _player;
    private Camera _camera;
    
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

        // Integer scale only: every virtual pixel maps to an exact NxN block of
        // screen pixels, so nothing can shimmer as the world scrolls.
        int scaleX = windowWidth / GameSettings.virtualWidth;
        int scaleY = windowHeight / GameSettings.virtualHeight;
        int scale = Math.Max(1, Math.Min(scaleX, scaleY));

        int finalWidth = GameSettings.virtualWidth * scale;
        int finalHeight = GameSettings.virtualHeight * scale;
        int posX = (windowWidth - finalWidth) / 2;
        int posY = (windowHeight - finalHeight) / 2;

        _screenScaleRectangle = new Rectangle(posX, posY, finalWidth, finalHeight);
    }
    protected override void Initialize()
    {
        _virtualRenderTarget = new RenderTarget2D(GraphicsDevice, GameSettings.virtualWidth, GameSettings.virtualHeight);
        UpdateScreenScale();
        _player = new Player();
        _camera = new Camera();
        // solids will be created and positioned in LoadContent where sprite sizes are available
        base.Initialize();
    }

    protected override void LoadContent()
    {
        InputHelper.Setup(this);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LevelManager.LoadContent(_spriteBatch); // needs SpriteBatch to bake tilesets
        CollisionSystem.LoadContent(GraphicsDevice);
        _player.LoadContent(GraphicsDevice);
        _player.position = LevelManager.PlayerSpawnPoint;
        _camera.SnapTo(_player.position.ToVector2());
    }

    protected override void Update(GameTime gameTime)
    {
        InputHelper.UpdateSetup();
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _player.Update(deltaTime);
        _camera.Update(deltaTime, _player.position, _player.ExactPosition);
        base.Update(gameTime);
        InputHelper.UpdateCleanup();
    }

    protected override void Draw(GameTime gameTime)
    {
        // PASS 1: Render the game world to the virtual texture
        GraphicsDevice.SetRenderTarget(_virtualRenderTarget);
        GraphicsDevice.Clear(LevelManager.BackgroundColor); // authored per-level in LDtk
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetMatrix());
        LevelManager.Draw(_spriteBatch); // level tiles first = behind actors
        _player.Draw(_spriteBatch);
        // TODO: once LevelManager.Draw's tile loop is filled in and tiles carry
        // the visuals, drop the "block" sprite from Solid and delete this line
        // (and CollisionSystem.Draw/LoadContent — solids become pure hitboxes).
        CollisionSystem.Draw(_spriteBatch);
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