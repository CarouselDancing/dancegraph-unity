using UnityEngine;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

using Newtonsoft.Json.Linq;



// Avatar Appearance Functions
// Nametags should be in here, since they may eventually be avatar-specific




namespace DanceGraph {

    public class Randomizer {

        // private unsigned int baseseed = 1729;
        // private int mul = 1664525;
        // private int inc = 1013904223;

        private int baseseed = 1729;
        private int mul = 1664525;
        private int inc = 1013904223;

        
        private Dictionary<int, int> currentSeed = new Dictionary<int, int>();
        
        public int Next(int body) {
            int nval;
            if (currentSeed.ContainsKey(body)) {
                // in case the C# int doesn't truncate it for me
                nval = (mul * currentSeed[body] + inc) & 2147483647;
            }
            else {
                nval = baseseed * body;
            }
            currentSeed[body] = nval;
            return nval;
        }

        public int Next(int body, int max) {
            return Next(body) % max;
        }
        
    };

    
    public class AvatarAppearance {
        public virtual  string avatarResource {
            get;
            set;
        }
        public string options;
        // Parameters applied before building the avatar
        public virtual void LoadParameters(int idx) {}

        // Parameters applied after building the avatar
        public virtual void ApplyParameters(int idx, GameObject gObj) {}
    };

    public class AvatarYBot : AvatarAppearance {

        cfg.YBotConfig config;
        
        public override string avatarResource {
            get { return "yBotAvatar"; }
            set {}
        }
        
        public static Color YBotGetColorByIndex(int idx, bool joints)
        {
            Color[] cvals = {
                Color.blue,
                Color.red,
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.magenta,
                Color.white,
                Color.gray};
            
            if (joints == false)
                return cvals[idx%cvals.Length];
            return cvals[(idx * 5 + 3)%cvals.Length];
        }

        public override void LoadParameters(int idx) {
            if (options != null)
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<cfg.YBotConfig>(options);
            else
                config = null;
        }
        
        public override void ApplyParameters(int idx, GameObject gObj) {
            Debug.Log($"Applying YBot colours to avatar {idx}");

            var matList = gObj.GetComponentsInChildren<Renderer>();

            Color c1;
            Color c2;            
            if ((config is null) || (config.colour1 is null))
                c1 = YBotGetColorByIndex(idx, true);
            else
                c1 = new Color(config.colour1[0],config.colour1[1],config.colour1[2]);
            
            if ((config is null) || (config.colour2 is null))
                c2 = YBotGetColorByIndex(idx, false);
            else
                c2 = new Color(config.colour2[0],config.colour2[1],config.colour2[2]);

            foreach(Renderer r in matList) {
                if (r.material.name.Contains("Joints"))
                    r.material.color = c1;
                else
                    r.material.color = c2;
            }
        }

    }            

	public class MagipeepSkinner {

		protected string model;
		protected string titleCaseModel;
		protected int variety;

		protected Material BodyMat;
		protected Material SkinMat;
		protected Material EyesMat;
		protected Material HairMat;
		
		public string ResString(string resType) {
                    return $"MagipeepMaterials/{model}/{titleCaseModel}{variety}/{titleCaseModel}{variety}{resType}Mat";

		}

		// Unaltered means the caller is passing the full resource path
		public Material GetRes(string resType, bool unaltered = false) {
			string rstr;
			if (unaltered)
				rstr = resType;
			else
				rstr = ResString(resType);
			
			Material m = Resources.Load(rstr, typeof(Material)) as Material;
			if (m is null) {
				Debug.LogWarning($"Material {rstr} not loaded!");
			}
			return m;
		}
		
		public virtual void GetMaterials(string mod, int v) {
			model = mod;
			variety = v;
			titleCaseModel = String.Join("", new String [] {model.Substring(0, 1), model.Substring(1).ToLower()});

			BodyMat = GetRes("Body");			
			SkinMat = GetRes("Skin");					
			EyesMat = GetRes("Eyes");			
			HairMat = GetRes("Hair");
		}

		public virtual void ApplyMaterials(GameObject gObj) {

			Debug.Log("Applying Generic Materials");
			GameObject bodyObj = gObj.transform.Find($"{titleCaseModel}Body").gameObject;
			if (bodyObj is null)
				Debug.LogWarning($"Can't find body for {titleCaseModel}");
			SkinnedMeshRenderer bodyMesh = bodyObj.GetComponent<SkinnedMeshRenderer>();
			
			if (bodyMesh is null)
				Debug.LogWarning($"Can't find mesh for {titleCaseModel}");
			bodyMesh.materials = new Material [] {SkinMat, BodyMat, EyesMat};
			
			GameObject hairObj = gObj.transform.Find($"{titleCaseModel}Hair").gameObject;
			SkinnedMeshRenderer hairMesh = hairObj.GetComponent<SkinnedMeshRenderer>();			
			hairMesh.materials = new Material [] {HairMat};
		}
		
	};

	public class KaraSkinner : MagipeepSkinner {
		
		protected Material HairbandMat;
		protected Material HeadbandMat;		
		
		public override void GetMaterials(string mod, int v) {
			base.GetMaterials(mod, v);

			HairbandMat = GetRes("Hairband");
			string headstr =  $"MagipeepMaterials/{model}/{titleCaseModel}HeadbandMat";
			HeadbandMat = GetRes(headstr, true);
		}

		public override void ApplyMaterials(GameObject gObj) {

			base.ApplyMaterials(gObj);
			
			GameObject flowerObj = gObj.transform.Find($"KaraFlowers").gameObject;
			SkinnedMeshRenderer flowerMesh = flowerObj.GetComponent<SkinnedMeshRenderer>();			
			flowerMesh.materials = new Material [] {HeadbandMat};

			GameObject headbandObj = gObj.transform.Find($"karaHeadBand").gameObject;
			SkinnedMeshRenderer headbandMesh = headbandObj.GetComponent<SkinnedMeshRenderer>();			
			headbandMesh.materials = new Material [] {HeadbandMat};

			GameObject hairObj = gObj.transform.Find($"KaraHair").gameObject;
			SkinnedMeshRenderer hairMesh = hairObj.GetComponent<SkinnedMeshRenderer>();			
			hairMesh.materials = new Material [] {HairMat, HairbandMat};

		}
		
	};

	public class QuinSkinner : MagipeepSkinner {

		Material HelmetMat;
		string [] QuinBits = new string [] {"HelmetBase", "HelmetRing", "HelmetWings", "Pouch"};
		
		public override void GetMaterials(string mod, int v) {
			base.GetMaterials(mod, v);
			HelmetMat = GetRes($"MagipeepMaterials/{model}/QuinHelmetAndPouchMat", true);			
		}

		public override void ApplyMaterials(GameObject gObj) {
			base.ApplyMaterials(gObj);

			foreach (string bit in QuinBits) {
				GameObject helmetObj = gObj.transform.Find($"{titleCaseModel}{bit}").gameObject;
				SkinnedMeshRenderer helmetMesh = helmetObj.GetComponent<SkinnedMeshRenderer>();			
				helmetMesh.materials = new Material [] {HelmetMat};

			}
		}
	};
	
	public class UnaSkinner : MagipeepSkinner {
		Material HairbandMat;
		
		public override void GetMaterials(string mod, int v) {
			base.GetMaterials(mod, v);

			string headstr =  $"MagipeepMaterials/{model}/UnaHairbandMat";
			HairbandMat = GetRes(headstr, true);
			/*
			string tiarastr =  $"MagipeepMaterials/{model}/{titleCaseModel}{variety}/UnaHairbandMat";
			TiaraMat = GetRes(tiarastr, true);
			*/
			
		}
		public override void ApplyMaterials(GameObject gObj) {
			Debug.Log("Applying Una Materials");
			base.ApplyMaterials(gObj);

			GameObject hairObj = gObj.transform.Find($"{titleCaseModel}Hair").gameObject;
			SkinnedMeshRenderer hairMesh = hairObj.GetComponent<SkinnedMeshRenderer>();			
			hairMesh.materials = new Material [] {HairbandMat, HairMat};
		}

	};
	
	
   public class AvatarMagipeep : AvatarAppearance {
        private static readonly string [] MagipeepModels = new string [] {
            "KARA", "QUIN", "NIAL", "UNA", "FIN", "NIAMH",
        };

        int materialCount = 8;

        public string model;
        public int materialIdx;

        //private static System.Random rng = new System.Random();
        private Randomizer rng = new Randomizer();
        
        public override string avatarResource {
            get {
                string resourcefile = string.Format("LODAvatars/{0}Avatar", model);
                return resourcefile;
            }
            set {}
        }
        
        public override void LoadParameters(int idx) {
            cfg.MagipeepConfig config;

            config = Newtonsoft.Json.JsonConvert.DeserializeObject<cfg.MagipeepConfig>(options);

            if (config is null) {
                model = MagipeepModels[rng.Next(idx, MagipeepModels.Length)];
                materialIdx = 1 + rng.Next(idx, materialCount);
            }

            else {
                if (config.model is null) {
                    model = MagipeepModels[rng.Next(idx, MagipeepModels.Length)];
                }
                else {
                    model = config.model;
                }
                if (config.materialIdx == 0) {
                    materialIdx = 1 + rng.Next(idx, materialCount);
                }
                else {
                    materialIdx = config.materialIdx;
                }
            }
        }

        public static void CheckMaterial(Material m, string mstr) {
            if (m is null) {
                Debug.LogWarning($"Material {mstr} not loaded");
            }
        }
	
        public override void ApplyParameters(int idx, GameObject gObj) {
            
            MagipeepSkinner mSkinner;
            switch(model) {
                case "KARA":
                    mSkinner = new KaraSkinner(); break;
                case "QUIN":
                    mSkinner = new QuinSkinner(); break;
                case "UNA":
                    mSkinner = new UnaSkinner(); break;
                default:
                    mSkinner = new MagipeepSkinner(); break;
            }
            mSkinner.GetMaterials(model, materialIdx);
            mSkinner.ApplyMaterials(gObj);
        }
   };
   
    public class AvatarMain : AvatarAppearance {
        // For the random fallback
        private static readonly string [] MainRaces  = new string [] {
            "AIAN", "ASIAN", "BLACK", "HISPANIC", "MENA", "NHPI", "WHITE"
        };
        
        private static readonly string [] MainGenders = new string [] {"M", "F"};
        private static System.Random rng = new System.Random();
        
        public string race;
        public string gender;
        public int model;
        public float shape;
        
        public override string avatarResource {
            get {
                string resourcefile = string.Format("{0}_{1}{2}Avatar", race, gender, model);
                return resourcefile;
            }
            set {}
        }

        public override void LoadParameters(int idx) {
            cfg.MainAvatarConfig config;
            
            if (options != null) {
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<cfg.MainAvatarConfig>(options);
                Debug.Log("Loaded Config from non-null options");
            }
            else
                config = null;

            if (config is null) {
                // We're not going to declare a 'default' race or gender.
                // If we can't work it out from the config, you're getting a random one

                race = MainRaces[rng.Next(MainRaces.Length)];
                gender = MainGenders[rng.Next(MainGenders.Length)];
                model = 1 + rng.Next()%2;
                Debug.Log($"Main Avatar fallback - generating {race}/{gender}/{model}");
                
            }
            else {
                if (config.race is null)
                    race = MainRaces[rng.Next(MainRaces.Length)];
                else 
                    race = config.race;

                if (config.gender is null) 
                    gender = MainGenders[rng.Next(MainGenders.Length)];
                else
                    gender = config.gender;

                if (config.model == 0)
                    model = 1 + rng.Next()%2;
                else 
                    model = config.model;

                if ((config.shape < 0) || (config.shape > 100)) 
                    shape = UnityEngine.Random.Range(0f, 100f);
                else
                    shape = config.shape;
                
                Debug.Log($"Main Avatar config - using {race}/{gender}/{model}/{shape}"); 
            }
        }
        
        public override void ApplyParameters(int idx, GameObject gObject) {
            //Extra piece of code by Samantha to attempt adding a random Blendshape value to the avatar for different body shapes.
            //index value 96 should point to the smallToLarge blendshape on each of the avatar meshes (assuming that indexes start of 0 so that the 97th blendshape would have and index of 96)
            SkinnedMeshRenderer skinnedMesh;

            skinnedMesh = gObject.GetComponentInChildren<SkinnedMeshRenderer>();
            String name = "smallToLarge"; //The name of Blendshape that contains the body shape change
            int blendIndex = 96; //If this is wrong feel free to change; I assumed this would be 96 as indexes in arrays usually start on 0
            Debug.Log($"Setting Blend Shape index {blendIndex} to {shape}");
            skinnedMesh.SetBlendShapeWeight(blendIndex, shape);
        }
    }


    
    public class AvatarLouise : AvatarAppearance {

        public override string avatarResource {
            get { return "LouisetestCombined"; }
            set {}
        }

        
        public override void LoadParameters(int idx) {
            cfg.LouiseConfig config;
            Debug.Log("Unimplemented Louise Avatar Parameter PreApplication");
        }
        
        public override void ApplyParameters(int idx, GameObject gObj) {
            cfg.LouiseConfig config;
            Debug.Log("Unimplemented Louise Avatar Parameter PostApplication");
        }
        
    };

    public class Avatar {

        public GameObject gObject;
        public String type;
        public AvatarAppearance avAppearance; 
        
        public GameObject LoadAvatar(int idx, EnvUserState userState) {
            // Loads avatar from the environment user settings
            // If this isn't available, switch to a sensible fallback
            Debug.Log($"Getting user of type {userState._avatarType}");
            switch(userState._avatarType) {
                case "Louise":
                    avAppearance = new AvatarLouise();
                    break;
                case "yBot":
                    avAppearance = new AvatarYBot();
                    break;
                case "Main" :
                    avAppearance = new AvatarMain();
                    break;
                case "Magipeep" :
                    avAppearance = new AvatarMagipeep();
                    break;
                default:
                    //avAppearance = new AvatarYBot();
                    avAppearance = new AvatarYBot();                                        
                    Debug.LogWarning($"Unknown avatar: {userState._avatarType}, defaulting to yBot");
                    break;
            }
            avAppearance.options = userState.avatarParams;     
            avAppearance.LoadParameters(idx);

            Debug.Log($"Avatar {idx} being loaded from resource {avAppearance.avatarResource}");
            if (avAppearance is null)
                Debug.Log("AvAppearance is strangely null");

            gObject = (GameObject) Resources.Load(avAppearance.avatarResource);
            return gObject;
        }

        public GameObject ReloadAvatar(int idx, EnvUserState userState) {
            Debug.Log("Instructed to reload the avatar");

            gObject = LoadAvatar(idx, userState);
            return gObject;
        }
    }
}
