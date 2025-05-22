using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionFog : MonoBehaviour
{

    [SerializeField] private float amount = 0.1f; 
    [SerializeField] private float waitTime = 0.01f; 
    [SerializeField] private bool isCorrupted = true;
    private bool isInside = false;
    private bool isWaitingForDamage = false;

    private PlayerControllerOLD player;
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
        if (collision.TryGetComponent(out PlayerControllerOLD pc))
        {
            //Debug.Log("Exit Collider");
            player = pc;
            isInside = true;
            if (!isWaitingForDamage)
            {
                if (isCorrupted) player.GainCorruption(amount); else player.GainSanity(amount);
                //Debug.Log("GainCorruption");
                StartCoroutine(Waiting());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerControllerOLD pc))
        {
            player = pc;
            //Debug.Log("Exit Collider");
            isInside = false;
        }
    }

    IEnumerator Waiting()
    {
        //Debug.Log("Start Waiting");
        isWaitingForDamage = true;
        yield return new WaitForSeconds(waitTime);
        isWaitingForDamage = false;
        if(isInside)
        {
            if (isCorrupted) player.GainCorruption(amount); else player.GainSanity(amount);
            //Debug.Log("ReGainCorruption");
            StartCoroutine(Waiting());
        }
    }

    //private void OnParticleTrigger()
    //{
    //    Debug.Log("PARTICLE TRIGGER");
    //    ParticleSystem ps = GetComponent<ParticleSystem>();

    //    // particles
    //    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    //    List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();

    //    // get
    //    int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    //    int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    //    Debug.Log("PARTICLE TRIGGER ENTER = " + numEnter);
    //    Debug.Log("PARTICLE TRIGGER EXIT = " + numExit);
    //}
}
