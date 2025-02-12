using Unity.Netcode;
using UnityEngine;

public class WeaponLaser : NetworkBehaviour
{
    public Transform laserOrigin;
    public Transform laseroffTarget;
    public Transform laseronTarget;
    public float range;

    public LineRenderer laserLine;
    public Material[] LaserMaterial;
    [SerializeField]private bool laserReady;

    private Animator anim;

    // Network variables
    private NetworkVariable<bool> laserNetworkReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> laserEndPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> laserStartPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    void Start()
    {
        anim = GetComponent<Animator>();
        laserLine.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        // Sync laser activation
        laserNetworkReady.OnValueChanged += (prev, curr) =>
        {
            laserLine.enabled = curr;
        };

        // Sync laser position
        laserEndPosition.OnValueChanged += (prev, curr) =>
        {
            laserLine.SetPosition(1, curr);
        };

        laserStartPosition.OnValueChanged += (prev, curr) =>
        {
            laserLine.SetPosition(0, curr);
        };
    }

    public void DisableLaser()
    {
        if(!IsServer && !IsClient)
        {
            laserReady = false;
            laserLine.enabled = false;
        }

        else if (IsOwner)
        {
            laserLine.enabled = false;
            laserNetworkReady.Value = false;
        }
        
    }

    public void DisplayLaser(bool isOnTarget)
    {
        if (!IsServer && !IsClient)
        {
            CheckAnimationProgress();

            if (laserReady)
            {
                laserLine.enabled = true;
                laserLine.SetPosition(0, laserOrigin.position);
                laserLine.SetPosition(1, isOnTarget ? laseronTarget.position : laseroffTarget.position);
            }
        }
        else if (IsOwner)
        {

            CheckAnimationProgress();

            if (laserNetworkReady.Value)
            {
                laserLine.enabled = true;
                laserStartPosition.Value = laserOrigin.position;
                laserEndPosition.Value = isOnTarget ? laseronTarget.position : laseroffTarget.position;
            }
        

        }
    }

    private void CheckAnimationProgress()
    {
        if(!IsServer && !IsClient)
        {
            laserReady = anim.GetLayerWeight(1) > 0.85f;
        }

        else if (IsOwner)
        {
            laserNetworkReady.Value = anim.GetLayerWeight(1) > 0.85f;
        }
       
    }

    public void ChangeLazerColorInventory()
    {
        if (!IsServer && !IsClient) laserLine.material = LaserMaterial[1];
        else if (IsOwner) ChangeLaserColorServerRpc(1);
    }

    public void ChangeLazerColorDefault()
    {
        if (!IsServer && !IsClient) laserLine.material = LaserMaterial[0];
        else if (IsOwner) ChangeLaserColorServerRpc(0);
    }

    [ServerRpc]
    private void ChangeLaserColorServerRpc(int index)
    {
        ChangeLaserColorClientRpc(index);
    }

    [ClientRpc]
    private void ChangeLaserColorClientRpc(int index)
    {
        laserLine.material = LaserMaterial[index];
    }

}
