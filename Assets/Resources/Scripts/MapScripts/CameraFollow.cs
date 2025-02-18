using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 5f; // Скорость следования камеры
    private Transform player; // Ссылка на игрока

    void Start()
    {
        FindPlayer();
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer(); // Попробовать снова найти игрока, если он пропал
            return;
        }

        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerMarker");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
}
