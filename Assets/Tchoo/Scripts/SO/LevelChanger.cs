using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private LevelConnection connection;
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Collider2D playerCollider;
    //[SerializeField] private Vector2 velocity = new (0f, 0.1f);

    private void Start()
    {
        if(connection == LevelConnection.ActiveConnection)
        {
            playerCollider.transform.position = spawnPoint.position;
            playerCollider.attachedRigidbody.linearVelocity = LevelConnection.VelocityOnConnection;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == playerCollider)
        {
            LevelConnection.VelocityOnConnection = collision.attachedRigidbody.linearVelocity;
            Debug.Log("Player!!");
            LevelConnection.ActiveConnection = connection;
            SceneManager.LoadSceneAsync("Tchoo/Scenes/" + targetSceneName, LoadSceneMode.Single);
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    PlayerController player = collision.collider.GetComponent<PlayerController>();
    //    if(player != null)
    //    {
    //        Debug.Log("Player!!");
    //        LevelConnection.ActiveConnection = connection;
    //        SceneManager.LoadScene("Tchoo/Scenes/" + targetSceneName, LoadSceneMode.Single);

    //    }
    //}
}
