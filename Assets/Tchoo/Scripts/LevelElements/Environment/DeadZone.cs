using UnityEngine;

public class DeadZone : MonoBehaviour
{
    PlayerController player;
    [SerializeField] private Transform[] RespawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out player))
        {
            player.transform.position = FindClosestPosition(player.transform.position);
            player.TakeDamage(1, true);
        }
    }

    private Vector3 FindClosestPosition(Vector3 pos)
    {
        Transform outT = RespawnPoints[0];
        for (int i = 1; i < RespawnPoints.Length; i++)
        {
            if (Vector3.Distance(pos, outT.position) > Vector3.Distance(pos, RespawnPoints[i].position))
                outT = RespawnPoints[i];
        }
        return outT.position;
    }
}
