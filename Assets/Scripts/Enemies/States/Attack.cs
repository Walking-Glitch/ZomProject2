using UnityEngine;

public class Attack : ZombieBaseState
{
    private float elapsed;
    private float cooldown;

    public override void EnterState(ZombieStateManager zombie)
    {
        elapsed = 0;
        cooldown = 5f;


        zombie.aiPath.maxSpeed = 0f;
        zombie.anim.SetTrigger("IsAttacking");
    }

    public override void UpdateState(ZombieStateManager zombie)
    {

        if (!zombie.IsPlayerInAttackArea())
        {
            if (zombie.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack_melee"))
            {
                float progress = zombie.anim.GetCurrentAnimatorStateInfo(1).normalizedTime % 1;

                if (progress > 0.9f)
                {
                    zombie.SwitchState(zombie.chasing);
                }
            }
        }


        if (elapsed < cooldown)
        {
            elapsed += Time.deltaTime;
        }
        else
        {
            zombie.SwitchState(zombie.attack);


            //Debug.Log(elapsed);

        }

    }
}
