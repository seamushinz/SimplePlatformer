using SimplePlatformer.Core;
using SimplePlatformer.Managers;

namespace SimplePlatformer.Entities;

public class Player : Actor
{
    private float speed = 260.0f;

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
        
        if (InputManager.moveUpCondition.Held())
        { 
            velocity.Y += -speed * deltaTime;
        }

        if (InputManager.moveDownCondition.Held())
        {
            velocity.Y += speed * deltaTime;
        }

        if (InputManager.moveLeftCondition.Held())
        {
            velocity.X += -speed * deltaTime;
        }

        if (InputManager.moveRightCondition.Held())
        {
            velocity.X += speed * deltaTime;
        }
        velocity.X = GlobalFunctions.MoveTowards(velocity.X, 0, speed/2 * deltaTime);
        velocity.Y = GlobalFunctions.MoveTowards(velocity.Y, 0, speed/2 * deltaTime);

        //apply gravity and movement/collisions
        base.Update(deltaTime);
    }
    
    
}