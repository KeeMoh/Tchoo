using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class SecretCollider : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private float updateInterval = 0.05f;
    [SerializeField] private float fadeAmount = 0.02f;
    [SerializeField] private float targetAmount = 0.05f;
    [SerializeField] private bool isVisible = true;

    private void Start()
    {
        Color tmp = tilemap.color;
        if(isVisible)
        {
            tmp.a = 1;
        }
        else
        {
            tmp.a = 0;
        }
        tilemap.color = tmp;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            if(isVisible)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();
            if (isVisible)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("FadeIn Secret");
        float alphaVal = tilemap.color.a;
        Color tmp = tilemap.color;
        

        while (tilemap.color.a > targetAmount)
        {
            alphaVal -= fadeAmount;
            tmp.a = alphaVal;
            tilemap.color = tmp;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private IEnumerator FadeIn()
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
