using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [System.Serializable]
    public class RoomType
    {
        public GameObject prefab;
        public int count;
        public bool isUnique;
        [Range(0f, 1f)] public float spawnChance = 1f;
    }

    public RoomType[] roomTypes;
    public GameObject bossRoomPrefab;
    public GameObject startRoomPrefab;
    public GameObject playerMarkerPrefab;
    private GameObject playerMarker;
    public float roomSize = 1f;
    public float playerMoveSpeed = 2f;
    public bool enableFogOfWar = true;

    private Dictionary<Vector2Int, GameObject> rooms = new Dictionary<Vector2Int, GameObject>();
    private List<Vector2Int> occupiedPositions = new List<Vector2Int>();
    private HashSet<Vector2Int> uniqueRoomPositions = new HashSet<Vector2Int>();
    private Vector2Int playerPosition;
    private System.Random random = new System.Random();

    void Awake()
    {
        GenerateRooms();
        SpawnPlayerMarker();
    }

    void GenerateRooms()
    {
        Vector2Int startPosition = Vector2Int.zero;
        playerPosition = startPosition;
        PlaceRoom(startRoomPrefab, startPosition);

        List<Vector2Int> normalRoomPositions = new List<Vector2Int>();

        foreach (RoomType type in roomTypes)
        {
            for (int i = 0; i < type.count; i++)
            {
                if (random.NextDouble() > type.spawnChance)
                    continue;

                Vector2Int nextPosition = type.isUnique ? GetValidUniqueRoomPosition() : GetRandomValidPosition();

                PlaceRoom(type.prefab, nextPosition);

                if (type.isUnique)
                    uniqueRoomPositions.Add(nextPosition);
                else
                    normalRoomPositions.Add(nextPosition);
            }
        }

        Vector2Int bossPosition = GetBossRoomPosition(normalRoomPositions);
        PlaceRoom(bossRoomPrefab, bossPosition);
    }

    Vector2Int GetRandomValidPosition()
    {
        Vector2Int position;
        int attempts = 0;
        do
        {
            position = occupiedPositions[random.Next(occupiedPositions.Count)] + GetRandomDirection();
            attempts++;
            if (attempts > 100)
            {
                Debug.LogWarning("Не удалось найти свободное место для комнаты.");
                break;
            }
        } while (occupiedPositions.Contains(position));
        return position;
    }

    Vector2Int GetValidUniqueRoomPosition()
    {
        Vector2Int position;
        int attempts = 0;
        do
        {
            position = GetRandomValidPosition();
            attempts++;
            if (attempts > 100)
            {
                Debug.LogWarning("Не удалось найти уникальное свободное место для комнаты.");
                break;
            }
        } while (uniqueRoomPositions.Contains(position));
        return position;
    }

    Vector2Int GetBossRoomPosition(List<Vector2Int> normalRooms)
    {
        Vector2Int bossPos;
        int attempts = 0;
        do
        {
            bossPos = normalRooms[random.Next(normalRooms.Count)] + GetRandomDirection();
            attempts++;
            if (attempts > 100)
            {
                Debug.LogWarning("Не удалось найти свободное место для комнаты босса.");
                break;
            }
        } while (occupiedPositions.Contains(bossPos));
        return bossPos;
    }

    Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return directions[random.Next(directions.Length)];
    }

    void PlaceRoom(GameObject prefab, Vector2Int position)
    {
        GameObject room = Instantiate(prefab, new Vector3(position.x * roomSize, position.y * roomSize, 0), Quaternion.identity);
        rooms[position] = room;
        occupiedPositions.Add(position);

        // Добавляем RoomClickable, если его нет
        RoomClickable clickable = room.GetComponent<RoomClickable>();
        if (clickable == null)
            clickable = room.AddComponent<RoomClickable>();

        clickable.Initialize(position, this);

        if (enableFogOfWar)
            room.SetActive(false);
    }


    void SpawnPlayerMarker()
    {
        if (playerMarker != null)
        {
            Debug.LogWarning("Попытка создать второй маркер!"); // Выведет сообщение в консоль
            return;
        }

        playerMarker = Instantiate(playerMarkerPrefab, new Vector3(playerPosition.x * roomSize, playerPosition.y * roomSize, 0), Quaternion.identity);
        RevealRoom(playerPosition);
        Debug.Log("Маркер игрока создан");
    }



    public void MovePlayer(Vector2Int newPosition)
    {
        if (rooms.ContainsKey(newPosition) && IsAdjacent(playerPosition, newPosition))
        {
            playerPosition = newPosition;
            StartCoroutine(SmoothMove(playerMarker.transform, newPosition));
            LogPlayerEntry();
            RevealRoom(newPosition);
        }
    }

    IEnumerator SmoothMove(Transform marker, Vector2Int targetPosition)
    {
        Vector3 start = marker.position;
        Vector3 end = new Vector3(targetPosition.x * roomSize, targetPosition.y * roomSize, 0);
        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * playerMoveSpeed;
            marker.position = Vector3.Lerp(start, end, elapsedTime);
            yield return null;
        }
        marker.position = end;
    }

    void RevealRoom(Vector2Int position)
    {
        if (enableFogOfWar)
        {
            if (rooms.ContainsKey(position))
                rooms[position].SetActive(true);

            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int adjacent = position + dir;
                if (rooms.ContainsKey(adjacent))
                    rooms[adjacent].SetActive(true);
            }
        }
    }

    bool IsAdjacent(Vector2Int from, Vector2Int to)
    {
        return (Mathf.Abs(from.x - to.x) == 1 && from.y == to.y) ||
               (Mathf.Abs(from.y - to.y) == 1 && from.x == to.x);
    }

    void LogPlayerEntry()
    {
        Debug.Log("Игрок попал в комнату");
    }
}
