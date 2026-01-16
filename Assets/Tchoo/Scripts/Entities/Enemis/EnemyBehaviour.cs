using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BoxCollider2D groundTrigger;

    [Header("Life/Damages")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float baseLife = 10f;
    [SerializeField] private ParticleSystem corruptionFog;

    [Header("Soul")]
    [SerializeField] private Soul soulPrefab;
    [SerializeField] private SoulType soulType;
    [SerializeField] private float soulLifeDuration;

    [Header("Attack")]
    [SerializeField] private LeafTrap trapToSpawn;
    [SerializeField, Range(0,6)] private int maxTrap;
    [SerializeField] private float trapLifeTime;
    [SerializeField] private const float spawnTimer = 2;
    [SerializeField] private Transform leafContainer;

    //[SerializeField] private prefab SoulPrefab;
    public float currentLifeAmount;
    private List<LeafTrap> activeTraps = new();
    private Animator animator;
    private SpriteRenderer sprite;
    private float moveSpeed;
    private Rigidbody2D rb;
    private Soul activeSoul;
    private bool isAlive = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        currentLifeAmount = baseLife;        
    }

    void Start()
    {
        if (LevelManager.Instance.EnemiesKilled.Contains(name))
        {
            Destroy(gameObject);
        }
        corruptionFog.Play();
        if (activeTraps.Count == 0)
        {
            while (activeTraps.Count < maxTrap)
            {
                var leafTrap = Instantiate(trapToSpawn, transform.position, Quaternion.identity, leafContainer);
                activeTraps.Add(leafTrap);
            }
        }
        activeTraps.ForEach(t => {
            t.OnEndLife += PrepareToShoot;
        });
        PrepareToShoot();
        moveSpeed = baseMoveSpeed;
    }

    void Update()
    {
        if (transform.localScale.x > Mathf.Epsilon)
        {
            rb.linearVelocityX = -moveSpeed;
        }
        else
        {
            rb.linearVelocityX = moveSpeed;
        }
    }

    [Button]
    public void Take4Damage()
    {
        TakeDamage(4);
    }

    public async void TakeDamage(float damages)
    {
        sprite.color = Color.red;
        Time.timeScale = 0;
        await Task.Delay(50);
        Time.timeScale = 1;
        sprite.DOColor(Color.white, 0.2f);
        currentLifeAmount -= damages;
        if (currentLifeAmount <= 0) Death();
    }

    private void Death()
    {
        if(!isAlive) return;
        isAlive = false;
        moveSpeed = 0;
        animator.SetTrigger("Death");
        animator.ResetTrigger("Attack");
        corruptionFog.Clear();
        corruptionFog.Stop();
        SpawnSoul();
        //Instantiate(SoulPrefab, transform.position);
    }

    private void SpawnSoul()
    {
        if (soulType == SoulType.Evil)
        {
            sprite.DOFade(0, 0.2f);
        }
        activeSoul = Instantiate(soulPrefab, transform, true);
        activeSoul.SpawnSoul(soulType, transform.position, soulLifeDuration);
        activeSoul.OnLevitationEnd += SoulDisappear;
    }

    private void SoulDisappear(bool isPurified)
    {
        activeSoul.OnLevitationEnd -= SoulDisappear;

        //GameObject soulToDestroy = activeSoul.gameObject;
        //activeSoul = null;

        Destroy(activeSoul.gameObject);
        Debug.Log("soul has been destroyed");
        if(!isPurified)
        {
            if (soulType == SoulType.Evil)
            {
                sprite.DOFade(1, 0.5f);
            }
            Revive();
        }
        else
        {
            if (soulType != SoulType.Evil)
            {
                Debug.Log("Destroy Enemy and add it to the list");
                sprite.DOFade(0, 1.2f).OnComplete(() => 
                {
                    LevelManager.Instance.EnemiesKilled.Add(name);
                    Destroy(gameObject); 
                });
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
    }

    [Button]
    private void Revive()
    {
        if (isAlive) return;
        isAlive = true;
        currentLifeAmount = baseLife / 3;
        corruptionFog.Play();
        StartCoroutine(StartAttack(0.25f));
    }

    private IEnumerator StartAttack(float spawnTimer = spawnTimer)
    {
        yield return new WaitForSeconds(spawnTimer);
        if (currentLifeAmount <= 0) yield break;
        animator.SetTrigger("Attack");
        //Debug.Log("SET TRIGGER ATTACK..");
        moveSpeed = 0;
        //Debug.Log("animation is finish !");
        animator.speed = 0;
        yield return new WaitForSeconds(0.2f);
        animator.speed = 1;
        yield return new WaitForSeconds(0.25f);
        moveSpeed = currentLifeAmount <= 0 ? 0 : baseMoveSpeed;
    }

    private void SpawnTrap()
    {
        if (currentLifeAmount <= 0) return;

        HashSet<float> usedValues = new();

        activeTraps.ForEach(t => {
            float randomX;
            bool isValid;

            do
            {
                randomX = Random.Range(-4.6f, 3.2f);
                isValid = true;

                foreach (float existingValue in usedValues)
                {
                    if (Mathf.Abs(existingValue - randomX) < 1.2f)
                    {
                        isValid = false;
                        break;
                    }
                }
            } while (!isValid);

            usedValues.Add(randomX); // Ajoute uniquement une valeur valide
            t.transform.position = transform.position;
            t.SpawnTrapWithForce(new(randomX, 2.8f), trapLifeTime);
        });
        //var trap = Instantiate(trapToSpawn, transform.position, Quaternion.Euler(0,0,Random.Range(0,360)));
        //activeTraps.Add(trap);
        //trap.StartLifeTime(trapLifeTime);
        //trap.OnEndLife += PrepareToShoot;
    }

    private void PrepareToShoot()
    {
        if(currentLifeAmount <= 0) return; else { 
            //Debug.Log("Prepaaaare"); 
        } 
        StartCoroutine(StartAttack());
    }


    //IEnumerator SpawnTraps()
    //{
    //    yield return new WaitForSeconds(spawnTimer);
    //    SpawnTrap();
    //    StartCoroutine(SpawnTraps());
    //}
    private void Flip()
    {
        //Flip visual
        Vector3 ls = transform.localScale;
        ls.x *= -1f;
        transform.localScale = ls;

        //Flip others who needs
        leafContainer.transform.localScale = ls;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Flip();
            //transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentLifeAmount <= 0) return;
        if (collision.TryGetComponent(out PlayerController player))
        {
            Debug.Log("TRY DO DAMAGES");
            player.TakeDamage(damage, transform.position);
        }
    }

}
