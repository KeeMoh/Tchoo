using System;
using System.Collections;
using UnityEngine;

public class LeafTrap : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _spriteList;
    public event Action<LeafTrap> OnDestroy;


    private void Awake()
    {
        _spriteRenderer.sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Length)]; ;
        var rb = gameObject.GetComponent<Rigidbody2D>();
        rb.linearVelocity = new(0.8f, 0.2f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController pc))
        {
            pc.TakeDamage(damageAmount, transform.position); 
        }
    }

    public void StartLifeTime(float time)
    {
        //Debug.Log("Start life time");
        StartCoroutine(LifeTime(time));
    }

    public IEnumerator LifeTime(float time)
    {
        //Debug.Log("Coroutine life time");

        yield return new WaitForSeconds(time);
        //Debug.Log("Destroy trap");
        OnDestroy?.Invoke(this);
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
}
