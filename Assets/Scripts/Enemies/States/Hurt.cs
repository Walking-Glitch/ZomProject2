using System.Collections;
using UnityEngine;

public class Hurt : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.SetIsAlerted(true);
        zombie.SetCanMove(false);
        //zombie.anim.SetTrigger("IsHit");
        zombie.PlayZombieAnimationTriggerClientRpc("IsHit");

        zombie.zombieAudioSource.Stop();
        zombie.PlayZombieHurtSfx();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
       
        if (zombie.isCrippled)
        {
            zombie.SwitchState(zombie.chasing);
        }
        else if (zombie.anim.GetCurrentAnimatorStateInfo(0).IsName("ZombieHit1"))
        {
            float progress = zombie.anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
      
            if (progress > 0.95f)
            {
                zombie.SwitchState(zombie.chasing);
            }
        }
    }

}
