using Assets.Scripts.Game_Manager;
using System.Collections;
using UnityEngine;

public class CasingManager : MonoBehaviour
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

    public void SpawnBulletCasing()
    {
        GameObject casing = gameManager.BulletCasingPool.RequestCasing();
        casing.transform.position = CasingSpawnTransform.position;
        casing.transform.rotation = CasingSpawnTransform.rotation * Quaternion.Euler(90, 0, 0);
        Rigidbody casingRigidbody = casing.GetComponent<Rigidbody>();

        casingRigidbody.isKinematic = false;
        
        casingRigidbody.AddForce(CasingSpawnTransform.right *-1 * Random.Range(MinEjectForce, MaxEjectForce), ForceMode.Impulse);
        casingRigidbody.AddForce(CasingSpawnTransform.forward  * Random.Range(MinEjectForce, MaxEjectForce), ForceMode.Impulse);
        casingRigidbody.AddTorque(new Vector3(10, 0, 0) * ejectTorque, ForceMode.Impulse);

        StartCoroutine(DisableCasing(casing));

    }

    

    private IEnumerator DisableCasing(GameObject casing)
    {
        yield return new WaitForSeconds(delay);
        casing.SetActive(false);
    }
}

