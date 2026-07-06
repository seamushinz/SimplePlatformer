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
        LevelManager.LoadContent();
        CollisionSystem.LoadContent(GraphicsDevice);
        _player.LoadContent(GraphicsDevice);
        _player.position = LevelManager.playerSpawnPoint;
        _camera.SnapTo(_player.position.ToVector2());
        
        
        // TODO(LDtk 10): replace everything below (the hand-placed test solids) with
        // the level pipeline: add a LevelManager field, have it load the .ldtk file
        // and build collision/visuals here (TODO(LDtk 4-9)), then position _player at
        // the PlayerSpawn from TODO(LDtk 9) — and SnapTo the camera *after* that, not
        // before. The _solids list and the block-row loop go away entirely: collision
        // rectangles live in CollisionSystem, tile art in the prerendered level.
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
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetMatrix());
        // TODO(LDtk 11): draw the level first, behind the player:
        // _levelManager.Draw(_spriteBatch) (from TODO(LDtk 8)). It must sit inside
        // this Begin/End so the camera matrix + PointClamp apply to the tiles. The
        // foreach over _solids below disappears with TODO(LDtk 10) — solids are
        // invisible once tiles carry the visuals. Optional: clear pass 1 to
        // level.BgColor instead of CornflowerBlue.
        _player.Draw(_spriteBatch);
        // draw all solids
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