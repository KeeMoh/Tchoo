using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BoxCollider2D groundTrigger;
    [SerializeField] private float damage = 1f;
    [SerializeField] private LeafTrap trapToSpawn;
    [SerializeField] private int maxTrap;
    [SerializeField] private float trapLifeTime;
    [SerializeField] private float spawnTimer;
    private List<LeafTrap> activeTraps = new();

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(SpawnTraps());
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

    private void SpawnTrap()
    {
        if(activeTraps.Count < maxTrap)
        {
            //Debug.Log("SpawnTrap");
            var trap = Instantiate(trapToSpawn, transform.position, Quaternion.Euler(0,0,Random.Range(0,360)));
            activeTraps.Add(trap);
            trap.StartLifeTime(trapLifeTime);
            trap.OnDestroy += RemoveTrap;
        }
    }

    private void RemoveTrap(LeafTrap trap)
    {
        activeTraps.Remove(trap);
    }

    IEnumerator SpawnTraps()
    {
        yield return new WaitForSeconds(spawnTimer);
        SpawnTrap();
        StartCoroutine(SpawnTraps());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.TryGetComponent(out PlayerController player))
    //    {
    //        player.TakeDamage(damage, transform.position);
    //    }
    //}    
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            Debug.Log("TRY DO DAMAGES");
            player.TakeDamage(damage, transform.position);
        }
    }

    //private bool IsGrounded()
    //{
    //    if (Physics2D.OverlapBox(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize, 0, groundLayer))
    //    {
    //        timeSinceGrounded = 0;
    //        return true;
    //    }
    //    else
    //    {
    //        timeSinceGrounded += Time.deltaTime;
    //        return false;
    //    }
    //}

    //private bool IsBesideWall()
    //{
    //    if (Physics2D.OverlapBox(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize, 0, wallLayer))
    //    {
    //        imgWall.color = Color.blue;
    //        return true;
    //    }
    //    imgWall.color = debugColor;
    //    return false;
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawCube(groundCheckPos.position + (Vector3)groundCheckOffset, groundCheckSize);

    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawCube(wallCheckPos.position + (Vector3)wallCheckOffset, wallCheckSize);
    //}
}
