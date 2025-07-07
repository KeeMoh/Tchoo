using UnityEngine;

public class ParalaxTileMap : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float xAxisParalaxEffect;
    [SerializeField, Range(0f, 1f)] private float yAxisParalaxEffect;
    [SerializeField] private GameObject cam;

    private Vector2 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    private void FixedUpdate()
    {
        float distanceX = cam.transform.position.x * xAxisParalaxEffect;
        float distanceY = cam.transform.position.y * yAxisParalaxEffect;
        //if(distanceX < distanceForEffect.x || distanceY < distanceForEffect.y) 
            transform.position = new Vector3(startPos.x + distanceX, startPos.y + distanceY, transform.position.z);
    }
}
