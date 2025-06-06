using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Color materialColor;
    private Color baseColor;
    [SerializeField] private Power power;

    private void Start()
    {
        materialColor = GetComponentInChildren<ParticleSystemRenderer>().material.GetColor("_GlowColor");
        baseColor = GetComponentInChildren<ParticleSystem>().startColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            player.CollectFoolet(baseColor, materialColor, power);
            Destroy(gameObject);
        }
    }

}

public enum Power{
    DoubleJump,
    WallJump,
    End
}