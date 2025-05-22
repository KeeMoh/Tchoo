using Unity.Cinemachine;
using UnityEngine;

public class ComposerHelper : MonoBehaviour
{
    [SerializeField] private float offSetXFactor = 2f;
    [SerializeField] private float offSetYBase = 1.5f;
    [SerializeField] private float offSetYFactor = 1f;
    [SerializeField] private float MaxOffSetY = -3f;
    private PlayerControllerOLD target;
    private CinemachinePositionComposer composer;

    void Start()
    {
        composer = GetComponent<CinemachinePositionComposer>();
        target = composer.FollowTarget.GetComponent<PlayerControllerOLD>();
        target.OnDirectionXChange += AdjustCameraXOffset;
        target.OnDirectionYChange += AdjustCameraYOffset;

        composer.TargetOffset.y = offSetYBase;
        composer.TargetOffset.x = offSetXFactor;
    }

    void AdjustCameraXOffset(float direction)
    {
        composer.TargetOffset.x = direction * offSetXFactor;
    }    
    
    void AdjustCameraYOffset(float direction)
    {
        if (direction < 0)
        {
            composer.TargetOffset.y = Mathf.Max(direction * offSetYFactor, MaxOffSetY);
        }
        else
        {
            composer.TargetOffset.y = offSetYBase;
        }
    }

    //void LateUpdate()
    //{
    //    composer.TargetOffset.x = offSetX * lookaheadFactor;
    //}

    private void OnDestroy()
    {
        Debug.Log("Reset Camera Composer values");
        composer.TargetOffset.x = offSetXFactor;
        composer.TargetOffset.y = offSetYBase;
    }
}
