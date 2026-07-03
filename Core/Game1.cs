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

namespace SimplePlatformer;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    private RenderTarget2D _virtualRenderTarget;
    private Rectangle _screenScaleRectangle;
    private Player _player;
    private Solid _solid;
    
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
        _solid = new Solid(new Point(100, 100)); //example solid
        base.Initialize();
    }

    protected override void LoadContent()
    {
        InputHelper.Setup(this);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here. load all the entity and stuff sprites here programatically somehow?
        _player.LoadContent(GraphicsDevice);
        _solid.LoadContent(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        InputHelper.UpdateSetup();
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
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
        _solid.Draw(_spriteBatch);
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