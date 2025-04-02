using UnityEngine;

public class Collectible : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger");
        if (collision.TryGetComponent(out PlayerController player))
        {
            player.CollectFoolet();
            Destroy(gameObject);
        }
    }

}
