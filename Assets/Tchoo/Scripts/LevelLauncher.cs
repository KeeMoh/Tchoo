using System.Threading.Tasks;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class LevelLauncher : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D confiner;
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private float LensFOVOverride = 65;
    LevelManager levelManager;


    async void Awake()
    {
        Debug.Log("Start Task");

        await Task.Delay(3000);
        Debug.Log("End Task");
    }
    private void Start()
    {
        levelManager = LevelManager.Instance;
        if (levelManager?.ActiveConnection == null)
        {
            Debug.Log("Level launcher spawn at default position bcse activeconnection is null");
            levelManager.SpawnPlayerAtPoint(defaultSpawnPoint);
        }
        Debug.Log("Start Level");
        levelManager.CineCamRef.Lens.FieldOfView = LensFOVOverride;
        levelManager.CineCamRef.GetComponent<CinemachineConfiner2D>().BoundingShape2D = confiner;
    }

}
