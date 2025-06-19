using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class LeafTrap : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _spriteList;
    [SerializeField] private ParticleSystem corruptionFog;
    public event Action OnEndLife;
    private Rigidbody2D _rb;
    private bool isActive = false;

    private void Awake()
    {
        _spriteRenderer.sprite = _spriteList[UnityEngine.Random.Range(0, _spriteList.Length)]; ;
        _spriteRenderer.DOFade(0, 0);
        _rb = gameObject.GetComponent<Rigidbody2D>();
        corruptionFog.Stop();
        //_rb.linearVelocity = new(0.8f, 0.2f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive && collision.TryGetComponent(out PlayerController pc))
        {
            pc.TakeDamage(damageAmount, transform.position); 
        }
    }

    public void StartLifeTime(float lifeTime)
    {
        _spriteRenderer.DOFade(0, 0.15f).SetDelay(lifeTime).OnComplete(() => {
            //Debug.Log("Leaf set inactive");
            OnEndLife?.Invoke();
            isActive = false;
            corruptionFog.Clear();
        });
    }

    public void SpawnTrapWithForce(Vector2 LaunchForce, float lifeTime)
    {
        //Debug.Log("Launch Leaf");
        _rb.AddForce(LaunchForce);
        //Debug.Log("Leaf set active");
        //corruptionFog.
        _spriteRenderer.DOFade(1, 0.15f).OnComplete(() => { 
            isActive = true;
            corruptionFog.Play();
            StartLifeTime(lifeTime);
        });
    }

    //public IEnumerator LifeTime(float time)
    //{
    //    //Debug.Log("Coroutine life time");

    //    yield return new WaitForSeconds(time);
    //    //Debug.Log("Destroy trap");
    //    OnEndLife?.Invoke();
    //    yield return new WaitForEndOfFrame();

    //}
}