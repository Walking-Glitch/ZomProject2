using UnityEngine;

public class Idle : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.SetIsAlerted(false); 

        float[] idleValues = { 0f, 1f };

        float randomValue = idleValues[Random.Range(0, idleValues.Length)];
 
        zombie.anim.SetFloat("IdleVariant", randomValue);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        //if (zombie.currentSpeed > 0.1f && !zombie.IsZombieAlerted())
        //{
        //    zombie.SwitchState(zombie.roaming);
        //}

        if (zombie.IsPlayerInAttackArea())
        {
            zombie.SwitchState(zombie.attack);
        }
    }
}
