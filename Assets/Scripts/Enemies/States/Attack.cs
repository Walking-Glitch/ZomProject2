using UnityEngine;

public class Attack : ZombieBaseState
{
    private float elapsed;
    private float cooldown;
    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0;
        cooldown = 3f;

        zombie.aiPath.canMove = false;
        zombie.aiPath.maxSpeed = 0f;

        zombie.anim.SetTrigger("IsAttacking");
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
        //    if (!zombie.IsPlayerInAttackArea())
        //    {
        //        zombie.SwitchState(zombie.chasing);
        //    }
        //    else
        //    {
        //        if (elapsed < cooldown)
        //        {
        //            elapsed += Time.deltaTime;
        //        }
        //        else
        //        {
        //            zombie.SwitchState(zombie.attack);
        //        }

        //    }
        //}}

        if (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
        }
        else
        {
            zombie.SwitchState(zombie.idle);
        }
    }
}
