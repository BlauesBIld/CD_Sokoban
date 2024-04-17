using System;

namespace libs;

public sealed class InputHandler{

    private static InputHandler? _instance;
    private GameEngine engine;

    public static InputHandler Instance {
        get{
            if(_instance == null)
            {
                _instance = new InputHandler();
            }
            return _instance;
        }
    }

    private InputHandler() {
        //INIT PROPS HERE IF NEEDED
        engine = GameEngine.Instance;
    }

    public void Handle(ConsoleKeyInfo keyInfo)
    {
        GameObject focusedObject = engine.GetFocusedObject();

        if (focusedObject != null) {
            // Handle keyboard input to move the player
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow: 
                case ConsoleKey.W:
                    focusedObject.Move(0, -1);
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    focusedObject.Move(0, 1);
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    focusedObject.Move(-1, 0);
                    break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    focusedObject.Move(1, 0);
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.Enter:
                    GameEngine.Instance.IncreaseLevel();
                    break;
                case ConsoleKey.R:
                    GameEngine.Instance.UndoMove();
                    break;
                default:
                    break;
            }
        }
    }
}