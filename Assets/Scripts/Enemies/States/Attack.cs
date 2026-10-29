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
        zombie.FaceAttackTarget();

        // also covers the target dying: dead targets drop out of reach
        if (!zombie.IsAttackableInAttackArea())
        {
            zombie.SwitchState(zombie.chasing);
        }

         

    }
}
