using Assets.Scripts.Game_Manager;
using System.Collections;
using UnityEngine;

public class CasingManager : MonoBehaviour
{
    public Transform CasingSpawnTransform;
    private GameManager gameManager;
    private float delay = 1f;
    public float ejectForce;
    public float ejectTorque;
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void SpawnBulletCasing()
    {
        GameObject casingRight = gameManager.BulletCasingPool.RequestCasing();
        casingRight.transform.position = CasingSpawnTransform.position;
        casingRight.transform.rotation = CasingSpawnTransform.rotation;
        Rigidbody casingRightRigidbody = casingRight.GetComponent<Rigidbody>();

        casingRightRigidbody.isKinematic = false;
        Vector3 rightForce = CasingSpawnTransform.right * 5f + CasingSpawnTransform.forward * 10f + CasingSpawnTransform.up * 10f;
        casingRightRigidbody.AddForce(rightForce * ejectForce);
        casingRightRigidbody.AddTorque(casingRight.transform.position * ejectTorque);

        StartCoroutine(DisableCasing(casingRight));

    }

    

    private IEnumerator DisableCasing(GameObject casing)
    {
        yield return new WaitForSeconds(delay);
        casing.SetActive(false);
    }
}

