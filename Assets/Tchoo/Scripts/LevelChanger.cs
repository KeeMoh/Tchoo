using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private LevelConnection connection;
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform spawnPoint;
    //[SerializeField] private Collider2D playerCollider;
    //[SerializeField] private Vector2 velocity = new (0f, 0.1f);
    private LevelManager levelManager;

    private void Start()
    {
        levelManager = LevelManager.Instance;
        //levelManager = LevelManager.Instance;
        //StartCoroutine(deactivateTrigger());
        if (levelManager?.ActiveConnection == connection)
        {
            levelManager.SpawnPlayerAtPoint(spawnPoint);
        }

        //if(levelManager.PlayerControllerRef != null) levelManager.PlayerControllerRef.transform.position = spawnPoint.position;
    }

    //IEnumerator deactivateTrigger()
    //{
    //    GetComponent<BoxCollider2D>().enabled = false;
    //    yield return new WaitForSeconds(0.5f);
    //    GetComponent<BoxCollider2D>().enabled = true;
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out PlayerController player))
        {
            Debug.Log("Enter Trigger -//");
            //LevelConnection.ActiveConnection = connection;
            //levelManager.PlayerControllerRef = player;
            levelManager.ActiveConnection = connection;
            levelManager.ConnectionToNewLevel(targetSceneName, false);
            //LevelConnection.VelocityOnConnection = collision.attachedRigidbody.linearVelocity;
            //Debug.Log("Player!!");
            //SceneManager.LoadSceneAsync("Tchoo/Scenes/" + targetSceneName, LoadSceneMode.Single);
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
