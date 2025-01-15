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

    public void SpawnDecal(Vector3 position, Quaternion rotation)
    {
        GameObject decal = gameManager.DecalPool.RequestDecal();
        decal.transform.position = position;
        decal.transform.rotation = rotation;

        StartCoroutine(DisableCasing(decal));
    }

    private IEnumerator DisableCasing(GameObject decal)
    {
        yield return new WaitForSeconds(delay);
        decal.SetActive(false);
    }
}
