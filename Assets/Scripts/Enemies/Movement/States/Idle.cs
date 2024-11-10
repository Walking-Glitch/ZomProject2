using UnityEngine;

public class Idle : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        float[] idleValues = { 0f, 1f };

        float randomValue = idleValues[Random.Range(0, idleValues.Length)];
 
        zombie.anim.SetFloat("IdleVariant", randomValue);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        if (zombie.currentSpeed > 0.1f)
        {
            zombie.SwitchState(zombie.roaming);
        }

    }
}
