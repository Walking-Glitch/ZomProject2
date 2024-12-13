using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieGirlB_Customization : MonoBehaviour
{
    // Start is called before the first frame update

    private int bodyTyp;
    private int hairTyp;
    private int eyesTyp;
    public GameObject partsParent;
    //public GameObject hairObject;
    public Material[] BodyMaterials = new Material[2];
    public Material[] HairMaterials = new Material[2];


    public enum BodyType
    {
        V1,
        V2
    }

    public enum HairType
    {
        V1,
        V2
    }

    public enum EyesGlow
    {
        No,
        Yes
    }


    public BodyType bodyType;
    public HairType hairType;
    public EyesGlow eyesGlow;


    public void charCustomize(int body, int hair, int eyes)
    {

        Material[] mat;


        if (partsParent != null)
        {
            Renderer[] childRenderers = partsParent.GetComponentsInChildren<Renderer>();

            if (eyes == 0)
            {



                BodyMaterials[body].DisableKeyword("_EMISSION");
                BodyMaterials[body].SetFloat("_EmissiveExposureWeight", 1);
            }
            else
            {


                BodyMaterials[body].EnableKeyword("_EMISSION");
                BodyMaterials[body].SetFloat("_EmissiveExposureWeight", 0);

            }


            foreach (Renderer renderer in childRenderers)
            {
                Material[] materials = renderer.sharedMaterials;



                if (materials.Length > 1)
                {
                    mat = new Material[2];
                    mat[0] = BodyMaterials[body];
                    mat[1] = HairMaterials[hair];


                    renderer.materials = mat;
                }
                else
                {
                    renderer.sharedMaterial = BodyMaterials[body];
                }
            }
        }


        //Renderer skinRend = hairObject.GetComponent<Renderer>();
        //skinRend.material = HairMaterials[hair];




    }

    void OnValidate()
    {
        //code for In Editor customize

        bodyTyp = (int)bodyType;
        hairTyp = (int)hairType;
        eyesTyp = (int)eyesGlow;

        charCustomize(bodyTyp, hairTyp, eyesTyp);

    }
}