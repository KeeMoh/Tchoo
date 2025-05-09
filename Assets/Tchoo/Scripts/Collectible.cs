using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Material material;

    private void Start()
    {
        material = GetComponentInChildren<ParticleSystemRenderer>().material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            player.CollectFoolet(material.GetColor("_GlowColor"));
            Destroy(gameObject);
        }
    }

}
