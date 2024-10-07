using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;
using dg.sig;
using UnityEngine;


namespace DanceGraph
{
    namespace SignalBehaviours
    {

        public struct Adjustment
        {
            public bool rotate180;
            public float rotAngle;
            public Vector3 startPosition;
            public Vector3 delta;
            public Vector3 lookDir;
        };
        
        public struct BodyData_34
        {
            public static int[] fullbones = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33 };
            public static int[] usefulbones = new int[] { 2, 3, 5, 6, 12, 13, 18, 19, 22, 23, 26 };            
            public static int num_bones = 34;

        };
        
        public struct BodyData_38
        {
            
            public static int[] fullbones = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37 };
            //public static int[] usefulbones = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37 };
            public static int[] usefulbones = new int[] { 1, 2, 3, 4, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }; 
            public static int num_bones = 38;
             
        };

        
        public class TimedSkeleton {
            public int userID;
            public float timeStamp;
            public Vector3 rootPos;
            public Quaternion rootOri;
            public Quaternion [] bones;
        }

        public class TimedKPSkeleton : TimedSkeleton {
            // public int userID;
            // public float timeStamp;
            // public Vector3 rootPos;
            // public Quaternion rootOri;
            public Vector3 [] keypoints;
        }

        public class TimedKPPlusSkeleton : TimedSkeleton {
            // public int userID;
            // public float timeStamp;
            // public Vector3 rootPos;
            // public Quaternion rootOri;
            public Vector3 [] keypoints;
            // public Quaternion [] bones;            
        }

        public class ZedSignal : MonoBehaviour, sig.ISignalHandler
        {
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct quant_quat
            {
                public short x, y, z;
                public Quaternion quat()
                {
                    var v = new Vector3(x, y, z) / 32767;
                    var sqrMag = v.sqrMagnitude;
                    Debug.Assert(sqrMag <= 1, $"sqr mag > 1: {sqrMag}");
                    var w = Mathf.Sqrt(1.0f - sqrMag);
                    return new Quaternion(v.x, v.y, v.z, w);
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct xform_quant
            {
                public Vector3 pos;
                public quant_quat ori;
            }

            // Keep a bones cache to avoid rebuilding an array each time
            public Quaternion[] bones;

            // Likewise for keypoint data
            public Vector3 [] keypoints;
            
            public SkeletonHandler skeletonHandler;

            public Avatar avatar;
            
            public SkeletonHandler.BODY_FORMAT bodyFormat;
            public int[] USEFUL_BONES;
            public int NUM_BONES_FULL;

            public TimedSkeleton prevSkel;
            public TimedSkeleton lastSkel;
            
            public World world;

            public bool placedInWorld = false;
            public bool simplePrediction = false;
            public bool signalUpdated = false;
            
            // Whether we've rotated the user 180 degrees for placement
            private Adjustment adjustment;

            // millimetres to metres
            public float SCALE_FACTOR_ZED_UNITY;
            
            public void Start()
            {
                Debug.Log("ZedSignal Start being run");

                SCALE_FACTOR_ZED_UNITY = World.instance.zedScaleFactor * World.instance.zedScaleAdjustment;
            }

            // This method expects that the userState for the avatar is already populated
            public GameObject LoadAvatar(String name, int idx,  EnvUserState userState, bool simpleSmoothing = false)
            {
                if (avatar is null)
                    avatar = new Avatar();                    
                
                simplePrediction = simpleSmoothing;
                // TODO: Read this from either the environment signals or the config system

                if (idx == World.instance.localClientUserIdx) {
                    Debug.Log($"Loading local avatar with state type {userState.avatarType} and params {userState.avatarParams}");
                 
                    avatar.LoadAvatar(idx, userState);
                }
                else {
                    avatar.LoadAvatar(idx, userState);
                }
                
                //bodyFormat = SkeletonHandler.BODY_FORMAT.BODY_34_KEYPOINTS;
                bodyFormat = World.instance.bodyFormat;
                switch (bodyFormat)
                {
                    case SkeletonHandler.BODY_FORMAT.BODY_38:
                        USEFUL_BONES = BodyData_38.usefulbones;
                        NUM_BONES_FULL = BodyData_38.num_bones;
                        break;
                    case SkeletonHandler.BODY_FORMAT.BODY_34_KEYPOINTS:
                        // Position data has no redundant bones
                        USEFUL_BONES = BodyData_34.fullbones;
                        NUM_BONES_FULL = BodyData_34.num_bones;
                        break;
                    case SkeletonHandler.BODY_FORMAT.BODY_38_KEYPOINTS:
                        // Position data has no redundant bones                        
                        USEFUL_BONES = BodyData_38.fullbones;
                        NUM_BONES_FULL = BodyData_38.num_bones;
                        break;
                    case SkeletonHandler.BODY_FORMAT.BODY_38_KEYPOINTSPLUS:
                        USEFUL_BONES = BodyData_38.usefulbones;
                        NUM_BONES_FULL = BodyData_38.num_bones;
                        break;
                    case SkeletonHandler.BODY_FORMAT.BODY_34_KEYPOINTSPLUS:
                        USEFUL_BONES = BodyData_34.usefulbones;
                        NUM_BONES_FULL = BodyData_34.num_bones;
                        break;
                    case SkeletonHandler.BODY_FORMAT.BODY_34:
                    default:
                        USEFUL_BONES = BodyData_34.usefulbones;
                        NUM_BONES_FULL = BodyData_34.num_bones;
                        break;
                }
                
                skeletonHandler = ScriptableObject.CreateInstance<SkeletonHandler>();

                if (skeletonHandler is null) 
                    Debug.Log("SkeletonHandler is null");

                skeletonHandler.currentBodyFormat = bodyFormat;

                skeletonHandler.nameTagHeight = World.instance.zedScaleAdjustment * SkeletonHandler.nameTagBaseHeight;

                GameObject trackedAvatar = skeletonHandler.DanceGraphCreate(avatar.gObject, bodyFormat);

                trackedAvatar.transform.localScale *= World.instance.avatarScale;
                
                Debug.Log(String.Format("Spawned avatar at {0}", trackedAvatar.transform.position));
                if (World.instance.nametags) 
                    skeletonHandler.AttachNameTag(avatar.gObject, name);

                trackedAvatar.transform.parent = gameObject.transform;

                if (World.instance.testAvatar) 
                    skeletonHandler.CreateTestSkeleton(gameObject, NUM_BONES_FULL);
                
                keypoints = new Vector3[NUM_BONES_FULL];
                for (int i = 0; i < NUM_BONES_FULL; i++) 
                     keypoints[i] = new Vector3(0.0f, 0.0f, 0.0f);

                
                bones = new Quaternion[NUM_BONES_FULL];
                for (int i = 0; i < bones.Length; ++i)
                    bones[i] = new Quaternion(0, 0, 0, 1);

                if (avatar is null)
                    Debug.Log("Avatar is null");

                if (avatar.avAppearance is null)
                    Debug.Log("avAppearance is null");                    
                else
                    Debug.Log("avAppearance is NOT null");
                avatar.avAppearance.ApplyParameters(idx, trackedAvatar);
                
                return trackedAvatar;
            }
            
            public void Update() {
                if (simplePrediction && !signalUpdated) {
                    PredictSignal();
                }
                signalUpdated = false;
                
                Transform ht = skeletonHandler.GetHeadTransform();
                
                GameObject xrOrigin = GameObject.Find("XR Origin");
                GameObject xrCam = GameObject.Find("CameraOffset");
                GameObject mCam = GameObject.Find("Main Camera");                
                // Debug.Log($"HMD Position is {xrOrigin.transform.position}");
                // Debug.Log($"Cam Offset is {xrCam.transform.position}");
                // Debug.Log($"Main Camera is {mCam.transform.position}, head is at {ht.transform.position}");
            }

            public void PredictSignal() {
                if ((prevSkel == null) || (lastSkel == null))
                    return;

                // Do we extrapolate root position and orientation?
                
                Quaternion [] predictedBones = new Quaternion[lastSkel.bones.Length];

                float newTime = Time.realtimeSinceStartup;

                float lerpTval = (newTime - prevSkel.timeStamp) / (lastSkel.timeStamp - prevSkel.timeStamp);

                Vector3 newRootPos = Vector3.LerpUnclamped(prevSkel.rootPos, lastSkel.rootPos, lerpTval);
                Quaternion newRootOri = Quaternion.SlerpUnclamped(prevSkel.rootOri, lastSkel.rootOri, lerpTval);

                // Vector3 newRootPos = lastSkel.rootPos;
                // Quaternion newRootOri = lastSkel.rootOri;
                
                for (int i = 0; i < lastSkel.bones.Length; i++) {
                    predictedBones[i] = Quaternion.SlerpUnclamped(prevSkel.bones[i], lastSkel.bones[i], lerpTval);
                }

                DriveSkeleton(lastSkel.userID, newRootPos, newRootOri, predictedBones);
            }

            public void DriveSkeleton(int userID, Vector3 rootPos, Quaternion rootOri, Quaternion [] bones) {
                var pos = gameObject.transform.position;
                var rot = gameObject.transform.rotation;
                gameObject.transform.position = default;
                World.instance.clientInfo.Update(userID, rootPos, rootOri, skeletonHandler.GetHeadTransform());

                skeletonHandler.DanceGraphSetHumanPoseControl(rootPos, rootOri, bones);
                skeletonHandler.DanceGraphMove();
                gameObject.transform.position = pos;
                gameObject.transform.rotation = rot;

            }

            public void DrivePositionSkeleton(int userID, Vector3 rootPos, Quaternion rootOri, Vector3 [] keypoints) {
                var pos = gameObject.transform.position;
                var rot = gameObject.transform.rotation;
                gameObject.transform.position = default;

                World.instance.clientInfo.Update(userID, rootPos, rootOri, skeletonHandler.GetHeadTransform());

                skeletonHandler.DanceGraphSetHumanPoseControlKP(rootPos, rootOri, keypoints);
                
                skeletonHandler.DanceGraphMoveKP();

                gameObject.transform.position = pos;
                gameObject.transform.rotation = rot;

            }

            public void DrivePositionRotationSkeleton(int userID, Vector3 rootPos, Quaternion rootOri, Vector3 [] keypoints, Quaternion [] bones) {
                var pos = gameObject.transform.position;
                var rot = gameObject.transform.rotation;
                gameObject.transform.position = default;

                World.instance.clientInfo.Update(userID, rootPos, rootOri, skeletonHandler.GetHeadTransform());
                //Debug.Log($"Updating client {userID} with root rot {rootOri}");

                skeletonHandler.DanceGraphSetHumanPoseControlKPRot(rootPos, rootOri, keypoints, bones, World.instance.avatarScale);
                //skeletonHandler.DanceGraphSetHumanPoseControlKP(rootPos, rootOri, keypoints);

                // The rotations come first, then the positions                
                skeletonHandler.DanceGraphAddRotation();                    
                skeletonHandler.DanceGraphMoveKP();

                gameObject.transform.position = pos;
                gameObject.transform.rotation = rot;
            }


            public void HandleSignalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta) {

                    
                if ((bodyFormat == SkeletonHandler.BODY_FORMAT.BODY_34_KEYPOINTS)
                    || (bodyFormat == SkeletonHandler.BODY_FORMAT.BODY_38_KEYPOINTS))
                    HandlePositionalData(data, sigMeta);
                else if ((bodyFormat == SkeletonHandler.BODY_FORMAT.BODY_34_KEYPOINTSPLUS)
                    || (bodyFormat == SkeletonHandler.BODY_FORMAT.BODY_38_KEYPOINTSPLUS))
                    HandleDualData(data, sigMeta);
                //HandlePositionalData(data, sigMeta);
                else
                    HandleRotationalData(data, sigMeta);

            }
            
            public void HandlePositionalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta) {

                //-------------------------------------------------------------
                // READ Keypoint Positional data into zedSkeletonSignal
                //-------------------------------------------------------------

                // READ u8 num_skeletons
                var num_skeletons = SpanConsumer.ReadValue<int>(ref data);
                if (num_skeletons < 1)
                    return;
                var this_is_10 = SpanConsumer.ReadValue<int>(ref data);

                // READ time_point elapsed
                var elapsed = SpanConsumer.ReadValue<ulong>(ref data);

                // READ first skeleton
                var skel_id = SpanConsumer.ReadValue<int>(ref data);
                // READ root transform
                var root_pos = SpanConsumer.ReadValue<Vector3>(ref data);
                root_pos *= SCALE_FACTOR_ZED_UNITY;
                
                var root_ori = SpanConsumer.ReadValue<quant_quat>(ref data);


                for (int j = 0; j < keypoints.Length; ++j)
                {
                    var skel_kp_data = SpanConsumer.ReadValue<Vector3> (ref data);
                    keypoints[j] = skel_kp_data * SCALE_FACTOR_ZED_UNITY;
                }


                if (num_skeletons == 1)
                    Debug.Assert(data.Length == 0, $"Data read error: We should be done but we have {data.Length} bytes left");

                if (skeletonHandler == null)
                {
                    Debug.LogWarning("Skeleton handler is null for ZedSignal");
                    return;
                }
                
                DrivePositionSkeleton(sigMeta.userIdx, root_pos, root_ori.quat(), keypoints);

                if (!placedInWorld)
                {
                    Debug.Log($"Placing object {gameObject.name} in world at {gameObject.transform}");
                    adjustment = World.DancePlacement(
                        sigMeta.userIdx,
                        gameObject.transform,
                        avatar.gObject.transform,
                        root_pos,
                        root_ori.quat()
                    );
                    
                    placedInWorld = true;
                }
                
                if (World.instance.testAvatar) {
                    skeletonHandler.MoveTestSkeleton(keypoints, World.instance.testAvatarOffset);
                }
             
                if (simplePrediction) {

                    prevSkel = lastSkel;
                    
                    lastSkel = new TimedKPSkeleton{
                        userID = sigMeta.userIdx,
                        timeStamp = Time.realtimeSinceStartup,
                        rootPos = root_pos,
                        rootOri = root_ori.quat(),
                        keypoints = keypoints
                    };
                    signalUpdated = true;
                }

            }

            public List<Transform> ListChildren(Transform trans) {

                List<Transform> current_list = new List<Transform>();

                current_list.Add(trans);

                foreach (Transform t in trans) {
                    current_list.AddRange(ListChildren(t));
                }
                return current_list;
                                        
                
            }
            
            public void DumpBonePositions() {
                List<Transform> bones = ListChildren(avatar.gObject.transform);
                String ss = "Bones: ";
                foreach (Transform t in bones) {
                    ss += t.gameObject.name + ":" + t.position.ToString("F4");
                }

                Debug.Log($"{ss}");
            }

            
            // The default signal handler prints the byte sequence for this signal packet
            public void HandleRotationalData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {
                //-------------------------------------------------------------
                // READ THE DATA into zedSkeletonSignal
                //-------------------------------------------------------------

                // READ u8 num_skeletons
                var num_skeletons = SpanConsumer.ReadValue<int>(ref data);
                if (num_skeletons < 1)
                    return;
                var this_is_10 = SpanConsumer.ReadValue<int>(ref data);

                // READ time_point elapsed
                var elapsed = SpanConsumer.ReadValue<ulong>(ref data);

                // READ first skeleton
                var skel_id = SpanConsumer.ReadValue<int>(ref data);
                // READ root transform
                var root_pos = SpanConsumer.ReadValue<Vector3>(ref data);
                var root_ori = SpanConsumer.ReadValue<quant_quat>(ref data);



                // READ each useful bone
                for (int j = 0; j < USEFUL_BONES.Length; ++j)
                {
                    var skel_bone_data = SpanConsumer.ReadValue<quant_quat>(ref data);
                    bones[USEFUL_BONES[j]] = skel_bone_data.quat();
                }
                
                // (Ignore any other skeletons.)
                if (num_skeletons == 1)
                    Debug.Assert(data.Length == 0, $"Data read error: We should be done but we have {data.Length} bytes left");

                //-------------------------------------------------------------
                // UPDATE Zed data structures
                //-------------------------------------------------------------
                root_pos *= 0.001f;
                if (skeletonHandler == null) // maybe we just created the gameobject and it hasn't Start()'d yet
                {
                    Debug.LogWarning("Skeleton handler is null for ZedSignal");
                    return;
                }

                DriveSkeleton(sigMeta.userIdx, root_pos, root_ori.quat(), bones);


                Debug.LogWarning("Place User IN World");

                if (!placedInWorld)
                {
                    Debug.Log($"Client not placed in world");
                    World.DancePlacement(
                        sigMeta.userIdx,
                        gameObject.transform,
                        avatar.gObject.transform,
                        root_pos,
                        root_ori.quat()
                    );
                    placedInWorld = true;
                }


                //DumpBonePositions();

                
                if (simplePrediction) {

                    prevSkel = lastSkel;
                    
                    lastSkel = new TimedSkeleton{
                        userID = sigMeta.userIdx,
                        timeStamp = Time.realtimeSinceStartup,
                        rootPos = root_pos,
                        rootOri = root_ori.quat(),
                        bones = bones
                    };
                    signalUpdated = true;
                }
            }


            // The default signal handler prints the byte sequence for this signal packet
            public void HandleDualData(ReadOnlySpan<byte> data, in SignalMetadata sigMeta)
            {
                //-------------------------------------------------------------
                // READ THE DATA into zedSkeletonSignal
                //-------------------------------------------------------------
                var init_length = data.Length;
                // READ u8 num_skeletons
                var num_skeletons = SpanConsumer.ReadValue<int>(ref data);
                if (num_skeletons < 1)
                    return;
                var this_is_10 = SpanConsumer.ReadValue<int>(ref data);

                // READ time_point elapsed
                var elapsed = SpanConsumer.ReadValue<ulong>(ref data);

                // READ first skeleton
                var skel_id = SpanConsumer.ReadValue<int>(ref data);
                // READ root transform
                var root_pos = SpanConsumer.ReadValue<Vector3>(ref data);
                root_pos *= SCALE_FACTOR_ZED_UNITY;
                
                var root_ori = SpanConsumer.ReadValue<quant_quat>(ref data);
                
                // READ each useful bone
                for (int j = 0; j < keypoints.Length; ++j)
                {
                    var skel_kp_data = SpanConsumer.ReadValue<Vector3>(ref data);
                    keypoints[j] = skel_kp_data * SCALE_FACTOR_ZED_UNITY;
                }

                //Debug.Log($"P{sigMeta.packetId} Root position: {root_pos}, Bone 0,1 kps: {keypoints[0]} {keypoints[1]}");

                for (int j = 0; j < USEFUL_BONES.Length; ++j)
                {
                    var skel_bone_data = SpanConsumer.ReadValue<quant_quat>(ref data);
                    bones[USEFUL_BONES[j]] = skel_bone_data.quat();
                }


                // (Ignore any other skeletons.)
                if (num_skeletons == 1)
                    Debug.Assert(data.Length == 0, $"Data read error: We should be done but we have {data.Length}/{init_length} bytes left");

                if (skeletonHandler == null)
                {
                    Debug.LogWarning("Skeleton handler is null for ZedSignal");
                    return;
                }

                Quaternion q = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                
                if (!placedInWorld)
                {
                    Debug.Log($"Placing skel {sigMeta.userIdx} in world");
                    adjustment = World.DancePlacement(
                        sigMeta.userIdx,
                        gameObject.transform,
                        avatar.gObject.transform,
                        root_pos,
                        root_ori.quat()
                    );

                    Debug.Log($"P{sigMeta.packetId} U{sigMeta.userIdx}, placing object {gameObject.name} at {gameObject.transform.position}");             
                    placedInWorld = true;

                }

                // For some bizarre reason, packet 1 has a zero root position (!), which screws with the HMD teleport
                if ((sigMeta.userIdx == World.instance.localClientUserIdx) && (World.instance.initialTeleport == 0) && (sigMeta.packetId > 1)) {
                    World.instance.initialTeleport = 1;
                }

                Quaternion rotation = Quaternion.Euler(0.0f, adjustment.rotAngle, 0.0f);                

                Vector3 [] newkp = new Vector3[keypoints.Length];

                Vector3 nkp = rotation * root_pos + rotation * (keypoints[0] - root_pos);
                
                for (int i = 0; i < keypoints.Length; i++) {
                    //newkp[i] = keypoints[0] + rotation * (keypoints[i] - keypoints[0]);
                    newkp[i] = nkp + rotation * (keypoints[i] - keypoints[0]);
                }

                //Debug.Log($"HD {sigMeta.packetId}:{sigMeta.userIdx} Position: {avatar.gObject.transform}, rotation: {rotation}, root: {root_pos}, nkp: {nkp}");
                DrivePositionRotationSkeleton(sigMeta.userIdx, rotation * root_pos, root_ori.quat(), newkp, bones);
                
                // for (int i = 0; i < keypoints.Length; i++) {
                //     newkp[i] = keypoints[0] + rotation * (keypoints[i] - keypoints[0]);
                // }
                // DrivePositionRotationSkeleton(sigMeta.userIdx, root_pos, root_ori.quat(), newkp, bones);
#if false                
                Quaternion rotation = Quaternion.Euler(0.0f, adjustment.rotAngle, 0.0f);
                //Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                
                Vector3 [] newkp = new Vector3[keypoints.Length];
                
                for (int i = 0; i < keypoints.Length; i++) {
                    newkp[i] = keypoints[0] + rotation * (keypoints[i] - keypoints[0]);
                }

                Vector3 new_root_pos = rotation * root_pos;
                
                Debug.Log($"AdjAngle is {adjustment.rotAngle}, Root pos is {root_pos}, kp0 is {keypoints[0]}, nkp0 = {newkp[0]}");

                Debug.Log("Switching gameobject transform from {gameObject.transform.position} to {root_pos}");
                avatar.gObject.transform.position = root_pos;
                DrivePositionRotationSkeleton(sigMeta.userIdx, root_pos, rotation * root_ori.quat(), newkp, bones);
#endif                 
                /*
                
                Quaternion rot180quat = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            
                if (adjustment.rotate180) {
                    Vector3 [] newkp = new Vector3[keypoints.Length];
                    for (int i = 0; i < keypoints.Length; i++) {
                        newkp[i] = new Vector3(-keypoints[i].x + adjustment.delta.x, keypoints[i].y, -keypoints[i].z + adjustment.delta.z);
                    }
                    
                    DrivePositionRotationSkeleton(sigMeta.userIdx, root_pos, rot180quat * root_ori.quat(), newkp, bones);
                }
                else {
                    DrivePositionRotationSkeleton(sigMeta.userIdx, root_pos, root_ori.quat(), keypoints, bones);
                }
                */
                
                if (World.instance.testAvatar) {
                    skeletonHandler.MoveTestSkeleton(keypoints, World.instance.testAvatarOffset);
                }
                
                if (simplePrediction) {

                    prevSkel = lastSkel;
                    
                    lastSkel = new TimedKPSkeleton{
                        userID = sigMeta.userIdx,
                        timeStamp = Time.realtimeSinceStartup,
                        rootPos = root_pos,
                        rootOri = root_ori.quat(),
                        keypoints = keypoints
                    };
                    signalUpdated = true;
                }
            }
        }
    }

}
