using Assets.Scripts.Game_Manager;
using System.Collections;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    private GameManager gameManager;
    private float delay = 1f;
     
    void Start()
    {
        gameManager = GameManager.Instance;
    }

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

    private IEnumerator DisableGroundDecal(GameObject decal)
    {
        yield return new WaitForSeconds(delay);
        decal.SetActive(false);
    }
}
