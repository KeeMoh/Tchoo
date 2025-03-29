using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    PlayerController controller;
    LayerMask mask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Layers = " + collision.callbackLayers);
        if (collision.callbackLayers == mask)
        {
            Debug.Log("Trigger ground");
            controller.TellIsGrounded();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.callbackLayers == mask)
        {
            Debug.Log("Collide ground");
        }
    }
}
