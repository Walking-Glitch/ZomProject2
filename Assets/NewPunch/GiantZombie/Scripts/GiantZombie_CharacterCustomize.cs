using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantZombie_CharacterCustomize : MonoBehaviour
{
	// Start is called before the first frame update
	private int headTyp;
	private int bodyTyp;
	private int trousersTyp;
	private int tanktopTyp;
	private int tshirtTyp;
	private int sneakersTyp;
	private int legsTyp;

    public Material[] BodyMaterials = new Material[4];
    public Material[] LowerBodyMaterials = new Material[4];
    public Material[] TshirtMaterials = new Material[4];
    public Material[] TankTopMaterials = new Material[4];
    public Material[] LegsMaterials = new Material[4];
    public Transform[] HeadTypes = new Transform[4];


    private bool tanktopOld;
	private Transform tshirtT;
	private Transform tanktopT;
	private Transform bodyToHideT;
	private Transform bodyExposedT;
	private Transform trousersT;
	private Transform legsT;
	private Transform sneakerR_T;
	private Transform sneakerL_T;
	private Transform footR_T;
	private Transform footL_T;

	private Transform headT_A;
	private Transform headT_B;
	private Transform headT_C;
	private Transform headT_D;
    private int eyesTyp;


    //private GiantZombie_AssetsList materialsList;

    private SkinnedMeshRenderer skinnedMeshRenderer;

	public enum FaceType
	{
		FaceV1,
		FaceV2,
		FaceV3,
		FaceV4
	}

	public enum BodySkin
	{
		BodyV1,
		BodyV2,
		BodyV3,
		BodyV4
	}

	public enum LegsSkin
	{
		LegsV1,
		LegsV2,
		LegsV3,
		LegsV4
	}

	public enum TrousersSkin
	{
		TrousersV1,
		TrousersV2,
		TrousersV3,
		TrousersV4

	}

	public enum SneakersSkin
	{
		SneakersV1,
		SneakersV2,
		SneakersV3,
		SneakersV4

	}

	public enum TankTopSkin
	{
		None,
		TankTopV1,
		TankTopV2,
		TankTopV3,
		TankTopV4
	}
	public enum TshirtSkin
	{
		None,
		TshirtV1,
		TshirtV2,
		TshirtV3,
		TshirtV4
	}

    public enum EyesGlow
    {
        No,
        Yes
    }




    public FaceType faceType;
	public BodySkin bodySkin;
	public LegsSkin legsSkin;
	public TrousersSkin trousersSkin;
	public SneakersSkin sneakersSkin;
	public TankTopSkin tanktopSkin;
	public TshirtSkin tshirtSkin;
	public bool trousersVisible = true;
	public bool sneakerLeftVisible = true;
	public bool sneakerRightVisible = true;
    public EyesGlow eyesGlow;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void charCustomize(int body, int trousers, int tanktop, int tshirt, int head, bool trousersV, int sneakers, int legs, int eyes)
	{
		//materialsList = gameObject.GetComponent<GiantZombie_AssetsList>();
		Material[] mat;
		GameObject obj;
		tshirtT = transform.Find("Giant_GRP/Tshirt");
		tanktopT = transform.Find("Giant_GRP/TankTop");
		trousersT = transform.Find("Giant_GRP/Trousers");
		bodyToHideT = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Torso");
		bodyExposedT = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Arms");
		legsT = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Legs");
		sneakerL_T = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Sneakers_L");
		sneakerR_T = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Sneakers_R");
		footL_T = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Foot_L");
		footR_T = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Foot_R");
		headT_A = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Head_A");
		headT_B = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Head_B");
		headT_C = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Head_C");
		headT_D = transform.Find("Giant_GRP/Giant_Zombie_SECTIONS/Head_D");


		for (int i = 0; i <= 3; i++)
		{
			HeadTypes[i].gameObject.SetActive(false);

		}
		HeadTypes[head].gameObject.SetActive(true);
		Renderer skinRend = HeadTypes[head].GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];
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
        // Body_Exposed arms



        skinRend = bodyExposedT.GetComponent<Renderer>();
			skinRend.material = BodyMaterials[body];

		//Torso to hide

		skinRend = bodyToHideT.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];


		//Trousers

		skinRend = trousersT.GetComponent<Renderer>();
		skinRend.material = LowerBodyMaterials[trousers];


		// legs


		skinRend = legsT.GetComponent<Renderer>();
		skinRend.material = LegsMaterials[legs];
		skinRend = footR_T.GetComponent<Renderer>();
		skinRend.material = LegsMaterials[legs];
		skinRend = footL_T.GetComponent<Renderer>();
		skinRend.material = LegsMaterials[legs];




		// Tshirt



		if (tshirt < 1)
		{

			tshirtT.gameObject.SetActive(false);
			bodyToHideT.gameObject.SetActive(true);
		}

		else
		{
			tshirtT.gameObject.SetActive(true);
			bodyToHideT.gameObject.SetActive(false);

			skinRend = tshirtT.GetComponent<Renderer>();
			skinRend.material = TshirtMaterials[tshirt - 1];

			if (tanktopOld)
			{

				tanktopT.gameObject.SetActive(false);
				tanktopSkin = 0;
				tanktopOld = false;
				tanktop = 0;

			}

		}

		// TankTop


		if (tanktop < 1)
		{

			tanktopT.gameObject.SetActive(false);
		}

		else
		{
			tanktopT.gameObject.SetActive(true);
			bodyToHideT.gameObject.SetActive(true);
			skinRend = tanktopT.GetComponent<Renderer>();
			skinRend.material = TankTopMaterials[tanktop - 1];

			tanktopOld = true;

		

			tshirtT.gameObject.SetActive(false);

			tshirtSkin = 0;

		}

		//TrousersVisible


        if (trousersVisible)
        {
			trousersT.gameObject.SetActive(true);
			legsT.gameObject.SetActive(false);

		}
        else
        {
			trousersT.gameObject.SetActive(false);
			legsT.gameObject.SetActive(true);
		}

		//Sneakers Visible


		if (sneakerRightVisible)
		{
			sneakerR_T.gameObject.SetActive(true);
			//legsT.gameObject.SetActive(false);
			skinRend = sneakerR_T.GetComponent<Renderer>();
			skinRend.material = LowerBodyMaterials[sneakers];

		}
		else
		{
			sneakerR_T.gameObject.SetActive(false);
			
		}

		if (sneakerLeftVisible)
		{
			sneakerL_T.gameObject.SetActive(true);
			//legsT.gameObject.SetActive(false);
			skinRend = sneakerL_T.GetComponent<Renderer>();
			skinRend.material = LowerBodyMaterials[sneakers];

		}
		else
		{
			sneakerL_T.gameObject.SetActive(false);

		}



	}

	void OnValidate()
	{
		//code for In Editor customize
		headTyp = (int)faceType;
		bodyTyp = (int)bodySkin;
		trousersTyp = (int)trousersSkin;
		tanktopTyp = (int)tanktopSkin;
		tshirtTyp = (int)tshirtSkin;
		sneakersTyp = (int)sneakersSkin;
		legsTyp = (int)legsSkin;
        eyesTyp = (int)eyesGlow;

        charCustomize(bodyTyp, trousersTyp, tanktopTyp, tshirtTyp, headTyp, trousersVisible, sneakersTyp, legsTyp, eyesTyp);

	}
}
