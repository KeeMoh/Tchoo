using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class SecretCollider : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private float updateInterval = 0.05f;
    [SerializeField] private float fadeAmount = 0.02f;


    private void Start()
    {
        Color tmp = tilemap.color;
        tmp.a = 1;
        tilemap.color = tmp;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        Debug.Log("FadeIn Secret");
        float alphaVal = tilemap.color.a;
        Color tmp = tilemap.color;
        

        while (tilemap.color.a > 0.05f)
        {
            alphaVal -= fadeAmount;
            tmp.a = alphaVal;
            tilemap.color = tmp;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut Secret");

        float alphaVal = tilemap.color.a;
        Color tmp = tilemap.color;

        while (tilemap.color.a < 1)
        {
            alphaVal += fadeAmount;
            tmp.a = alphaVal;
            tilemap.color = tmp;

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
