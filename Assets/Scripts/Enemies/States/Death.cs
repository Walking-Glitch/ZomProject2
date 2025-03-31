using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Death : ZombieBaseState
{
     public override void EnterState(ZombieStateManager zombie)
    {
        zombie.NetworkIsDead.Value = true;
   
        zombie.SetCanMove(false); ;

        zombie.zombieAudioSource.Stop();

        zombie.PlayZombieAnimationBoolClientRpc("IsDead", zombie.isDead);

        if (zombie.IsKilledByTurret())
        {
            zombie.RagdollModeOnClientRpc();
        }

        else if (zombie.IsKilledByExplosion())
        {
            
            zombie.RagdollModeOnClientRpc();            
            zombie.rig.GetComponent<Rigidbody>().AddForce(zombie.GetExplosionDirection() *700f, ForceMode.Impulse);
            zombie.DismembermentByExplosionClientRpc();
        }

        else if (zombie.isCrippled)
        {
            zombie.RagdollModeOnClientRpc();
        }
        else
        {
            zombie.PlayZombieAnimationBoolClientRpc("IsActive", true);
           
            //zombie.anim.SetBool("IsActive", true);
        }

        //zombie.PlayerDestroyZombieClientRpc();
        zombie.PlayerDestroyZombieServerRpc();
    }

    public override void UpdateState(ZombieStateManager zombie)
    {
      
    }
}
