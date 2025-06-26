using UnityEngine;

public class PlayerVFXHandler : MonoBehaviour
{
    [SerializeField] private ParticleSystem landingEffect;

    public void Hook(PlayerMovementHandler movement)
    {
        movement.OnGroundedStateChanged += HandleGroundedStateChanged;
    }

    private void HandleGroundedStateChanged(bool grounded)
    {
        if(grounded) // FreshlyGrounded / Landing
        {
            PlayLandingEffect();
        }
        else { } // Freshly jumped / Fall from platform
    }

    private void PlayLandingEffect()
    {
        landingEffect?.Play();
    }
}