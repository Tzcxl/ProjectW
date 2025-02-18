using UnityEngine;

public class RoomClickable : MonoBehaviour
{
    private Vector2Int position;
    private MapLoader mapLoader;

    public void Initialize(Vector2Int pos, MapLoader loader)
    {
        position = pos;
        mapLoader = loader;
    }

    void OnMouseDown()
    {
        if (mapLoader != null)
        {
            mapLoader.MovePlayer(position);
        }
    }
}
