using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrbanZombie_Parts_CharacterCustomize : MonoBehaviour
{
	// Start is called before the first frame update
	// Start is called before the first frame update
	private int headTyp;
	private int bodyTyp;
	private int trousersTyp;
	private int tanktopTyp;
	private int hoodieTyp;
    private int eyesTyp;

    public Material[] BodyMaterials = new Material[5];
    public Material[] TrousersMaterials = new Material[4];
    public Material[] HoodieMaterials = new Material[4];
    public Material[] TankTopMaterials = new Material[4];


    private bool tanktopOld;
	private Transform hoodieT;
	private Transform tanktopT;
	private Transform bodyToHideT;
	private Transform bodyExposedT;

	private Transform headT_A;
	private Transform headT_B;


	private UrbanZombie_AssetsList materialsList;

	private SkinnedMeshRenderer skinnedMeshRenderer;

	public enum FaceType
	{
		V1,
		V2
	}

	public enum BodySkin
	{
		V1,
		V2,
		V3,
		V4,
		V5
	}

	public enum TrousersSkin
	{
		V1,
		V2,
		V3,
		V4

	}

	public enum TankTopSkin
	{
		None,
		V1,
		V2,
		V3,
		V4
	}
	public enum HoodieSkin
	{
		None,
		V1,
		V2,
		V3,
		V4
	}

    public enum EyesGlow
    {
        No,
        Yes
    }

    public FaceType faceType;
	public BodySkin bodyType;
	public TrousersSkin trousersType;
	public TankTopSkin tanktopType;
	public HoodieSkin hoodieType;
    public EyesGlow eyesGlow;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public void charCustomize(int body, int trousers, int tanktop, int hoodie, int head, int eyes)
	{
		materialsList = gameObject.GetComponent<UrbanZombie_AssetsList>();
		Material[] mat;
		hoodieT = transform.Find("Geo/HoodieParts");
		tanktopT = transform.Find("Geo/Body_Parts/Hips/Torso/TankTop");
		headT_A = transform.Find("Geo/Body_Parts/Head_A/HeadA");
		headT_B = transform.Find("Geo/Body_Parts/Head_B/HeadB");


		// KneeL
		Transform curSub = transform.Find("Geo/Body_Parts/KneeL");
		Renderer skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// KneeR
		curSub = transform.Find("Geo/Body_Parts/KneeR");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;
		// FootL
		curSub = transform.Find("Geo/Body_Parts/FootL");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// FootR
		curSub = transform.Find("Geo/Body_Parts/FootR");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// LegL
		curSub = transform.Find("Geo/Body_Parts/LegL");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// LegR
		curSub = transform.Find("Geo/Body_Parts/LegR");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;
		// Hips
		curSub = transform.Find("Geo/Body_Parts/Hips");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[1] = BodyMaterials[body];
		mat[0] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// Torso
		curSub = transform.Find("Geo/Body_Parts/Hips/Torso");
		skinRend = curSub.GetComponent<Renderer>();
		mat = new Material[2];
		mat[0] = BodyMaterials[body];
		mat[1] = TrousersMaterials[trousers];

		skinRend.materials = mat;

		// Neck

		curSub = transform.Find("Geo/Body_Parts/Hips/Neck");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];

		// HeadA

		curSub = transform.Find("Geo/Body_Parts/Head_A/HeadA");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];

		// HeadB

		curSub = transform.Find("Geo/Body_Parts/Head_B/HeadB");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];


		// ArmL

		curSub = transform.Find("Geo/Body_Parts/ArmL");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];
		// ArmR

		curSub = transform.Find("Geo/Body_Parts/ArmR");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];

		// ForeArmL

		curSub = transform.Find("Geo/Body_Parts/ForeArmL");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];
		// ForeArmR

		curSub = transform.Find("Geo/Body_Parts/ForeArmR");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];

		// HandL

		curSub = transform.Find("Geo/Body_Parts/HandL");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];
		// HandR

		curSub = transform.Find("Geo/Body_Parts/HandR");
		skinRend = curSub.GetComponent<Renderer>();
		skinRend.material = BodyMaterials[body];

		// Hoodie



		if (hoodie < 1)
		{

			//hoodieT.gameObject.SetActive(false);
			//bodyToHideT.gameObject.SetActive(true);
			ShowHoodie(false);
			ShowTorso(true);
		}

		else
		{
			//hoodieT.gameObject.SetActive(true);
			ShowHoodie(true);
			//bodyToHideT.gameObject.SetActive(false);
			ShowTorso(false);





			curSub = transform.Find("Geo/Body_Parts/Hoodie_ArmL");
			skinRend = curSub.GetComponent<Renderer>();
			mat = new Material[2];
			mat[1] = BodyMaterials[body];
			mat[0] = HoodieMaterials[hoodie - 1];

			skinRend.materials = mat;

			curSub = transform.Find("Geo/Body_Parts/Hoodie_ArmR");
			skinRend = curSub.GetComponent<Renderer>();
			mat = new Material[2];
			mat[1] = BodyMaterials[body];
			mat[0] = HoodieMaterials[hoodie - 1];

			skinRend.materials = mat;
			curSub = transform.Find("Geo/Body_Parts/Hoodie_ForeArmL");
			skinRend = curSub.GetComponent<Renderer>();
			mat = new Material[2];
			mat[1] = BodyMaterials[body];
			mat[0] = HoodieMaterials[hoodie - 1];

			skinRend.materials = mat;
			curSub = transform.Find("Geo/Body_Parts/Hoodie_ForeArmR");
			skinRend = curSub.GetComponent<Renderer>();
			mat = new Material[2];
			mat[1] = BodyMaterials[body];
			mat[0] = HoodieMaterials[hoodie - 1];
			skinRend.materials = mat;

			curSub = transform.Find("Geo/Body_Parts/Hoodie_Torso");
			skinRend = curSub.GetComponent<Renderer>();
			mat = new Material[2];
			mat[1] = BodyMaterials[body];
			mat[0] = HoodieMaterials[hoodie - 1];

		skinRend.materials = mat;


			if (tanktopOld)
			{

				tanktopT.gameObject.SetActive(false);
				tanktopType = 0;
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
			//bodyToHideT.gameObject.SetActive(true);
			ShowTorso(true);
			for (int i = 0; i <= 3; i++)
			{



				curSub = tanktopT;

				skinRend = curSub.GetComponent<Renderer>();
				skinRend.material = TankTopMaterials[tanktop - 1];


				tanktopOld = true;

			}

			ShowHoodie(false);

			hoodieType = 0;

		}


		if (head == 1)
		{
			headT_A.gameObject.SetActive(false);
			headT_B.gameObject.SetActive(true);
		}
		else
		{
			headT_B.gameObject.SetActive(false);
			headT_A.gameObject.SetActive(true);
		}

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


    }

	void ShowHoodie(bool show)
    {
		transform.Find("Geo/Body_Parts/Hoodie_ArmL").gameObject.SetActive(show);
		transform.Find("Geo/Body_Parts/Hoodie_ArmR").gameObject.SetActive(show);
		transform.Find("Geo/Body_Parts/Hoodie_ForeArmL").gameObject.SetActive(show);
		transform.Find("Geo/Body_Parts/Hoodie_ForeArmR").gameObject.SetActive(show);
		transform.Find("Geo/Body_Parts/Hoodie_Torso").gameObject.SetActive(show);
	}
	void ShowTorso(bool show)
    {
		transform.Find("Geo/Body_Parts/Hips/Torso").gameObject.SetActive(show);
	}
	void OnValidate()
	{
		//code for In Editor customize
		headTyp = (int)faceType;
		bodyTyp = (int)bodyType;
		trousersTyp = (int)trousersType;
		tanktopTyp = (int)tanktopType;
		hoodieTyp = (int)hoodieType;
        eyesTyp = (int)eyesGlow;



        charCustomize(bodyTyp, trousersTyp, tanktopTyp, hoodieTyp, headTyp, eyesTyp);

	}
}