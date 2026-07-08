using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimplePlatformer.Core;
using SimplePlatformer.Managers;
using MonoGame.Aseprite;

namespace SimplePlatformer.Entities;

public class Player : Actor
{
    private float maxSpeed = 0.2f;
    private float maxFallSpeed = 1f;
    private float acceleration = 0.005f;
    private Vector2 velocity;
    private bool onGround = false;
    private bool wasOnGround = false;
    private Vector2 _scale;
    private const float jumpBufferTime = 150f; //time in ms 
    private float jumpBufferTimer = -1f;
    private float jumpSpeed = 0.5f;
    private bool inAirFromJump = false;
    private bool jumpedThisFrame = false;
    
    public Player()
    {
        spriteAssetName = "player";
        _scale = new Vector2(1f, 1f);
    }

    private void doJump()
    {
        velocity.Y = -jumpSpeed;
        _scale = new Vector2(0.5f, 1.5f);
        jumpBufferTimer = -1;
        inAirFromJump = true;
        jumpedThisFrame = true;
    }
    
    public override void LoadContent(GraphicsDevice graphicsDevice)
    {
        base.LoadContent(graphicsDevice);
        sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
    }
    public void Update(float deltaTime)
    {
        //inputs
        if (InputManager.jumpOrSelectCondition.Pressed())
        {
            jumpBufferTimer = jumpBufferTime;
        }
        if (onGround && jumpBufferTimer >= 0)
        {
            doJump();
        }
        jumpBufferTimer -= deltaTime;

        //variable jump height
        if ((!InputManager.jumpOrSelectCondition.Held()) && inAirFromJump && !onGround && velocity.Y < 0)
        {
            velocity.Y = 0;
        }
        
        if (InputManager.moveDownCondition.Held() && !onGround && !InputManager.moveUpCondition.Held())
        {
            velocity.Y += acceleration * deltaTime;
            velocity.Y = Math.Min(maxFallSpeed, velocity.Y);
        }

        if (InputManager.moveLeftCondition.Held() && !InputManager.moveRightCondition.Held())
        {
            velocity.X -= acceleration * deltaTime;
            velocity.X = Math.Max(-maxSpeed, velocity.X);
        }

        if (InputManager.moveRightCondition.Held() && !InputManager.moveLeftCondition.Held())
        {
            velocity.X += acceleration * deltaTime;
            velocity.X = Math.Min(maxSpeed, velocity.X);
        }

        if ((!InputManager.moveRightCondition.Held() && !InputManager.moveLeftCondition.Held()) || (InputManager.moveRightCondition.Held() && InputManager.moveLeftCondition.Held()))
        {
            velocity.X = GlobalFunctions.MoveTowards(velocity.X, 0, acceleration * deltaTime);
        }
        
        //gravity
        if (!onGround)
        {
            velocity.Y += GameSettings.gravity * deltaTime;
            velocity.Y = Math.Min(maxFallSpeed, velocity.Y);
        }else if (!wasOnGround)
        {
            //landed
            _scale = new Vector2(1.5f, 0.5f);
            if (!jumpedThisFrame){inAirFromJump = false;}
        }
        
        //apply gravity and movement/collisions
        MoveX(velocity.X * deltaTime, () => { velocity.X = 0;});
        MoveY(velocity.Y * deltaTime, () => { velocity.Y = 0;});
        
        float t = 1f - MathF.Exp(-0.015f * deltaTime);
        _scale = Vector2.Lerp(_scale, Vector2.One, t);
        
        wasOnGround = onGround;
        onGround = CollidesAt(0, 1);
        jumpedThisFrame = false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        sprite.Scale = _scale;
        Vector2 feetPosition = new Vector2(
            position.X + sprite.Width / 2f,
            position.Y + sprite.Height);
        spriteBatch.Draw(sprite, feetPosition);
    }
}