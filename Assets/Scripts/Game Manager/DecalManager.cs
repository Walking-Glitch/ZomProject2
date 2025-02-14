using Assets.Scripts.Game_Manager;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DecalManager : NetworkBehaviour
{
    private GameManager gameManager;
    private float delay = 1f;

     
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    #region SinglePlayer
    public void SpawnGroundHitDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestGroundDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    public void SpawnBloodHitDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestBloodDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    public void SpawnMetalHitDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestMetalDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    public void SpawnWoodHitDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestWoodDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    public void SpawnConcreteHitDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestConcreteDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }
    #endregion

    
    #region Multiplayer

    [ServerRpc(RequireOwnership = false)]
    public void SpawnGroundHitDecalServerRpc(Vector3 position, Quaternion rotation)
    {
        // Ensure only the server processes the request
        if (!IsServer) return;
         
        SpawnGroundHitDecalClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SpawnGroundHitDecalClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestGroundDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBloodHitDecalServerRpc(Vector3 position, Quaternion rotation)
    {
        // Ensure only the server processes the request
        if (!IsServer) return;

        SpawnBloodHitDecalClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SpawnBloodHitDecalClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestBloodDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnMetalHitDecalServerRpc(Vector3 position, Quaternion rotation)
    {
        // Ensure only the server processes the request
        if (!IsServer) return;

        SpawnMetalHitDecalClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SpawnMetalHitDecalClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestMetalDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnWoodHitDecalServerRpc(Vector3 position, Quaternion rotation)
    {
        // Ensure only the server processes the request
        if (!IsServer) return;

        SpawnWoodHitDecalClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SpawnWoodHitDecalClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestWoodDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnConcreteHitDecalServerRpc(Vector3 position, Quaternion rotation)
    {
        // Ensure only the server processes the request
        if (!IsServer) return;

        SpawnConcreteHitDecalClientRpc(position, rotation);
    }

    [ClientRpc]
    private void SpawnConcreteHitDecalClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestConcreteDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableGroundDecal(decal));
    }

    #endregion

    private IEnumerator DisableGroundDecal(GameObject decal)
    {
        yield return new WaitForSeconds(delay);
        decal.SetActive(false);
    }
}
