namespace libs;

public class BoxOnGoal : Obstacle {
    public BoxOnGoal () : base(){
        Type = GameObjectType.BoxOnGoal;
        CharRepresentation = '■';
        Color = ConsoleColor.Green;
    }
}