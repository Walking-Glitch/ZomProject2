using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TurretSniper : TurretBase
{
    private float barrelRecoil; 
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
    protected override void DisplayLaser()
    {
        if (currentEnemy != null)
        {


            Vector3 laserDirection = GunEndTransform.forward;


            Ray ray = new Ray(GunEndTransform.position, laserDirection);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ShootMask))
            {
                LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
                LaserAimTransform.position = hit.point;

            }

            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, LaserAimTransform.position);

            if (WeaponSpotLight != null)
            {
                WeaponSpotLight.gameObject.SetActive(true);
                WeaponSpotLight.transform.LookAt(LaserAimTransform.position);
            }

        }
        else
        {
            LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;
            laserLine.enabled = false;

            if (WeaponSpotLight != null) WeaponSpotLight.gameObject.SetActive(false);
        }
    }

}
