namespace libs;

public class GameObjectFactory : IGameObjectFactory
{
    public GameObject CreateGameObject(dynamic obj) {

        GameObject newObj = new GameObject();
        int type = obj.Type;

        switch (type)
        {
            case (int) GameObjectType.Player:
                Player.Instance.PosX = obj.PosX;
                Player.Instance.PosY = obj.PosY;
                newObj = Player.Instance;
                break;
            case (int) GameObjectType.Obstacle:
                newObj = obj.ToObject<Obstacle>();
                break;
            case (int) GameObjectType.Box:
                newObj = obj.ToObject<Box>();
                break;
        }

        return newObj;
    }
}