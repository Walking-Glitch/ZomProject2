using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TurretSniper : TurretBase
{
   
    protected override void Start()
    {
        gameManager = GameManager.Instance;

        turretAudioSource = GetComponent<AudioSource>();

        CollectMuzzleFlashChildObjects(ParentMuzzleVFX);
    }

    protected override void Fire(bool hasRecoil)
    {
        base.Fire(true);
    }
   
}
