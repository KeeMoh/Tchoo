using Unity.Cinemachine;
using UnityEngine;

public class SwitchConfiner : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D confiner1;
    [SerializeField] private PolygonCollider2D confiner2;
    private Collider2D cinemachineConfiner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cinemachineConfiner = LevelManager.Instance.CineCamRef.GetComponent<CinemachineConfiner2D>().BoundingShape2D;

            if (cinemachineConfiner == confiner1)
            {
                cinemachineConfiner = confiner2;
            }
            else
            {
                cinemachineConfiner = confiner1;
            }
        }
}
}
