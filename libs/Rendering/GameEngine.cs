using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;

namespace libs;

using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json;

public sealed class GameEngine
{
    private static GameEngine? _instance;
    private IGameObjectFactory gameObjectFactory;
    string currentLevelName = "Level 0 - Intro";
    private int currentLevel = 0;
    private int totalAmountOfLevels = FileHandler.CountLevelFiles();
    private int amountOfBoxesInCurrentLevel = 0;
    
    public HashSet<State> states = new HashSet<State>();

    public static GameEngine Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameEngine();
            }
            return _instance;
        }
    }

    private GameEngine()
    {
        //INIT PROPS HERE IF NEEDED
        gameObjectFactory = new GameObjectFactory();
    }

    private GameObject? _focusedObject;

    private Map map = new Map();

    private List<GameObject> gameObjects = new List<GameObject>();


    public Map GetMap()
    {
        return map;
    }

    public GameObject GetFocusedObject()
    {
        return _focusedObject;
    }

    public void Setup()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        dynamic gameData = FileHandler.ReadJson("level" + currentLevel + ".json");

        map.MapWidth = gameData.map.width;
        map.MapHeight = gameData.map.height;

        foreach (var gameObject in gameData.gameObjects)
        {
            GameObject newGameObject = CreateGameObject(gameObject);
            if (newGameObject is Box)
            {
                amountOfBoxesInCurrentLevel++;
            }
            
            AddGameObject(newGameObject);
        }

        _focusedObject = gameObjects.OfType<Player>().First();

    }

    public void Render()
    {
        Console.Clear();

        map.Initialize();

        PlaceGameObjects();

        for (int i = 0; i < map.MapHeight; i++)
        {
            for (int j = 0; j < map.MapWidth; j++)
            {
                DrawObject(map.Get(i, j));
            }
            int levelLine = map.MapHeight / 2;
            if (i == levelLine)
            {
                DrawLevelName();
                if (GameEngine.Instance.IsLevelCompleted())
                {
                    Console.Write(" - ✅ CLEAR");
                }
            } else if (i == levelLine + 1)
            {
                if(GameEngine.Instance.IsLevelCompleted())
                {
                    if (IsGameOver())
                    {
                        Console.Write("\t GAME OVER");
                    }
                    else
                    {
                        DrawPressEnterLine();
                    }
                }
            } else if(i == levelLine - 1)
            {
                Console.Write("\t [ESC] to quit");
            }
            Console.WriteLine();
        }
    }

    public GameObject CreateGameObject(dynamic obj)
    {
        return gameObjectFactory.CreateGameObject(obj);
    }

    public void AddGameObject(GameObject gameObject)
    {
        gameObjects.Add(gameObject);
    }

    private void PlaceGameObjects()
    {

        gameObjects.ForEach(delegate(GameObject obj)
        {
            if (obj is not Player)
            {
                map.Set(obj);
            }
        });
        map.Set(Player.Instance);
    }

    private void DrawObject(GameObject gameObject)
    {

        Console.ResetColor();

        if (gameObject != null)
        {
            Console.ForegroundColor = gameObject.Color;
            Console.Write(gameObject.CharRepresentation);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(' ');
        }
    }

    private void DrawLevelName()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("\t " + currentLevelName);
    }
    
    private void DrawPressEnterLine()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("\t Press Enter to continue to the next level");
    }
    
    public void RemoveFromBoxesInCurrentLevel()
    {
        amountOfBoxesInCurrentLevel--;
    }
    
    public void IncreaseLevel()
    {
        if (IsLevelCompleted() && !IsGameOver())
        {
            currentLevel++;
            currentLevelName = "Level " + currentLevel;
            states.Clear();
            gameObjects.Clear();
            amountOfBoxesInCurrentLevel = 0;
            Setup();
            
        }
    }
    
    public void SaveState()
    {
        State state = new State( Player.Instance.PosX, Player.Instance.PosY, gameObjects);
        states.Add(state);
    }
    
    public void UndoMove()
    {
        if (states.Count > 0 && !GameEngine.Instance.IsLevelCompleted())
        {
            gameObjects.Clear();
            amountOfBoxesInCurrentLevel = 0;
            State state = states.Last();
            states.Remove(state);
            foreach (var savedGameObject in state.gameObjects)
            {
                GameObject gameObject = gameObjectFactory.LoadGameObject(savedGameObject);
                if (gameObject is Box && gameObject.PosX != -1 && gameObject.PosY != -1)
                {
                    amountOfBoxesInCurrentLevel++;
                }
            
                AddGameObject(gameObject);
            }
            
            Player.Instance.PosX = state.playerPosX;
            Player.Instance.PosY = state.playerPosY;
            AddGameObject(Player.Instance);
            _focusedObject = Player.Instance;
        }
    }
    
    public bool IsGameOver()
    {
        return currentLevel >= totalAmountOfLevels-1 && IsLevelCompleted();
    }
    
    public bool IsLevelCompleted()
    {
        return amountOfBoxesInCurrentLevel == 0;
    }
}