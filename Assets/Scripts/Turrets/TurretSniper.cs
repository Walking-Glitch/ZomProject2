using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TurretSniper : TurretBase
{
   
    protected override void Start()
    {
        gameManager = GameManager.Instance;

        turretAudioSource = GetComponent<AudioSource>();

        lightIntensity = muzzleFlashLight.intensity;
        muzzleFlashLight.intensity = 0;
    }

    protected override void Fire(bool hasRecoil)
    {
        base.Fire(true);
    }
   
}
