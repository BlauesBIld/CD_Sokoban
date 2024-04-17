namespace libs;

public class Player : GameObject {

    private static Player _instance;

    public static Player Instance {
        get{
            if(_instance == null)
            {
                _instance = new Player();
            }
            return _instance;
        }
    }
    
    private Player () : base(){
        Type = GameObjectType.Player;
        CharRepresentation = '☻';
        Color = ConsoleColor.DarkYellow;
    }
    
    public static void ResetPlayer() {
        _instance = null;
    }
}