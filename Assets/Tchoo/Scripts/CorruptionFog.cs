using System.Collections;
using UnityEngine;

public class CorruptionFog : MonoBehaviour
{

    [SerializeField] private float waitTime = 0.5f; 
    [SerializeField] private bool isCorrupted = true;
    private bool isInside = false;
    private bool isWaitingForDamage = false;

    private PlayerController player;
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.TryGetComponent(out PlayerController player) && !isInside)
    //    {
    //        StartCoroutine(Waiting());
    //        Debug.Log("Gain corruption from FOG");
    //        player.GainCorruption(0.25f);
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out player))
        {
            Debug.Log("Exit Collider");
            isInside = true;
            if (!isWaitingForDamage)
            {
                if (isCorrupted) player.GainCorruption(0.25f); else player.GainSanity(0.5f);
                Debug.Log("GainCorruption");
                StartCoroutine(Waiting());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out player))
        {
            Debug.Log("Exit Collider");
            isInside = false;
        }
    }

    IEnumerator Waiting()
    {
        Debug.Log("Start Waiting");
        isWaitingForDamage = true;
        yield return new WaitForSeconds(waitTime);
        isWaitingForDamage = false;
        if(isInside)
        {
            if (isCorrupted) player.GainCorruption(0.25f); else player.GainSanity(0.5f);
            Debug.Log("ReGainCorruption");
            StartCoroutine(Waiting());
        }
    }
}
