using Microsoft.Xna.Framework;
using SimplePlatformer.Core;
using SimplePlatformer.Managers;

namespace SimplePlatformer.Entities;

public class Player : Actor
{
    private float maxSpeed = 0.3f;
    private float maxFallSpeed = 1f;
    private float acceleration = 0.005f;
    private Vector2 velocity;
    private bool onGround = true; 

    public Player()
    {
        spriteAssetName = "player";
    }
    public void Update(float deltaTime)
    {
        //inputs
        if (InputManager.jumpOrSelectCondition.Pressed() && onGround)
        {
            // Handle jump or select action
            velocity.Y -= acceleration*100;
        }
        //variable jump height
        if (InputManager.jumpOrSelectCondition.Released() && !onGround && velocity.Y < 0)
        {
            // Handle jump or select action
            velocity.Y = 0;
        }
        
        //inputs should zero out if opposite directions held
        if (InputManager.moveDownCondition.Held() && !InputManager.moveUpCondition.Held())
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
        }
        
        //apply gravity and movement/collisions
        MoveX(velocity.X * deltaTime, () => { velocity.X = 0;});
        MoveY(velocity.Y * deltaTime, () => { velocity.Y = 0;});
        
        onGround = CollidesAt(0, 1);
    }
}