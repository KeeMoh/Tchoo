using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SecretCollider : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private float updateInterval = 0.05f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        StopAllCoroutines();
    //        StartCoroutine(FadeOut());
    //    }
    //}

    private IEnumerator FadeIn()
    {
        Debug.Log("FadeIn");
        float alphaVal = tilemap.color.a;
        Color tmp = tilemap.color;

        while (tilemap.color.a > 0.2f)
        {
            alphaVal -= 0.01f;
            tmp.a = alphaVal;
            tilemap.color = tmp;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut");

        float alphaVal = tilemap.color.a;
        Color tmp = tilemap.color;

        while (tilemap.color.a < 1)
        {
            alphaVal += 0.01f;
            tmp.a = alphaVal;
            tilemap.color = tmp;

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
