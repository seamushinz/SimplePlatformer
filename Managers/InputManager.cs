using Apos.Input;
using Microsoft.Xna.Framework.Input;

namespace SimplePlatformer.Managers;

public static class InputManager
{
    //Inputs
    public static ICondition menuButtonCondition = new AnyCondition(
        new KeyboardCondition(Keys.Escape),
        new GamePadCondition(GamePadButton.Start, 0)
    );
    
    public static ICondition jumpOrSelectCondition = new AnyCondition(
        new KeyboardCondition(Keys.Space),
        new KeyboardCondition(Keys.V),
        new GamePadCondition(GamePadButton.A, 0)
    );
    
    public static ICondition moveRightCondition = new AnyCondition(
        new KeyboardCondition(Keys.Right),
        new KeyboardCondition(Keys.D),
        new GamePadCondition(GamePadButton.Right, 0)
    );

    public static ICondition moveLeftCondition = new AnyCondition(
        new KeyboardCondition(Keys.Left),
        new KeyboardCondition(Keys.A),
        new GamePadCondition(GamePadButton.Left, 0)
    );
    
    public static ICondition moveUpCondition = new AnyCondition(
        new KeyboardCondition(Keys.Up),
        new KeyboardCondition(Keys.W),
        new GamePadCondition(GamePadButton.Up, 0)
    );
    
    public static ICondition moveDownCondition = new AnyCondition(
        new KeyboardCondition(Keys.Down),
        new KeyboardCondition(Keys.S),
        new GamePadCondition(GamePadButton.Down, 0)
    );
}