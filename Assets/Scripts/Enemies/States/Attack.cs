using UnityEngine;

public class Attack : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
    

        zombie.SetCanMove(false);

        zombie.zombieAudioSource.Stop();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        zombie.PlayZombieAttackSfx();

        if (!zombie.IsPlayerInAttackArea())
        {
            zombie.SwitchState(zombie.chasing);
        }

         

    }
}
