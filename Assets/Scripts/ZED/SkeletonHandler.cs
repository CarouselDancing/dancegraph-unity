//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

using TMPro;

public class SkeletonHandler : ScriptableObject
{

    public enum BODY_FORMAT {
        BODY_34,
        BODY_38,
        BODY_34_KEYPOINTS,
        BODY_38_KEYPOINTS,
        BODY_34_KEYPOINTSPLUS,
        BODY_38_KEYPOINTSPLUS,        
    };
    
    
    // Initial tpose positions
    public static readonly Vector3[] tpose_34 = new Vector3[] {
        
        new Vector3( 0.000000f,  0.966564f,   0.000000f),
        new Vector3( 0.000000f,  1.065799f,  -0.012273f),
        new Vector3( 0.000000f,  1.315855f,  -0.042761f),
        new Vector3( 0.000000f,  1.466180f,  -0.034832f),
        new Vector3( -0.061058f, 1.406959f,  -0.035706f),
        new Vector3( -0.187608f, 1.404300f,  -0.061715f),
        new Vector3( -0.461655f, 1.404300f,  -0.061715f),
        new Vector3( -0.737800f, 1.404300f,  -0.061715f),
        Vector3.zero,
        Vector3.zero,       
        new Vector3( -0.856083f, 1.341810f,  -0.017511f),
        new Vector3( 0.061057f,  1.406960f,  -0.035706f),
        new Vector3( 0.187608f,  1.404300f,  -0.061715f),
        new Vector3( 0.461654f,  1.404301f,  -0.061715f),
        new Vector3( 0.737799f,  1.404301f,  -0.061714f),
        Vector3.zero,
        Vector3.zero,       
        new Vector3( 0.856082f,  1.341811f,  -0.017511f),
        new Vector3( -0.091245f, 0.900000f,  -0.000554f),
        new Vector3( -0.093691f, 0.494046f,  -0.005710f),
        new Vector3( -0.091244f, 0.073566f,  -0.026302f),
        new Vector3( -0.094977f, -0.031356f,  0.100105f),
        new Vector3( 0.091245f,  0.900000f,  -0.000554f),
        new Vector3( 0.093692f,  0.494046f,  -0.005697f),
        new Vector3( 0.091245f,  0.073567f,  -0.026302f),
        new Vector3( 0.094979f,  -0.031355f,  0.100105f),
        new Vector3( 0.000000f,  1.569398f,  -0.003408f),
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,
        Vector3.zero,        
    };
    #region const_variables
    // For Skeleton Display
    public const int
        // --------- Common
        JointType_PELVIS = 0,
        JointType_SPINE_1 = 1,
        JointType_SPINE_2 = 2,
        JointType_SPINE_3 = 3,
        JointType_NECK = 4,
        JointType_NOSE = 5,
        JointType_LEFT_EYE = 6,
        JointType_RIGHT_EYE = 7,
        JointType_LEFT_EAR = 8,
        JointType_RIGHT_EAR = 9,
        JointType_LEFT_CLAVICLE = 10,
        JointType_RIGHT_CLAVICLE = 11,
        JointType_LEFT_SHOULDER = 12,
        JointType_RIGHT_SHOULDER = 13,
        JointType_LEFT_ELBOW = 14,
        JointType_RIGHT_ELBOW = 15,
        JointType_LEFT_WRIST = 16,
        JointType_RIGHT_WRIST = 17,
        JointType_LEFT_HIP = 18,
        JointType_RIGHT_HIP = 19,
        JointType_LEFT_KNEE = 20,
        JointType_RIGHT_KNEE = 21,
        JointType_LEFT_ANKLE = 22,
        JointType_RIGHT_ANKLE = 23,
        JointType_LEFT_BIG_TOE = 24,
        JointType_RIGHT_BIG_TOE = 25,
        JointType_LEFT_SMALL_TOE = 26,
        JointType_RIGHT_SMALL_TOE = 27,
        JointType_LEFT_HEEL = 28,
        JointType_RIGHT_HEEL = 29,
        // --------- Body 38 specific
        JointType_38_LEFT_HAND_THUMB_4 = 30, // tip
        JointType_38_RIGHT_HAND_THUMB_4 = 31,
        JointType_38_LEFT_HAND_INDEX_1 = 32, // knuckle
        JointType_38_RIGHT_HAND_INDEX_1 = 33,
        JointType_38_LEFT_HAND_MIDDLE_4 = 34, // tip
        JointType_38_RIGHT_HAND_MIDDLE_4 = 35,
        JointType_38_LEFT_HAND_PINKY_1 = 36, // knuckle
        JointType_38_RIGHT_HAND_PINKY_1 = 37,
        JointType_38_COUNT = 38,
        // --------- Body34
        JointType_34_Head = 26,
        JointType_34_Neck = 3,
        JointType_34_ClavicleRight = 11,
        JointType_34_ShoulderRight = 12,
        JointType_34_ElbowRight = 13,
        JointType_34_WristRight = 14,
        JointType_34_ClavicleLeft = 4,
        JointType_34_ShoulderLeft = 5,
        JointType_34_ElbowLeft = 6,
        JointType_34_WristLeft = 7,
        JointType_34_HipRight = 22,
        JointType_34_KneeRight = 23,
        JointType_34_AnkleRight = 24,
        JointType_34_FootRight = 25,
        JointType_34_HeelRight = 33,
        JointType_34_HipLeft = 18,
        JointType_34_KneeLeft = 19,
        JointType_34_AnkleLeft = 20,
        JointType_34_FootLeft = 21,
        JointType_34_HeelLeft = 32,
        JointType_34_EyesRight = 30,
        JointType_34_EyesLeft = 28,
        JointType_34_EarRight = 31,
        JointType_34_EarLeft = 29,
        JointType_34_SpineBase = 0,
        JointType_34_SpineNaval = 1,
        JointType_34_SpineChest = 2,
        JointType_34_Nose = 27,
        jointType_34_COUNT = 34;

    // List of bones (pair of joints) for BODY_38. Used for Skeleton mode.
    private static readonly int[] bonesList38 = new int[] {
    // Torso
    JointType_PELVIS, JointType_SPINE_1,
    JointType_SPINE_1, JointType_SPINE_2,
    JointType_SPINE_2, JointType_SPINE_3,
    JointType_SPINE_3, JointType_NECK,
    JointType_PELVIS, JointType_LEFT_HIP,
    JointType_PELVIS, JointType_RIGHT_HIP,
    JointType_NECK, JointType_NOSE,
    JointType_NECK, JointType_LEFT_CLAVICLE,
    JointType_LEFT_CLAVICLE, JointType_LEFT_SHOULDER,
    JointType_NECK, JointType_RIGHT_CLAVICLE,
    JointType_RIGHT_CLAVICLE, JointType_RIGHT_SHOULDER,
    JointType_NOSE, JointType_LEFT_EYE,
    JointType_LEFT_EYE, JointType_LEFT_EAR,
    JointType_NOSE, JointType_RIGHT_EYE,
    JointType_RIGHT_EYE, JointType_RIGHT_EAR,
    // Left arm
    JointType_LEFT_SHOULDER, JointType_LEFT_ELBOW,
    JointType_LEFT_ELBOW, JointType_LEFT_WRIST,
    JointType_LEFT_WRIST, JointType_38_LEFT_HAND_THUMB_4, // -
    JointType_LEFT_WRIST, JointType_38_LEFT_HAND_INDEX_1,
    JointType_LEFT_WRIST, JointType_38_LEFT_HAND_MIDDLE_4,
    JointType_LEFT_WRIST, JointType_38_LEFT_HAND_PINKY_1, // -
    // right arm
    JointType_RIGHT_SHOULDER, JointType_RIGHT_ELBOW,
    JointType_RIGHT_ELBOW, JointType_RIGHT_WRIST,
    JointType_RIGHT_WRIST, JointType_38_RIGHT_HAND_THUMB_4, // -
    JointType_RIGHT_WRIST, JointType_38_RIGHT_HAND_INDEX_1,
    JointType_RIGHT_WRIST, JointType_38_RIGHT_HAND_MIDDLE_4,
    JointType_RIGHT_WRIST, JointType_38_RIGHT_HAND_PINKY_1, // -
    // legs
    JointType_LEFT_HIP, JointType_LEFT_KNEE,
    JointType_LEFT_KNEE, JointType_LEFT_ANKLE,
    JointType_LEFT_ANKLE, JointType_LEFT_HEEL,
    JointType_LEFT_ANKLE, JointType_LEFT_BIG_TOE,
    JointType_LEFT_ANKLE, JointType_LEFT_SMALL_TOE,
    JointType_RIGHT_HIP, JointType_RIGHT_KNEE,
    JointType_RIGHT_KNEE, JointType_RIGHT_ANKLE,
    JointType_RIGHT_ANKLE, JointType_RIGHT_HEEL,
    JointType_RIGHT_ANKLE, JointType_RIGHT_BIG_TOE,
    JointType_RIGHT_ANKLE, JointType_RIGHT_SMALL_TOE
    };

    // List of bones (pair of joints) for BODY_34. Used for Skeleton mode.
    private static readonly int[] bonesList34 = new int[] {
    // Torso
        JointType_34_SpineBase, JointType_34_HipRight,
        JointType_34_HipLeft, JointType_34_SpineBase,
        JointType_34_SpineBase, JointType_34_SpineNaval,
        JointType_34_SpineNaval, JointType_34_SpineChest,
        JointType_34_SpineChest, JointType_34_Neck,
        JointType_34_EarRight, JointType_34_EyesRight,
        JointType_34_EarLeft, JointType_34_EyesLeft,
        JointType_34_EyesRight, JointType_34_Nose,
        JointType_34_EyesLeft, JointType_34_Nose,
        JointType_34_Nose, JointType_34_Neck,
    // left
        JointType_34_SpineChest, JointType_34_ClavicleLeft,
        JointType_34_ClavicleLeft, JointType_34_ShoulderLeft,
        JointType_34_ShoulderLeft, JointType_34_ElbowLeft,         // LeftUpperArm
        JointType_34_ElbowLeft, JointType_34_WristLeft,            // LeftLowerArm
        JointType_34_HipLeft, JointType_34_KneeLeft,               // LeftUpperLeg
        JointType_34_KneeLeft, JointType_34_AnkleLeft,             // LeftLowerLeg6
        JointType_34_AnkleLeft, JointType_34_FootLeft,
        JointType_34_AnkleLeft, JointType_34_HeelLeft,
        JointType_34_FootLeft, JointType_34_HeelLeft,
    // right
        JointType_34_SpineChest, JointType_34_ClavicleRight,
        JointType_34_ClavicleRight, JointType_34_ShoulderRight,
        JointType_34_ShoulderRight, JointType_34_ElbowRight,       // RightUpperArm
        JointType_34_ElbowRight, JointType_34_WristRight,          // RightLowerArm
        JointType_34_HipRight, JointType_34_KneeRight,             // RightUpperLeg
        JointType_34_KneeRight, JointType_34_AnkleRight,           // RightLowerLeg
        JointType_34_AnkleRight, JointType_34_FootRight,
        JointType_34_AnkleRight, JointType_34_HeelRight,
        JointType_34_FootRight, JointType_34_HeelRight
    };

    /*
        _boneNameToJointIndex.Add("Pelvis", 0);
        _boneNameToJointIndex.Add("L_Hip", 1);
        _boneNameToJointIndex.Add("R_Hip", 2);
        _boneNameToJointIndex.Add("Spine1", 3);
        _boneNameToJointIndex.Add("L_Knee", 4);
        _boneNameToJointIndex.Add("R_Knee", 5);
        _boneNameToJointIndex.Add("Spine2", 6);
        _boneNameToJointIndex.Add("L_Ankle", 7);
        _boneNameToJointIndex.Add("R_Ankle", 8);
        _boneNameToJointIndex.Add("Spine3", 9);
        _boneNameToJointIndex.Add("L_Foot", 10);
        _boneNameToJointIndex.Add("R_Foot", 11);
        _boneNameToJointIndex.Add("Neck", 12);
        _boneNameToJointIndex.Add("L_Collar", 13);
        _boneNameToJointIndex.Add("R_Collar", 14);
        _boneNameToJointIndex.Add("Head", 15);
        _boneNameToJointIndex.Add("L_Shoulder", 16);
        _boneNameToJointIndex.Add("R_Shoulder", 17);
        _boneNameToJointIndex.Add("L_Elbow", 18);
        _boneNameToJointIndex.Add("R_Elbow", 19);
        _boneNameToJointIndex.Add("L_Wrist", 20);
        _boneNameToJointIndex.Add("R_Wrist", 21);
        _boneNameToJointIndex.Add("L_Hand", 22);
        _boneNameToJointIndex.Add("R_Hand", 23);
    */

    
    // Indexes of bones' parents for BODY_38 
    private static readonly int[] parentsIdx_38 = new int[]
    {
        -1,
        0,
        1,
        2,
        3,
        4,
        4,
        4,
        4,
        4,
        3,
        3,
        10,
        11,
        12,
        13,
        14,
        15,
        0,
        0,
        18,
        19,
        20,
        21,
        22,
        23,
        22,
        23,
        22,
        23,
        16,
        17,
        16,
        17,
        16,
        17,
        16,
        17
    };

    // Indexes of bones' parents for BODY_34 
    private static readonly int[] parentsIdx_34 = new int[]
    {
        -1,
        0,
        1,
        2,
        2,
        4,
        5,
        6,
        7,
        8,
        7,
        2,
        11,
        12,
        13,
        14,
        15,
        14,
        0,
        18,
        19,
        20,
        0,
        22,
        23,
        24,
        3,
        26,
        26,
        26,
        26,
        26,
        20,
        24
        
    };
    // Bones output by the ZED SDK (in this order)
    private static HumanBodyBones[] humanBones38 = new HumanBodyBones[] {
    HumanBodyBones.Hips,
    HumanBodyBones.Spine,
    HumanBodyBones.Chest,
    HumanBodyBones.UpperChest,
    HumanBodyBones.Neck,
    HumanBodyBones.LastBone, // Nose
    HumanBodyBones.LastBone, // Left Eye
    HumanBodyBones.LastBone, // Right Eye
    HumanBodyBones.LastBone, // Left Ear
    HumanBodyBones.LastBone, // Right Ear
    HumanBodyBones.LeftShoulder,
    HumanBodyBones.RightShoulder,
    HumanBodyBones.LeftUpperArm,
    HumanBodyBones.RightUpperArm,
    HumanBodyBones.LeftLowerArm,
    HumanBodyBones.RightLowerArm,
    HumanBodyBones.LeftHand, // Left Wrist
    HumanBodyBones.RightHand, // Left Wrist
    HumanBodyBones.LeftUpperLeg, // Left Hip
    HumanBodyBones.RightUpperLeg, // Right Hip
    HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.RightFoot,
        HumanBodyBones.LastBone, // Left Big Toe
    HumanBodyBones.LastBone, // Right Big Toe
    HumanBodyBones.LastBone, // Left Small Toe
    HumanBodyBones.LastBone, // Right Small Toe
    HumanBodyBones.LastBone, // Left Heel
    HumanBodyBones.LastBone, // Right Heel
    // Hands
    HumanBodyBones.LastBone, // Left Hand Thumb Tip
    HumanBodyBones.LastBone, // Right Hand Thumb Tip
    HumanBodyBones.LastBone, // Left Hand Index Knuckle
    HumanBodyBones.LastBone, // Right Hand Index Knuckle
    HumanBodyBones.LastBone, // Left Hand Middle Tip
    HumanBodyBones.LastBone, // Right Hand Middle Tip
    HumanBodyBones.LastBone, // Left Hand Pinky Knuckle
    HumanBodyBones.LastBone, // Right Hand Pinky Knuckle
    HumanBodyBones.LastBone // Last
    };

    // Bones output by the ZED SDK (in this order)
    private static HumanBodyBones[] humanBones34 = new HumanBodyBones[] {
    HumanBodyBones.Hips,
    HumanBodyBones.Spine,
    HumanBodyBones.UpperChest,
    HumanBodyBones.Neck,
    HumanBodyBones.LeftShoulder,
    HumanBodyBones.LeftUpperArm,
    HumanBodyBones.LeftLowerArm,
    HumanBodyBones.LeftHand, // Left Wrist
    HumanBodyBones.LastBone, // Left Hand
    HumanBodyBones.LastBone, // Left HandTip
    HumanBodyBones.LastBone,
    HumanBodyBones.RightShoulder,
    HumanBodyBones.RightUpperArm,
    HumanBodyBones.RightLowerArm,
    HumanBodyBones.RightHand, // Right Wrist
    HumanBodyBones.LastBone, // Right Hand
    HumanBodyBones.LastBone, // Right HandTip
    HumanBodyBones.LastBone,
    HumanBodyBones.LeftUpperLeg,
    HumanBodyBones.LeftLowerLeg,
    HumanBodyBones.LeftFoot,
    HumanBodyBones.LeftToes,
    HumanBodyBones.RightUpperLeg,
    HumanBodyBones.RightLowerLeg,
    HumanBodyBones.RightFoot,
    HumanBodyBones.RightToes,
    HumanBodyBones.Head,
    HumanBodyBones.LastBone, // Nose
    HumanBodyBones.LastBone, // Left Eye
    HumanBodyBones.LastBone, // Left Ear
    HumanBodyBones.LastBone, // Right Eye
    HumanBodyBones.LastBone, // Right Ear
    HumanBodyBones.LastBone, // Left Heel
    HumanBodyBones.LastBone, // Right Heel
    };
    #endregion

    private static int[] poseBoneList34 = new int[] {
    Array.IndexOf(humanBones34,HumanBodyBones.Hips),
    Array.IndexOf(humanBones34,HumanBodyBones.UpperChest),
    Array.IndexOf(humanBones34,HumanBodyBones.RightShoulder),
    Array.IndexOf(humanBones34,HumanBodyBones.RightUpperArm),
    Array.IndexOf(humanBones34,HumanBodyBones.RightLowerArm),
    Array.IndexOf(humanBones34,HumanBodyBones.RightHand),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftShoulder),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftUpperArm),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftLowerArm),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftHand),
    Array.IndexOf(humanBones34,HumanBodyBones.Neck),
    Array.IndexOf(humanBones34,HumanBodyBones.Head),
    Array.IndexOf(humanBones34,HumanBodyBones.RightUpperLeg),
    Array.IndexOf(humanBones34,HumanBodyBones.RightLowerLeg),
    Array.IndexOf(humanBones34,HumanBodyBones.RightFoot),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftUpperLeg),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftLowerLeg),
    Array.IndexOf(humanBones34,HumanBodyBones.LeftFoot)
    };


    private static int [] poseBoneList38 = new int[] {
        Array.IndexOf(humanBones38,HumanBodyBones.Hips),
        Array.IndexOf(humanBones38,HumanBodyBones.Spine),
        Array.IndexOf(humanBones38,HumanBodyBones.UpperChest),
        Array.IndexOf(humanBones38,HumanBodyBones.Neck),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftShoulder),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftUpperArm),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftLowerArm),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftHand), // Left Wrist
        Array.IndexOf(humanBones38,HumanBodyBones.RightShoulder),
        Array.IndexOf(humanBones38,HumanBodyBones.RightUpperArm),
        Array.IndexOf(humanBones38,HumanBodyBones.RightLowerArm),
        Array.IndexOf(humanBones38,HumanBodyBones.RightHand), // Right Wrist
        Array.IndexOf(humanBones38,HumanBodyBones.LeftUpperLeg),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftLowerLeg),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftFoot),
        Array.IndexOf(humanBones38,HumanBodyBones.LeftToes),
        Array.IndexOf(humanBones38,HumanBodyBones.RightUpperLeg),
        Array.IndexOf(humanBones38,HumanBodyBones.RightLowerLeg),
        Array.IndexOf(humanBones38,HumanBodyBones.RightFoot),
        Array.IndexOf(humanBones38,HumanBodyBones.RightToes),
        Array.IndexOf(humanBones38,HumanBodyBones.Head)
    };
        
    #region vars

    // public Vector3[] joints34 = new Vector3[jointType_34_COUNT];
    // public Vector3[] joints38 = new Vector3[JointType_38_COUNT];

    private GameObject humanoid;

    private float heightOffset = 0.0f;

    private Vector3 targetBodyPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Quaternion targetBodyOrientation = Quaternion.identity;

    private RigBone[] rigBoneIdx;
    private Quaternion[] rigBoneTargetIdx;

    private Vector3 [] rigBoneTargetPos;
    
    private Quaternion[] default_RotationsIdx;

    public BODY_FORMAT currentBodyFormat;

    public BODY_FORMAT BodyFormat
    {
        get
        {
            return currentBodyFormat;
        }
        set
        {
            currentBodyFormat = value;
            UpdateCurrentValues(currentBodyFormat);
        }
    }

    private Animator animator;

    public HumanBodyBones[] currentHumanBodyBones;

    public GameObject nametag;


    public GameObject [] testAvatarJoints;
    // Test link between joints
    public GameObject [] testAvatarBones;
    public Vector3 [] smplBones;

    public const float nameTagBaseHeight = 0.8f;

    public float nameTagHeight;

    
    #endregion

    private void UpdateCurrentValues(BODY_FORMAT pBodyFormat)
    {
        switch (pBodyFormat)
        {
            case BODY_FORMAT.BODY_34:

                rigBoneIdx = new RigBone[34];
                rigBoneTargetIdx = new Quaternion[34];
                default_RotationsIdx = new Quaternion[34];

                break;
            case BODY_FORMAT.BODY_38:

                rigBoneIdx = new RigBone[38];
                rigBoneTargetIdx = new Quaternion[38];
                default_RotationsIdx = new Quaternion[38];
                break;

            case BODY_FORMAT.BODY_34_KEYPOINTS:
                rigBoneIdx = new RigBone[34];
                rigBoneTargetPos = new Vector3[34];

                // Do we need these?
                rigBoneTargetIdx = new Quaternion[34];
                default_RotationsIdx = new Quaternion[34];
                
                break;
                
            case BODY_FORMAT.BODY_38_KEYPOINTS:
                rigBoneIdx = new RigBone[38];
                rigBoneTargetPos = new Vector3[38];

                // Do we need these?
                rigBoneTargetIdx = new Quaternion[38];
                default_RotationsIdx = new Quaternion[38];
                
                break;
                
            case BODY_FORMAT.BODY_34_KEYPOINTSPLUS:
                rigBoneIdx = new RigBone[34];
                rigBoneTargetPos = new Vector3[34];

                // Do we need these?
                rigBoneTargetIdx = new Quaternion[34];
                default_RotationsIdx = new Quaternion[34];
                
                break;
                
            case BODY_FORMAT.BODY_38_KEYPOINTSPLUS:
                rigBoneIdx = new RigBone[38];
                rigBoneTargetPos = new Vector3[38];

                // Do we need these?
                rigBoneTargetIdx = new Quaternion[38];
                default_RotationsIdx = new Quaternion[38];
                
                break;
            default:
                Debug.LogError("Error: Invalid BODY_MODEL!");
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
                break;
        }
    }


    public GameObject DanceGraphCreate(GameObject h, BODY_FORMAT body_format)
    {
        humanoid = (GameObject)Instantiate(h, Vector3.zero, Quaternion.identity);
        animator = humanoid.GetComponent<Animator>();
        BodyFormat = body_format;

        // Init list of bones that will be updated by the data retrieved from the ZED SDK

        HumanBodyBones[] humanBone;

        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38):
            case (BODY_FORMAT.BODY_38_KEYPOINTS):
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):                
                humanBone = humanBones38;            
                break;
            case (BODY_FORMAT.BODY_34):
            case (BODY_FORMAT.BODY_34_KEYPOINTS):
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):                                
            default:
                humanBone = humanBones34;
                break;
        }

        smplBones = new Vector3[humanBone.Length];
        Debug.Log($"RBTI.Length is {rigBoneTargetIdx.Length}");
        for (int i = 0; i < rigBoneTargetIdx.Length; i++)
        {
            if (humanBone[i] != HumanBodyBones.LastBone)
            {
                rigBoneIdx[i] = new RigBone(humanoid, humanBone[i]);
                
                if (h.GetComponent<Animator>())
                {
                    default_RotationsIdx[i] = humanoid.GetComponent<Animator>().GetBoneTransform(humanBone[i]).localRotation;
                }
            }
            rigBoneTargetIdx[i] = Quaternion.identity;
        }


        /*
        if (BodyFormat == BODY_FORMAT.BODY_38_KEYPOINTS) {
            IKControl ikc = humanoid.GetComponent<IKControl>();
            ikc.rightHandObj = rigBoneIdx[JointType_38_RIGHT_HAND_INDEX_1].transform;
            ikc.leftHandObj = rigBoneIdx[JointType_38_LEFT_HAND_INDEX_1].transform;
            ikc.rightFootObj = rigBoneIdx[JointType_RIGHT_ANKLE].transform;
            ikc.leftFootObj = rigBoneIdx[JointType_LEFT_ANKLE].transform;            
        }
        
        if (BodyFormat == BODY_FORMAT.BODY_34_KEYPOINTS) {
            IKControl ikc = humanoid.GetComponent<IKControl>();            
            ikc.rightHandObj = rigBoneIdx[JointType_34_WristRight].transform;
            ikc.leftHandObj = rigBoneIdx[JointType_34_WristLeft].transform;
            ikc.rightFootObj = rigBoneIdx[JointType_RIGHT_ANKLE].transform;
            ikc.leftFootObj = rigBoneIdx[JointType_LEFT_ANKLE].transform;            
        }
        */
        return humanoid;
    }



    public void ChangeNameTag(GameObject go)
    {
        nametag.GetComponent<TextMeshPro>().text = go.name;
        Transform ntfr = nametag.transform.Find("ReverseNametag");
        ntfr.GetComponent<TextMeshPro>().text = go.name;

        Debug.Log($"Changenametag via gameobject; name set to {go.name}");
    }

    public void ChangeNameTag(String s)
    {
        TextMeshPro tm = nametag.GetComponent<TextMeshPro>();
        tm.text = s;
        Transform ntfr = nametag.transform.Find("ReverseNametag");
        ntfr.GetComponent<TextMeshPro>().text = s;
        Debug.Log($"Changenametag via string; name set to {s}");
    }
    
    public void AttachNameTag(GameObject humanoid, String name)
    {
        GameObject nametagPrefab = (GameObject)Resources.Load("Nametag", typeof(GameObject));
        nametag = (GameObject)Instantiate(nametagPrefab);
        nametag.transform.SetParent(rigBoneIdx[0].transform);

        
        // Set the local position in the prefab in the editor
        //nametag.transform.localPosition = new Vector3(-0.14f, 0.8f, 0.0f);
        nametag.transform.localPosition = new Vector3(0.0f, nameTagHeight, 0.0f);
        Debug.Log($"Initial Nametag attach for name {name}");
        ChangeNameTag(name);
        //nametag.GetComponent<TextMeshPro>().text = name;

    }


    public void Destroy()
    {
        GameObject.Destroy(humanoid);

        Array.Clear(rigBoneIdx, 0, rigBoneIdx.Length);
        Array.Clear(rigBoneTargetIdx, 0, rigBoneIdx.Length);

    }


    

    public Transform GetHeadTransform()
    {
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38):
            case (BODY_FORMAT.BODY_38_KEYPOINTS):
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):                
                return rigBoneIdx[JointType_NECK].transform;
                break;
            case (BODY_FORMAT.BODY_34):
            case (BODY_FORMAT.BODY_34_KEYPOINTS):
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):
            default:
                return rigBoneIdx[JointType_34_Head].transform;
                break;
        }

    }

    public void DanceGraphMove()
    {
        HumanBodyBones[] humanBone;
        int[] parentsIdx;
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38):
                humanBone = humanBones38;
                parentsIdx = parentsIdx_38;
                break;
            case (BODY_FORMAT.BODY_34):
            default:
                humanBone = humanBones34;
                parentsIdx = parentsIdx_34;
                break;
        }

        for (int i = 0; i < humanBone.Length; i++)
        {
            if (humanBone[i] != HumanBodyBones.LastBone)
            {
                if (rigBoneIdx[i].transform)
                {
                    rigBoneIdx[i].transform.localRotation = default_RotationsIdx[i];
                }

            }
            
        }

        // for (int i = 0; i < humanBone.Length; i++) {
        //     if (rigBoneIdx[i].transform && (humanBone[i] != HumanBodyBones.LastBone)) {
        //         s += String.Format(" {1}:{0:0.0000} ", rigBoneIdx[i].transform.position, i);
        //         ls += String.Format(" {1}:{0:0.0000} ", rigBoneIdx[i].transform.localPosition, i);
        //     }
        // }
        
        // Debug.Log($"Position = {s}");
        // Debug.Log($"LocalPosition = {ls}");        
        
        for (int i = 0; i < humanBone.Length; i++)
        {
            if (humanBone[i] != HumanBodyBones.LastBone && rigBoneIdx[i].transform)
            {
                if (parentsIdx[i] != -1)
                {
                    Quaternion newRotation = rigBoneTargetIdx[i] * rigBoneIdx[i].transform.localRotation;
                    rigBoneIdx[i].transform.localRotation = newRotation;
                }
            }
        }

        // Apply global transform
        if (rigBoneIdx[0].transform)
        {
            var animator = humanoid.GetComponent<Animator>();
            // There is an offset between the joint "Hips" and the equivalent in the ZED SDK. This offset compensates it.
            Vector3 offset = new Vector3(0, (animator.GetBoneTransform(HumanBodyBones.Hips).position.y - animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position.y), 0);

            rigBoneIdx[0].transform.localPosition = targetBodyPosition + offset + new Vector3(0, heightOffset, 0);

            rigBoneIdx[0].transform.localRotation = targetBodyOrientation;
        }
    }

    public void DanceGraphMoveKP()
    {
        HumanBodyBones[] humanBone;
        int[] parentsIdx;
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38_KEYPOINTS):
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):                
                humanBone = humanBones38;
                parentsIdx = parentsIdx_38;
                break;
            case (BODY_FORMAT.BODY_34_KEYPOINTS):
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):                
            default:
                humanBone = humanBones34;
                parentsIdx = parentsIdx_34;
                break;
        }

        rigBoneIdx[0].transform.localPosition = rigBoneTargetPos[0];
        
          // What the fuck? Why isn't this necessary to make the avatar render properly?
        for (int i = 0; i < humanBone.Length; i++) {
            if (humanBone[i] != HumanBodyBones.LastBone) {
                //Vector3 parentsPos = rigBoneIdx[parentsIdx[i]].transform.position;
                rigBoneIdx[i].transform.position = rigBoneTargetPos[i];
            }
            
        }
    }

    public void DanceGraphAddRotation()
    {
        HumanBodyBones[] humanBone;
        int[] parentsIdx;
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):
                humanBone = humanBones38;
                parentsIdx = parentsIdx_38;
                break;
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):
            default:
                humanBone = humanBones34;
                parentsIdx = parentsIdx_34; 
                break;
        }

            
        // Root orientation needs to be set first
        rigBoneIdx[0].transform.localRotation = targetBodyOrientation;

        for (int i = 1; i < humanBone.Length; i++)
        {
            if (humanBone[i] != HumanBodyBones.LastBone && rigBoneIdx[i].transform)
            {
                if (parentsIdx[i] != -1)
                {
                    Quaternion newRotation = rigBoneTargetIdx[i] * default_RotationsIdx[i];

                    rigBoneIdx[i].transform.localRotation = newRotation;                    

                }
            }
        }

        // Apply global transform
        // if (rigBoneIdx[0].transform)
        // {
        //     var animator = humanoid.GetComponent<Animator>();
        //     // There is an offset between the joint "Hips" and the equivalent in the ZED SDK. This offset compensates it.
        //     Vector3 offset = new Vector3(0, (animator.GetBoneTransform(HumanBodyBones.Hips).position.y - animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position.y), 0);
        //     rigBoneIdx[0].transform.position = targetBodyPosition + offset + new Vector3(0, heightOffset, 0);

        //     rigBoneIdx[0].transform.localRotation = targetBodyOrientation;
            
        // }
    }
    

    public void DanceGraphSetHumanPoseControl(Vector3 rootPosition, Quaternion rootRotation, Quaternion[] jointsRotation)
        {
                
        HumanBodyBones[] humanBone;
        int [] poseBoneList;

        
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38):
                humanBone = humanBones38;
                poseBoneList = poseBoneList38;
                break;
            case (BODY_FORMAT.BODY_34):
            default:
                humanBone = humanBones34;
                poseBoneList = poseBoneList34;
                break;
        }

        int hipidx = Array.IndexOf(humanBone, HumanBodyBones.Hips);
        int spineidx = Array.IndexOf(humanBone, HumanBodyBones.Spine);
        // Store any joint local rotation (if the bone exists)
        if (rigBoneIdx[hipidx].transform)
        {
            rigBoneTargetIdx[spineidx] = jointsRotation[spineidx];
        }
        
        foreach (int boneIdx in poseBoneList)
        {
            if (boneIdx > 0)
                rigBoneTargetIdx[boneIdx] = jointsRotation[boneIdx];
        }
        // Store global transform (to be applied to the Hips joint).
        targetBodyOrientation = rootRotation;
        targetBodyPosition = rootPosition;

    }

    public void DanceGraphSetHumanPoseControlKP(Vector3 rootPosition, Quaternion rootRotation, Vector3 [] jointskeypoints)
    {

        HumanBodyBones[] humanBone;
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38_KEYPOINTS):
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):                
                humanBone = humanBones38; break;
            case (BODY_FORMAT.BODY_34_KEYPOINTS):
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):                                
            default:
                humanBone = humanBones34; break;
        }
        
        int hipidx = Array.IndexOf(humanBone, HumanBodyBones.Hips);
        int spineidx = Array.IndexOf(humanBone, HumanBodyBones.Spine);
        // Store any joint local rotation (if the bone exists)
        if (rigBoneIdx[hipidx].transform)
        {
            rigBoneTargetPos[spineidx] = jointskeypoints[spineidx];
        }
        
        for (int idx = 0; idx < rigBoneTargetPos.Length; idx++)
            rigBoneTargetPos[idx] = jointskeypoints[idx];

        // Store global transform (to be applied to the Hips joint).
        targetBodyOrientation = rootRotation;
        targetBodyPosition = rootPosition;
    }


    public void DanceGraphSetHumanPoseControlKPRot(Vector3 rootPosition,
                                                   Quaternion rootOrientation,
                                                   Vector3 []keypoints,
                                                   Quaternion [] jointsRotation) {

        int [] poseBoneList;
        HumanBodyBones[] humanBone;
        switch (BodyFormat)
        {
            case (BODY_FORMAT.BODY_38_KEYPOINTSPLUS):                
                humanBone = humanBones38;
                poseBoneList = poseBoneList38;
                break;
            case (BODY_FORMAT.BODY_34_KEYPOINTSPLUS):                                
            default:
                humanBone = humanBones34;
                poseBoneList = poseBoneList34;
                break;
        }
        
        int hipidx = Array.IndexOf(humanBone, HumanBodyBones.Hips);
        int spineidx = Array.IndexOf(humanBone, HumanBodyBones.Spine);
        // Store any joint local rotation (if the bone exists)
        if (rigBoneIdx[hipidx].transform)
        {
            rigBoneTargetPos[spineidx] = keypoints[spineidx];
            rigBoneTargetIdx[spineidx] = jointsRotation[spineidx];
        }

        
        for (int idx = 0; idx < rigBoneTargetPos.Length; idx++)
            rigBoneTargetPos[idx] = keypoints[idx];
        foreach (int boneIdx in poseBoneList)
            if (boneIdx > 0)
                rigBoneTargetIdx[boneIdx] = jointsRotation[boneIdx];

        // Store global transform (to be applied to the Hips joint).
        targetBodyOrientation = rootOrientation;
        targetBodyPosition = rootPosition;

    }

    
    public void CreateTestSkeleton(GameObject gObj, int num_bones) {
         testAvatarJoints = new GameObject[num_bones];
         testAvatarBones = new GameObject[num_bones];
         
         GameObject testJointPrefab = (GameObject)Resources.Load("TestJoint", typeof(GameObject));
         for (int i = 0; i < num_bones; i ++)
         {
              testAvatarJoints[i] = (GameObject)Instantiate(testJointPrefab);
              testAvatarJoints[i].transform.parent = gObj.transform;
              testAvatarJoints[i].name = String.Format("Bone {0}",i);
         }

         int [] parentsIdx;
         GameObject testBonePrefab = (GameObject)Resources.Load("TestSkelBone", typeof(GameObject));
         
         if ((currentBodyFormat == BODY_FORMAT.BODY_34_KEYPOINTS)
             || (currentBodyFormat == BODY_FORMAT.BODY_34_KEYPOINTSPLUS))
         {
              parentsIdx = parentsIdx_34;
         }         
         else {
              parentsIdx = parentsIdx_38;
         }

         for (int i = 0; i < parentsIdx.Length; i++) {
              if ((parentsIdx[i]) > 0 && (parentsIdx[i] < i)) {
                  testAvatarBones[i] = (GameObject)Instantiate(testBonePrefab);
                  testAvatarBones[i].transform.parent = gObj.transform;
              }
         }
    }

    public void MoveTestSkeleton(Vector3 [] keypoints, Vector3 testOffset) {

         for (int i = 0; i < keypoints.Length; i++) {
             testAvatarJoints[i].transform.localPosition = keypoints[i] + testOffset;
         }

         int [] parentsIdx;
         if ((currentBodyFormat == BODY_FORMAT.BODY_34_KEYPOINTS)
             ||(currentBodyFormat == BODY_FORMAT.BODY_34_KEYPOINTSPLUS))
         {
              parentsIdx = parentsIdx_34;
         }         
         else {
              parentsIdx = parentsIdx_38;
         }

         for (int i = 0; i < parentsIdx.Length; i++) {
              if ((parentsIdx[i]) > 0 && (parentsIdx[i] < i)) {
                   Vector3 pos1 = testAvatarJoints[i].transform.localPosition;
                   Vector3 pos2 = testAvatarJoints[parentsIdx[i]].transform.localPosition;

                   testAvatarBones[i].transform.localScale = new Vector3(0.01f, 0.5f * Vector3.Distance(pos1, pos2), 0.01f);
                   testAvatarBones[i].transform.localPosition = 0.5f * (pos1 + pos2);

                   float angle = Vector3.Angle(pos2 - pos1, Vector3.up);
                   Vector3 axis = Vector3.Normalize(Vector3.Cross(pos2 - pos1, Vector3.up));
                   testAvatarBones[i].transform.rotation = Quaternion.identity;
                   
                   testAvatarBones[i].transform.Rotate(axis, - angle, Space.World);
              }
         }
    }

}

