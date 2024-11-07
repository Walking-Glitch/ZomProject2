using System.Collections;
using UnityEngine;

public class Hurt : ZombieBaseState
{
    public override void EnterState(ZombieStateManager zombie)
    {
        zombie.aiPath.canMove = false;
        zombie.anim.SetTrigger("IsHit");
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        if (zombie.anim.GetCurrentAnimatorStateInfo(0).IsName("ZombieHit1"))
        {
            float progress = zombie.anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
      
            if (progress > 0.9f)
            {
                zombie.SwitchState(zombie.chasing);
            }
        }
    }

}
