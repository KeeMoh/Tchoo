using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ComposerHelper : MonoBehaviour
{
    [SerializeField] private float offSetXFactor = 2f;
    [SerializeField] private float offSetYBase = 1.5f;
    [SerializeField] private float offSetYFactor = 1f;
    [SerializeField] private float secondsToWait = 0.2f;
    [SerializeField] private float stepOffset = 0.5f;
    //[SerializeField] private float MaxOffSetY = -3f;
    private PlayerController target;
    private CinemachinePositionComposer composer;

    void Start()
    {
        composer = GetComponent<CinemachinePositionComposer>();
        target = composer.FollowTarget.GetComponent<PlayerController>();
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
        StopAllCoroutines();
        if (direction < 0)
        {
            StartCoroutine(LerpOffsetY(direction * offSetYFactor));
        }
        else
        {
            StartCoroutine(LerpOffsetY(offSetYBase));
        }
    }

    IEnumerator LerpOffsetY(float target)
    {
        while(composer.TargetOffset.y != target)
        {
            composer.TargetOffset.y = Mathf.MoveTowards(composer.TargetOffset.y, target, stepOffset);
            yield return new WaitForSeconds(secondsToWait);
        }
    }

    //IEnumerator LerpOffsetX(float target)
    //{
    //    composer.TargetOffset.x = Mathf.MoveTowards(composer.TargetOffset.x, target, 0.5f);
    //    if(composer.TargetOffset.x == target )
    //    {
    //        yield return null;
    //    }
    //    else
    //    {
    //        yield return new WaitForSeconds(secondsToWait);
    //    }
    //}

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
