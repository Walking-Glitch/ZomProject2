using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Weapon;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CasingManager : NetworkBehaviour
{
    public Transform CasingSpawnTransform;
    private GameManager gameManager;
    private float delay = 1f;
    public float MaxEjectForce;
    public float MinEjectForce;
    public float ejectTorque;
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    #region SinglePlayer
    public void SpawnBulletCasing()
    {
        GameObject casing = gameManager.BulletCasingPool.RequestCasing();
        casing.transform.position = CasingSpawnTransform.position;
        casing.transform.rotation = CasingSpawnTransform.rotation * Quaternion.Euler(90, 0, 0);
        Rigidbody casingRigidbody = casing.GetComponent<Rigidbody>();

        casingRigidbody.isKinematic = false;

        casingRigidbody.AddForce(CasingSpawnTransform.right * -1 * Random.Range(MinEjectForce, MaxEjectForce), ForceMode.Impulse);
        casingRigidbody.AddForce(CasingSpawnTransform.forward * Random.Range(MinEjectForce, MaxEjectForce), ForceMode.Impulse);
        casingRigidbody.AddTorque(new Vector3(10, 0, 0) * ejectTorque, ForceMode.Impulse);

        StartCoroutine(DisableCasing(casing));
    }
    #endregion

    #region Multiplayer

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBulletCasingServerRpc(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 forceForward, Vector3 forceRight, Vector3 torque)
    {
     
        Quaternion finalRotation = spawnRotation * Quaternion.Euler(90, 0, 0);


        //Vector3 forceRight = CasingSpawnTransform.right * -1 * Random.Range(MinEjectForce, MaxEjectForce);
        //Vector3 forceForward =  CasingSpawnTransform.forward * Random.Range(MinEjectForce, MaxEjectForce);
        //Vector3 torque = new Vector3(10, 0, 0) * ejectTorque;

        //Destroy(tempRigidbody.gameObject);

        SpawnBulletCasingClientRpc(spawnPosition, finalRotation, forceForward, forceRight, torque);
    }

    [ClientRpc]
    private void SpawnBulletCasingClientRpc(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 forceForward, Vector3 forceRight, Vector3 torque)
    {
        

        //Debug.Log("CALLING CASINGS FROM RPC");

        GameObject casing = gameManager.BulletCasingPool.RequestCasing();
        casing.transform.position = spawnPosition;
        casing.transform.rotation = spawnRotation;
        Rigidbody casingRigidbody = casing.GetComponent<Rigidbody>();

        casingRigidbody.isKinematic = false;
        casingRigidbody.AddForce(forceRight, ForceMode.Impulse);
        casingRigidbody.AddForce(forceForward, ForceMode.Impulse);       
        casingRigidbody.angularVelocity = torque;

        StartCoroutine(DisableCasing(casing));
    }
    #endregion

    private IEnumerator DisableCasing(GameObject casing)
    {
        yield return new WaitForSeconds(delay);
        casing.SetActive(false);
    }

    
}


