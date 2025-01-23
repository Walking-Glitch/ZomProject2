using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Death : ZombieBaseState
{
     public override void EnterState(ZombieStateManager zombie)
    {
        zombie.isDead = true;
        zombie.SetCanMove(false); ;

        zombie.zombieAudioSource.Stop();

        if (zombie.IsKilledByTurret())
        {
            zombie.RagdollModeOn();
        }

        else if (zombie.IsKilledByExplosion())
        {
            
            zombie.RagdollModeOn();            
            zombie.rig.GetComponent<Rigidbody>().AddForce(zombie.GetExplosionDirection() *700f, ForceMode.Impulse);
            zombie.DismembermentByExplosion();
        }

        else if (zombie.isCrippled)
        {
            zombie.RagdollModeOn();
        }
        else
        {
            zombie.anim.SetBool("IsDead", true);
        }

        zombie.PlayerDestroyZombie();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
      
    }
}
