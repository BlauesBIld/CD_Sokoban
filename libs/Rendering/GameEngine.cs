using System;
using System.Diagnostics;
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
    string currentSubtitle = "";
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

        dynamic gameData = FileHandler.ReadJson("/levels/level" + currentLevel + ".json");

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
            int middleLine = map.MapHeight / 2;
            WriteLevelInformation(i, middleLine);
            WriteKeyBinds(i, middleLine);
            WriteSaveSlots(i, middleLine);
            Console.WriteLine();
        }
    }

    private void WriteSaveSlots(int currentLine, int middleLine)
    {
        int basePadding = 10;
        
        string output = currentLine switch
        {
            var x when x == middleLine - 2 => GetInformationStringAboutSaveSlot(1),
            var x when x == middleLine - 1 => GetInformationStringAboutSaveSlot(2),
            var x when x == middleLine => GetInformationStringAboutSaveSlot(3),
            var x when x == middleLine + 1 => GetInformationStringAboutSaveSlot(4),
            var x when x == middleLine + 2 => GetInformationStringAboutSaveSlot(5),
            _ => ""
        };
        Console.ForegroundColor = ConsoleColor.Yellow;
        string padding = new string(' ', basePadding);
        output = padding + output;
        Console.Write(output);
    }
    
    private string GetInformationStringAboutSaveSlot(int slotNumber)
    {
        if (FileHandler.SaveExists(slotNumber))
        {
            Save save = FileHandler.GetSave(slotNumber);
            return "Save slot " + slotNumber + " - Level " + save.CurrentLevel + " - " + save.SaveTimeStamp;
        }
        return "Save slot " + slotNumber + " - Empty";
    }
    
    private void WriteKeyBinds(int currentLine, int middleLine)
    {
        int basePadding = 80;

        string output = currentLine switch
        {
            var x when x == middleLine - 2 => "[ESC] to quit".PadLeft(basePadding),
            var x when x == middleLine - 1 => "[WASD]/[↑←↓→] to move".PadLeft(basePadding),
            var x when x == middleLine => "[R] to undo".PadLeft(basePadding - currentLevelName.Length - 6),
            var x when x == middleLine + 1 => "[NUMBER] to save to slot".PadLeft(basePadding - currentSubtitle.Length - 6),
            var x when x == middleLine + 2 => "[SHIFT + NUMBER] to load from slot".PadLeft(basePadding),
            _ => ""
        };
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(output);
    }
    
    private void WriteLevelInformation(int currentLine, int levelLine)
    {
        if (currentLine == levelLine)
        {
            if (IsLevelCompleted())
            {
                currentLevelName = "Level " + currentLevel + " - ✅ CLEAR";
            }
            DrawLevelName();
        } else if (currentLine == levelLine + 1)
        {
            if(GameEngine.Instance.IsLevelCompleted())
            {
                if (IsGameOver())
                {
                    currentSubtitle = "GAME OVER";
                }
                else
                {
                    currentSubtitle = "Press [ENTER] to continue";
                }
            }
            else
            {
                currentSubtitle = "Boxes left: " + amountOfBoxesInCurrentLevel;
            }
            DrawSubtitleLine();
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
        string padding = new string(' ', 6);
        string output = padding + currentLevelName;
        Console.Write(output);
    }
    
    private void DrawSubtitleLine()
    {
        Console.ForegroundColor = ConsoleColor.White;
        string padding = new string(' ', 6);
        string output = padding + currentSubtitle;
        Console.Write(output);
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
    
    public void SaveState(State stateToSave)
    {
        states.Add(stateToSave);
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
    
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public void SaveGame(int saveSlot)
    {
        Save save = new Save(GetCurrentState(), currentLevel, map.MapHeight, map.MapWidth, DateTime.Now);
        FileHandler.SaveGame(save, saveSlot);
    }
    
    public State GetCurrentState()
    {
        return new State(Player.Instance.PosX, Player.Instance.PosY, gameObjects);
    }
    
    public void LoadGame(int saveSlot)
    {
        if (FileHandler.SaveExists(saveSlot))
        {
            Save save = FileHandler.GetSave(saveSlot);
            currentLevel = save.CurrentLevel;
            currentLevelName = "Level " + currentLevel;
            states.Clear();
            gameObjects.Clear();
            amountOfBoxesInCurrentLevel = 0;
            foreach (dynamic savedGameObject in save.MapState.gameObjects)
            {
                GameObject gameObject = gameObjectFactory.CreateGameObject(savedGameObject);
                if (gameObject is Box && gameObject.PosX != -1 && gameObject.PosY != -1)
                {
                    amountOfBoxesInCurrentLevel++;
                }
            
                AddGameObject(gameObject);
            }
            
            Player.Instance.PosX = save.MapState.playerPosX;
            Player.Instance.PosY = save.MapState.playerPosY;
            AddGameObject(Player.Instance);
            _focusedObject = Player.Instance;
            map.MapHeight = save.MapHeight;
            map.MapWidth = save.MapWidth;
            SaveState(GetCurrentState());
        }
    }
}