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

            Debug.Log("Code exe");
        }

        if (zombie.currentTarget.GetHealth() <= 0)
        {
            zombie.currentTarget = null;
            zombie.PlayZombieAnimationBoolClientRpc("IsAttacking", false);
            zombie.SwitchState(zombie.chasing);


            Debug.Log("Code exe");
        }


    }
}
