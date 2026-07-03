using SimplePlatformer.Core;
using SimplePlatformer.Managers;

namespace SimplePlatformer.Entities;

public class Player : Actor
{
    private float maxSpeed = 0.5f;
    private float acceleration = 0.01f;

    public Player()
    {
        spriteAssetName = "player";
    }
    public override void Update(float deltaTime)
    {
        //inputs
        if (InputManager.jumpOrSelectCondition.Pressed())
        {
            // Handle jump or select action
        }
        
        //inputs should zero out if opposite directions held
        
        if (InputManager.moveUpCondition.Held() && !InputManager.moveDownCondition.Held())
        { 
            velocity.Y -= acceleration * deltaTime;
            velocity.Y = Math.Max(-maxSpeed, velocity.Y);
        }

        if (InputManager.moveDownCondition.Held() && !InputManager.moveUpCondition.Held())
        {
            velocity.Y += acceleration * deltaTime;
            velocity.Y = Math.Min(maxSpeed, velocity.Y);
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
            velocity.X = GlobalFunctions.MoveTowards(velocity.X, 0, (acceleration) * deltaTime);
        }
        if ((!InputManager.moveUpCondition.Held() && !InputManager.moveDownCondition.Held()) || (InputManager.moveUpCondition.Held() && InputManager.moveDownCondition.Held()))
        {
            velocity.Y = GlobalFunctions.MoveTowards(velocity.Y, 0, (acceleration) * deltaTime);
        }

        Console.Out.WriteLine($"Player : {velocity.X}, {velocity.Y}");

        //apply gravity and movement/collisions
        base.Update(deltaTime);
    }
    
    
}