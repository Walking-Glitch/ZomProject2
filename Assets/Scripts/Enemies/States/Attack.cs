using UnityEngine;

public class Attack : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
    

        zombie.SetCanMove(false);

        zombie.zombieAudioSource.Stop();

        zombie.PlayZombieAnimationBoolClientRpc("IsAttacking", zombie.IsAttackableInAttackArea());
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        zombie.PlayZombieAttackSfx();

        if (!zombie.IsAttackableInAttackArea())
        {
            zombie.PlayZombieAnimationBoolClientRpc("IsAttacking", zombie.IsAttackableInAttackArea());
            zombie.SwitchState(zombie.chasing);
        }

         

    }
}
