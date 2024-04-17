namespace libs;

public class BoxGoal : GameObject {
    public BoxGoal () : base(){
        Type = GameObjectType.BoxGoal;
        CharRepresentation = '☐';
        Color = ConsoleColor.DarkGreen;
    }
}