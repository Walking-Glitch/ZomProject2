using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TurretShotgun : TurretBase
{
    protected override void Start()
    {
        gameManager = GameManager.Instance;

        turretAudioSource = GetComponent<AudioSource>();

        CollectMuzzleFlashChildObjects(ParentMuzzleVFX);
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

        gameManager.EconomyManager.SpendAmmoFromPooledResources(pelletCount);

        for (int i = 0; i < pelletCount; i++)
        {
            gameManager.EconomyManager.SpendAmmoFromPooledResources(8);
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

                    gameManager.DecalManager.SpawnGroundHitDecal(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Metal"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    gameManager.DecalManager.SpawnMetalHitDecal(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Wood"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    gameManager.DecalManager.SpawnWoodHitDecal(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Concrete"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    gameManager.DecalManager.SpawnConcreteHitDecal(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Zombie"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                    gameManager.DecalManager.SpawnBloodHitDecal(hit.point, decalRotation);

                    ZombieStateManager zombieStateManager;

                    Limbs limb = hit.collider.GetComponent<Limbs>();

                    ShotForceDir = hit.normal * -1;

                    if (limb != null)
                    {

                        float finalDamage = WeaponDamage * limb.damageMultiplier;

                        if (limb.limbName == "head")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true, 0);
                        }

                        else if (limb.limbName == "torso" || limb.limbName == "belly")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true, 0);
                        }

                        else
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true, 0);
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
