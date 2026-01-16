using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "AnimationState")]
public class AnimationState : ScriptableObject
{
    //[SerializeField] private string animationName;
    [SerializeField] private EPlayerAnim ePlayerAnim;
    [SerializeField, Range(0, 1)] private float cancelableAtPercent;
    [SerializeField, Range(1, 4)] private int priorityLevel = 4;
    [SerializeField] private bool loop = false;
    [SerializeField] private int id = -1;

    public string AnimationName => ePlayerAnim.ToString();
    public bool Loop => loop;
    public float CancelableAtPercent => cancelableAtPercent;
    public int PriorityLevel => priorityLevel;
    public int Id => id;

    public EPlayerAnim EPlayerAnim { get => ePlayerAnim; set => ePlayerAnim = value; }

    public void SetAnimationHash(int id)
    {
        if (this.id == -1)
        {
            Debug.Log("SetAnimHash for " + AnimationName + " : " + id.ToString());
            this.id = id;
        }
        else
        {
            Debug.LogWarning("id is already set for animation " + AnimationName);
        }
    }
}
