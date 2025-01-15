using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TurretShotgun : TurretBase
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
        fireRateTimer = 0;

        TriggerMuzzleFlash();
        PlaySfx();

        hasRecoil = true;

        if (hasRecoil)
        {
            AddRecoil();
        }

        int pelletCount = 8; // Number of pellets per shot
        float spreadAngle = 5f; // Spread angle in degrees

        for (int i = 0; i < pelletCount; i++)
        {
            // Calculate random spread
            Vector3 randomOffset = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
            Vector3 pelletDirection = ((currentEnemy.transform.position + Vector3.up * Random.Range(0.8f, 1.8f)) - GunEndTransform.position).normalized;
            pelletDirection += GunEndTransform.TransformDirection(randomOffset);

            // Perform raycast for each pellet
            if (Physics.Raycast(GunEndTransform.position, pelletDirection.normalized, out RaycastHit hit, Mathf.Infinity,
                 ShootMask))
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Environment"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    Instantiate(hitGroundDecal, hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Zombie"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                    Instantiate(hitGroundDecal, hit.point, decalRotation);

                    ZombieStateManager zombieStateManager;

                    Limbs limb = hit.collider.GetComponent<Limbs>();

                    ShotForceDir = hit.normal * -1;

                    if (limb != null)
                    {

                        float finalDamage = WeaponDamage * limb.damageMultiplier;

                        if (limb.limbName == "head")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                        }

                        else if (limb.limbName == "torso" || limb.limbName == "belly")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                        }

                        else
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                        }


                        float baseLimbDmg = 100f;
                        float finalLimbDmg = baseLimbDmg * limb.limbDamageMultiplier;

                        limb.LimbTakeDamage((int)finalLimbDmg);

                        if (limb.limbReplacement != null && limb.limbHealth <= 0)
                        {

                            Rigidbody rb = limb.limbReplacement.GetComponent<Rigidbody>();
                            Transform trans = limb.limbReplacement.transform;
                            Vector3 backwardForce = pelletDirection * 10f;
                            rb.AddForce(backwardForce, ForceMode.Impulse);
                            Debug.Log("force added");
                            Debug.Log("Force direction: " + pelletDirection);
                            Debug.DrawRay(trans.position, pelletDirection, Color.red, 2f);
                        }
                        Debug.Log(limb.limbName);
                    }

                }

                else
                {
                    Debug.Log(hit.distance);
                }
            }
        }
    }

    protected override void AddRecoil()
    {
        PitchTransform.Rotate(-10, 0, 0);
    }


}
