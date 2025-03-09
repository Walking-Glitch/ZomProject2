using UnityEngine;

public class Idle : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.SetIsAlerted(true); 

        //float[] idleValues = { 0f, 1f };

        //float randomValue = idleValues[Random.Range(0, idleValues.Length)];
 
        //zombie.anim.SetFloat("IdleVariant", 0);
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        //if (zombie.currentSpeed > 0.1f && !zombie.IsZombieAlerted())
        //{
        //    zombie.SwitchState(zombie.roaming);
        //}
        if (zombie.IsZombieAlerted())
        {
            zombie.SwitchState(zombie.chasing);
        }
       

        if (zombie.IsPlayerInAttackArea() && !zombie.isDead)
        {
            zombie.SwitchState(zombie.attack);
        }
    }
}
