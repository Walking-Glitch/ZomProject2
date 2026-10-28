using Assets.Scripts.Player.Weapon;
using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [Tooltip("Rounds per magazine.")]
    public int clipSize;
    [Tooltip("Reserve rounds carried for reloads. Raise it for long test sessions.")]
    public int extraAmmo;
    [Tooltip("Rounds in the current magazine (runtime; reset to Clip Size on start).")]
    public int currentAmmo;

    public AudioClip magInClip;
    public AudioClip magOutClip;
    public AudioClip releaseSlideClip;
    public AudioClip switchFireMode;

    private WeaponManager weaponManager;


    void Start()
    {
        currentAmmo = clipSize;
        weaponManager = GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reload()
    {
        if (extraAmmo >= clipSize)
        {
            int ammoToReload = clipSize - currentAmmo;
            extraAmmo -= ammoToReload;
            currentAmmo += ammoToReload;
        }
        else if (extraAmmo > 0)
        {
            if (extraAmmo + currentAmmo > clipSize)
            {
                int leftOverAmmo = extraAmmo + currentAmmo - clipSize;
                extraAmmo = leftOverAmmo;
                currentAmmo = clipSize;
            }
            else
            {
                currentAmmo += extraAmmo;
                extraAmmo = 0;
            }
        }

        //weaponManager.RefreshDisplay(currentAmmo, extraAmmo);

    }



}
