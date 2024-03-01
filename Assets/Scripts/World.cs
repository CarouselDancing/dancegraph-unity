#define TEST_DGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DanceGraph.sig;
using DanceGraph.SignalBehaviours;
using DanceGraph.SignalSerialization;
using dg.sig;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.Windows;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Input = UnityEngine.Input;

using System.Text;

namespace DanceGraph
{
     public struct ClientData
     {
          public Vector3 rootPos;
          public Quaternion rootOri;

          public float lastSeen;

          //public Quaternion headRotation;
          public Transform headTrans;
     }
    

     [Serializable]
     public class MovingAverage  
     {
          private Queue<double> samples = new();
          private int windowSize = 100;
          private double sampleAccumulator;
          public float average;

          public static MovingAverage New(float newSample)
          {
               var ma = new MovingAverage();
               ma.ComputeAverage(newSample);
               return ma;
          }

          public float ComputeAverage(double newSample)
          {
               sampleAccumulator += newSample;
               samples.Enqueue(newSample);

               if (samples.Count > windowSize)
                    sampleAccumulator -= samples.Dequeue();
               average = (float)(sampleAccumulator / samples.Count);
               return average;
          }
     }

     [Serializable]
     public class SignalCountMap
     {
          [Serializable]
          public class Elem
          {
               public short userIdx;
               public byte sigIdx;
               public byte sigType;
               public int count;
               public float lastRecordedTime;
               public MovingAverage ma;
          }

          public List<Elem> elements = new List<Elem>();

          public void Add(in SignalMetadata sigMeta)
               {
                    var timeNow = Time.time;
                    int iFound = elements.Count;
                    for (int i = 0; i < iFound; ++i)
                    {
                         var elem = elements[i];
                         if (elem.userIdx == sigMeta.userIdx && elem.sigIdx == sigMeta.sigIdx && elem.sigType == sigMeta.sigType)
                         {
                              iFound = i;
                              break;
                         }
                    }

                    if (iFound == elements.Count)
                         elements.Add(new Elem()
                         {
                              userIdx = sigMeta.userIdx,
                                   sigIdx = sigMeta.sigIdx,
                                   sigType = sigMeta.sigType,
                                   count = 1,
                                   lastRecordedTime = timeNow,
                                   ma = new MovingAverage()
                                   });
                    else
                    {
                         var elem = elements[iFound];
                         elem.count++;
                         elem.ma.ComputeAverage(timeNow - elem.lastRecordedTime);
                         elem.lastRecordedTime = timeNow;
                         elements[iFound] = elem;
                    }
               }
     }


     public class ClientInfo
     {
          public List<ClientData> clients;

          public void Update(int clientIdx, in Vector3 rootPos, in Quaternion rootOri, in Transform headTrans)
               {

                    if (clientIdx >= clients.Count - 1)
                    {
                         for (int i = clients.Count; i <= clientIdx; ++i)
                              clients.Add(new ClientData()
                              {
                                   rootPos = Vector3.zero,
                                        rootOri = new Quaternion(0f, 0f, 0f, 1f),
                                        lastSeen = -1f,
                                        headTrans = headTrans,
                                        });
                    }
            
                    clients[clientIdx] = new ClientData()
                    {
                         rootPos = rootPos,
                         rootOri = rootOri,
                         lastSeen = Time.realtimeSinceStartup,
                         headTrans = headTrans,
                    };
               }
     }

     [Serializable]
     public class AppSettings
     {
          public cfg.rt.Client preset;
          public int logLevel = 2;
          public string logToFileStem = "unity";
          public bool autoConnect = false;
          public bool simpleSkeletonSmoothing = false;
          public bool framewiseClientMatching;
     }

     public class World : MonoBehaviour
     {
          bool DIRECT_DLL = true;

          public static string DANCEGRAPH_PATH =>
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/DanceGraph";

          public static string CONFIG_PATH => DANCEGRAPH_PATH + "/dancegraph.json";

          public static World instance = null;

          public AppSettings appSettings;
          public GameObject scene;

          public List<GameObject> clients;

          private List<GameObject> localAvatarObjects;

          public ClientInfo clientInfo;

          public cfg.DanceGraphConfig config;

          public GameObject localDevice;
          //        public GameObject localClient;

          // This is a multi-stage teleport indicator
          public int initialTeleport = 0;

          public string statusMessage;
        
          public int localClientUserIdx = -1;


          public float zedScaleFactor = 0.001f;
          public float zedScaleAdjustment = 1.3f;
          public float zedAvatarRootHeight = 0.9f;
          
          // Post-init state
          public bool isInitialized = false;
          public string activeScene;
          public string[] orderedUserSignals;

          public bool testAvatar = true;
          public Vector3 testAvatarOffset = new Vector3(1.5f, 0.0f, 0.0f);
        
          // Basic instrumentation
          public SignalCountMap envSignalCountMap = new SignalCountMap();
          public SignalCountMap userSignalCountMap = new SignalCountMap();

          public GameObject musicPlayer;

          public bool paused = false;
          
          public SkeletonHandler.BODY_FORMAT bodyFormat = SkeletonHandler.BODY_FORMAT.BODY_38_KEYPOINTS;
        
          public static string _GetLocalIPAddress()
          {
               string localIP;
               using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
               {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
               }
               return localIP;
          }

          static Color GetColorByIndex(int idx, bool joints)
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
               // Ideally the multiplier and cvals.Length are relatively prime            
               return cvals[(idx * 5 + 3)%cvals.Length];
          }


          void LogSystemSettings() {
               DanceGraphMinimalCpp.NativeDebugLog($"CPU: {SystemInfo.processorType}, {SystemInfo.processorFrequency}");
               DanceGraphMinimalCpp.NativeDebugLog($"Mem: {SystemInfo.systemMemorySize}");
               DanceGraphMinimalCpp.NativeDebugLog($"OS: {SystemInfo.operatingSystem}");
               DanceGraphMinimalCpp.NativeDebugLog($"GPU: {SystemInfo.graphicsDeviceName}");
          }

          public void Awake()
          {
              instance = this;
          }

          void Start()
          {
               scene = new GameObject("Scene");
               scene.transform.parent = gameObject.transform; // scene is child of world
               clients = new List<GameObject>();

               // Load configuration file
               config = Newtonsoft.Json.JsonConvert.DeserializeObject<cfg.DanceGraphConfig>(
                    System.IO.File.ReadAllText(CONFIG_PATH));

               localDevice = GameObject.Find("Main Camera");

               clientInfo = new ClientInfo();
               clientInfo.clients = new List<ClientData>();

               localAvatarObjects = new List<GameObject>();

               envSignal = scene.AddComponent<EnvSignal>();
            
               var settingsFilename = Application.streamingAssetsPath + "/settings.json";
               if (System.IO.File.Exists(settingsFilename))
               {
                    try
                    {
                         appSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(System.IO.File.ReadAllText(settingsFilename));
                    }
                    catch (Exception e)
                    {
                         Debug.LogException(e);
                         // Can we handle this properly?
                         //QuitApplication(String.Format("JSON Deserialization Error in {0}", settingsFilename));
                         throw;
                    }
               }

               statusMessage = "Initializing";
               
               // Update settings
               if (appSettings.preset.address.ip == null || appSettings.preset.address.ip == "")
               {
                    var ip = _GetLocalIPAddress();
                    Debug.Log("Setting ip address: " + ip);
                    appSettings.preset.address.ip = ip;
               }

               Debug.Log("Setting log level: " + appSettings.logLevel);
               DanceGraphMinimalCpp.SetLogLevel(appSettings.logLevel);
               if (appSettings.logToFileStem != null && appSettings.logToFileStem != "")
               {
                    var logFile = World.DANCEGRAPH_PATH + $"/log_{appSettings.logToFileStem}.txt";
                    Debug.Log("Setting log file: " + logFile);
                    DanceGraphMinimalCpp.SetLogToFile(logFile);
               }
               // Default username if we haven't specified one
               if (appSettings.preset.username == null || appSettings.preset.username == "")
                    appSettings.preset.username = Environment.UserName;
            
               LogSystemSettings();
               Debug.Log($"Local Username set to {appSettings.preset.username}");
          }


          public EnvSignal envSignal = null;

          public void InitScene(string scene_name)
          {
               if (isInitialized)
                    return;
               
               activeScene = scene_name;

               // TODO: normally the exact type of signal depends on the scene spec in json
               var signalContainer = scene.AddComponent<SignalContainer>();
               int sigIdx = 0; // e.g. here we assume EnvSignal has index of 0 in the sorted list

               signalContainer.OnSignalAdded(envSignal, sigIdx);
               // load/sort all signals used in scene, so that we can map signal index to component needed to be added
               if (config.scenes.TryGetValue(scene_name, out var scene_cfg))
               {
                    var user_signal_names = new HashSet<string>();
                    foreach (var (name, role) in scene_cfg.user_roles)
                         foreach (var sig in role.user_signals)
                              user_signal_names.Add(sig.name);
                    orderedUserSignals = user_signal_names.ToArray();
                    Debug.Log($"Initialize scene {scene_name} with signals {String.Join(", ", orderedUserSignals)}");
               }
               else
               {
                    Debug.LogError($"Scene {scene_name} not found in database");
               }
               isInitialized = true;
          }

          // This manipulates the transform so that we place people properly
          // Current strategy is:
          //  * Each pair is facing each other at X distance
          //  * Pairs are arrange in a horizontal row, starting in middle and placed left,right,left

          public static Adjustment StartPosition(int index) {
               const float kIntraPairDistance = 1.0f; // distance between a pair of dancers
               const float kInterPairDistance = 2.0f; // distance between pairs of dancers
               bool rotate180 = (index & 1) == 1;

               // we want to achieve indexing: 6 4 2 0 1 3 5
               int pairIndex = index >> 1;
               bool pairIsOdd = (pairIndex & 1) == 1;
               int absSlotIndex = (pairIndex + 1) >> 1;
               float x = kInterPairDistance * absSlotIndex * (pairIsOdd ? 1.0f : -1.0f);
               float y = World.instance.zedAvatarRootHeight; // move everybody up 1 unit
               float z = 0.5f * kIntraPairDistance * (rotate180 ? 1.0f : -1.0f);


               Adjustment pos = new Adjustment();
               pos.rotate180 = rotate180;
               pos.startPosition = new Vector3(x, y, z);
               pos.delta = new Vector3(0.0f, 0.0f, 0.0f);
               return pos;
          }


          public static Adjustment DancePlacement(int index, Transform transform)
          {

               Adjustment ip = StartPosition(index);
               transform.position = ip.startPosition;
               bool rotate180 = ip.rotate180;
            
               Vector3 delta = new Vector3(0.0f, 0.0f, 0.0f);

               if (rotate180) {
                    transform.RotateAround(transform.position, Vector3.up, 180);
               }
            
               //transform.rotation *= Quaternion.inverse(zRot);
               Debug.Log($"OrigPl: Index {index} pos {transform.position} rot {rotate180}");

               Adjustment adj = new Adjustment();
               adj.rotate180 = ip.rotate180;
               adj.startPosition = ip.startPosition;
               adj.delta = delta;

            
               return adj;
            
          }

          // Dance placement taking into account zed position/rotation
          public static Adjustment DancePlacement(int index,
                                                  Transform tForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot)
          {

               Adjustment ip = StartPosition(index);
               bool rotate180 = ip.rotate180;
               Vector3 pos = ip.startPosition;

            
            
               Vector3 delta = new Vector3(ip.startPosition.x - tForm.position.x,
                                           tForm.position.y,
                                           ip.startPosition.z - tForm.position.z);

               tForm.position = pos + delta;
            
               if (rotate180)
               {
                    tForm.RotateAround(pos, Vector3.up, 180);
                    tForm.position = new Vector3(tForm.position.x, tForm.position.y, tForm.position.z);
               }


               Adjustment adj = new Adjustment();
               adj.rotate180 = ip.rotate180;
               adj.startPosition = ip.startPosition;
               adj.delta = delta;

               return adj;
          }
        
          // Dance placement taking into account zed position/rotation
          public static Adjustment DancePlacement(int index,
                                                  Transform tForm,
                                                  Transform avForm,
                                                  Vector3 zPos,
                                                  Quaternion zRot) {

               Adjustment ip = StartPosition(index);
               bool rotate180 = ip.rotate180;
               Vector3 pos = ip.startPosition;
               Debug.Log($"Setting user {index} to position {pos}");
               // Vector3 delta = new Vector3(ip.startPosition.x - tForm.position.x,
               //                             0.0f,
               //                             ip.startPosition.z - tForm.position.z);
               Vector3 delta = new Vector3(0.0f, 0.0f, 0.0f);
               Transform cForm = avForm.Find("mixamorig:Hips");
            
               if (rotate180) {
                    cForm.Rotate(0, 180, 0, Space.Self);
                    tForm.position = pos + delta;
               }
               else {
                    tForm.position = pos + delta;                
               }
            
               Debug.Log("Initialized position at {transform.position}");
               Adjustment adj = new Adjustment();
               adj.rotate180 = ip.rotate180;
               adj.startPosition = ip.startPosition;
               adj.delta = delta;

               return adj;
          }

          /*
            public static Adjustment DancePlacement(int index,
            Transform tForm,
            Transform avForm,
            Vector3 zPos,
            Quaternion zRot)
            {

            const float kIntraPairDistance = 1.0f; // distance between a pair of dancers
            const float kInterPairDistance = 2.0f; // distance between pairs of dancers
            bool rotate180 = (index & 1) == 1;

            // we want to achieve indexing: 6 4 2 0 1 3 5
            int pairIndex = index >> 1;
            bool pairIsOdd = (pairIndex & 1) == 1;
            int absSlotIndex = (pairIndex + 1) >> 1;
            float x = kInterPairDistance * absSlotIndex * (pairIsOdd ? 1.0f : -1.0f);
            float y = 0.9f; // move everybody up 1 unit
            float z = 0.5f * kIntraPairDistance * (rotate180 ? -1.0f : 1.0f);

            Vector3 pos = new Vector3(x, y, z);

            Transform cForm = avForm.Find("mixamorig:Hips");
            Vector3 delta = new Vector3(x - tForm.position.x, 0.0f, z - tForm.position.z);

            //Vector3 delta = pos - cForm.position;

            if (rotate180)
            {
            cForm.Rotate(0, 180, 0, Space.Self);
            tForm.position = pos + delta + new Vector3(0.0f, 0.0f, 2.5f);
                
            }
            else {
            tForm.position = pos + delta + new Vector3(0.0f, 0.0f, -2.5f);                
            }

            Debug.Log($"Tform now {tForm.position}");

            
            Adjustment adj = new Adjustment();
            adj.rotate180 = rotate180;
            adj.posDelta = new Vector3(0.0f, 0.0f, 0.0f);
            return adj;
            }
          */



        
            
          public void CreateClient(string role, int index)
          {
               if (index == -1)
               {
                    Debug.LogError("Invalid client index: -1");
                    return;
               }

               // Client-specific
               var client = new GameObject("client " + index);
               client.transform.parent = gameObject.transform; // client is child of world
               // resize clients list and insert
               for (int i = clients.Count; i <= index; ++i)
                    clients.Add(null);

               clients[index] = client;
               // Add client component
               var clientCmp = client.AddComponent<Client>();
               clientCmp.userIdx = index;
               clientCmp.name = $"Anonymous";

               // all clients will contain signals
               var signalContainer = client.AddComponent<SignalContainer>();
               // TODO: now we need to add relevant components based on the role
               int sigIdx = 0; // e.g. here we assume ZedSignal has index of 0 in the sorted list
               var zedsignalcomponent = client.AddComponent<ZedSignal>();

               signalContainer.OnSignalAdded(zedsignalcomponent, sigIdx);

               GameObject gObj;

               if (index >= envSignal.userStates.Length) {
                    Array.Resize (ref envSignal.userStates, index + 1);
               }

               if (index == localClientUserIdx) {
                    gObj = zedsignalcomponent.LoadAvatar(appSettings.preset.username, appSettings.simpleSkeletonSmoothing);
                    envSignal.EnvUserState_SetName(localClientUserIdx, appSettings.preset.username);
               }
               else {
                    string tempName = "Client " + index;
                    gObj = zedsignalcomponent.LoadAvatar(tempName);
                
                    // If there's no username info coming from the client, then
                    // this is left blank
                    envSignal.MatchStateWithClient(index);
               }

               var matList = gObj.GetComponentsInChildren<Renderer>();
               foreach (Renderer r in matList) {
                    Debug.Log($"Found materials {r.material.name} in client {index}");
                    Color c = GetColorByIndex(index, r.material.name.Contains("Joints"));
                    r.material.color = c;
                    //(Shader.PropertyToID("Base Map"), c);
               }
            
               for (int i = localAvatarObjects.Count; i <= index; ++i)
                    localAvatarObjects.Add(null);

               localAvatarObjects[index] = gObj;
               //DancePlacement(index, client.transform);

               try
               {
                    if (DIRECT_DLL)
                    {
                         // Do we need this?
                         //DanceGraphMinimalCpp.Tick();
                    }
                    else // IPC mode
                    {
                         // For each potential user signal, see if we have anything in the IPC channel
                         if (orderedUserSignals != null)
                         {
                              foreach (var user_signal in orderedUserSignals)
                                   ; // DanceGraphMinimalCpp.TickIpc(user_signal, span); // TODO: implement TickIpc
                         }
                    }
               }
               catch (Exception e)
               {
                    Debug.LogException(e);
                    throw;
               }

               if (localClientUserIdx == -1)
               {
                    localClientUserIdx = DanceGraphMinimalCpp.GetUserIdx();
                    if (localClientUserIdx == -1) {
                         Debug.LogWarning("Client User Index remains -1 after call to native");
                    }
               }

               if (index == DanceGraphMinimalCpp.GetUserIdx())
               {
                    zedsignalcomponent.skeletonHandler.ChangeNameTag(appSettings.preset.username);
               }

#if false
               // When we get a new client, send off a 'what's this guy's user data?' and
               // we'll populate when it comes in
               if (index != localClientUserIdx) {
                    envSignal.EnvUserState_SendUserReq(index);
               }
#endif


               // For each potential env signal, see if we have anything in the IPC channel
#if false
               if (clients.Count > 0)
               {
                    ReadOnlySpan<byte> span = ReadOnlySpan<byte>.Empty;
                    var sigMeta = new SignalMetadata()
                    {
                         acquisitionTime = 0,
                         packetId = 0,
                         sigIdx = 0,
                         sigType = (byte) CallbackHandler.SignalType.Client,
                         userIdx = 0
                    };
                    CallbackHandler.ProcessSignalData(span, ref sigMeta);    
               }
#endif

               
          }

          public bool TeleportHMDToClient()
          {
               // localDevice is the HMD for the local user; localDevice.transform is the user's headset transform
               // clientInfo.clients[localClientUserIdx].headTrans is the transform of a headbone pulled from the SkeletonHandler; should be the world position
               // clientInfo.clients[localClientUserIdx].rootPos is the root position of the user in the ZEDCam's frame of reference
               // clientInfo.clients[localClientUserIdx].rootOri is the root orientation of the user in the ZEDCam's frame of reference              
               // clients[localClientUserIdx].transform is the gameworld position of the origin of the zedcam
               // In theory clientInfo.clients[localClientUserIdx].rootPos + clients[localClientUserIdx].transform.position is the root position
               // of the zedcam avatar in the gameworld

               if (localClientUserIdx < 0)
                    return false;


               if (clientInfo.clients.Count() <= localClientUserIdx)
               {
                    // Debug.Log(String.Format("Refusing to teleport to unregged client {0}",
                    //     localClientUserIdx));
                    return false;
               }


                
               GameObject xrOrigin = GameObject.Find("XR Origin");

               Transform hmdHead = localDevice.transform;

               //tr.matchOrientation = MatchOrientation.TargetUpAndForward;

               Transform clientHead = clientInfo.clients[localClientUserIdx].headTrans;

               TeleportRequest tr = new TeleportRequest();

               Debug.Log($"Client size: {clients.Count()}"); 
               Debug.Log($"Clientinfo size: {clientInfo.clients.Count()}");           
            
               Vector3 nPos = clients[localClientUserIdx].transform.position
                    + clientInfo.clients[localClientUserIdx].rootPos;

               // tr.destinationPosition = new Vector3(nPos.x,
               //     0.0f,
               //     nPos.z);

            
               tr.destinationPosition = new Vector3(clientHead.transform.position.x,
                                                    0.0f,
                                                    clientHead.transform.position.z);
            

               //Vector3 nRot = clientInfo.clients[localClientUserIdx].rootOri.eulerAngles;

               Vector3 nRot = clientInfo.clients[localClientUserIdx].headTrans.eulerAngles;

               tr.destinationRotation = Quaternion.Euler(hmdHead.rotation.x,
                                                         nRot.y,
                                                         hmdHead.rotation.z);


               tr.matchOrientation = MatchOrientation.TargetUpAndForward;

               TeleportationProvider teepee = xrOrigin.GetComponent<TeleportationProvider>();

               teepee.QueueTeleportRequest(tr);
               // Debug.Log(String.Format("Index {0} out of {1} or {2}",
               Debug.Log(String.Format("Teleporting client {2} to {0} from {1}",
                                       tr.destinationPosition,
                                       localDevice.transform.position,
                                       localClientUserIdx                                    
                              ));
               Debug.Log($"Client is at {clients[localClientUserIdx].transform.position}, {clientInfo.clients[localClientUserIdx].rootPos}, head@{clientInfo.clients[localClientUserIdx].headTrans.position}");
               // 
               initialTeleport = 2;
               return true;

          }


          public void AdjustClientToHmd()
          {
               Transform clientGameTransform = clients[localClientUserIdx].transform;
               Transform hmdTransform = localDevice.transform;

               Vector3 clientZedPos = clientInfo.clients[localClientUserIdx].rootPos;
               Vector3 clientPos = clientGameTransform.position + clientZedPos;

               Transform clientHead = clientInfo.clients[localClientUserIdx].headTrans;

               Debug.Log(String.Format("ClientHead: {0}, hmd: {1}, avatar: {2}",
                                       clientHead.position,
                                       hmdTransform.position,
                                       localAvatarObjects[localClientUserIdx].transform.position
                              ));


               // Actual avatar root position


               Vector3 adjustmentPos = hmdTransform.position -
                    localAvatarObjects[localClientUserIdx].transform.position;

               adjustmentPos.y = 0;

               localAvatarObjects[localClientUserIdx].transform.position += adjustmentPos;

          }


          public GameObject RequestClient(int index)
          {
               if (index >= clients.Count)
                    for (int i = clients.Count; i <= index; ++i)
                         clients.Add(null);
               if (clients[index] == null)
                    CreateClient("IGNORED for now -- assume zed role", index);
               return clients[index];
          }

          public void QuitApplication(String msg = "")
          {
              /*
#if UNITY_EDITOR
              //Application.Quit() does not work in the editor so
              //UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
              UnityEditor.EditorApplication.isPlaying = false;
#else
              Application.Quit();
#endif
              */
              
              // Optionally send a message to the user in the GUI and pause until they click it
              if (msg.Length > 0) {
                  gameObject.GetComponent<QuitDialog>().QuitWithMessage(msg);
              }
              
              else {
                  // save any game data here
#if UNITY_EDITOR
               // Application.Quit() does not work in the editor so
               // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                  UnityEditor.EditorApplication.isPlaying = false;                  
#else
                  
                  Application.Quit();
#endif
             }

          }

          void Update()
          {

              if (World.instance.paused)
                  return;
              
              if (Keyboard.current[Key.Escape].wasPressedThisFrame) {

                  QuitApplication();
                  //World.instance.gameObject.GetComponent<QuitDialog>().QuitWithMessage("Esc pressed");
              } 

              if (!isInitialized)
                  return;
	    
              if (localClientUserIdx == -1)
              {
                  localClientUserIdx = DanceGraphMinimalCpp.GetUserIdx();
              }

              try
              {
                    if (DIRECT_DLL)
                    {
                         DanceGraphMinimalCpp.Tick();
                    }
                    else // IPC mode
                    {
                         // For each potential user signal, see if we have anything in the IPC channel
                         if (orderedUserSignals != null)
                         {
                              foreach (var user_signal in orderedUserSignals)
                                   ; // DanceGraphMinimalCpp.TickIpc(user_signal, span); // TODO: implement TickIpc
                         }
                    }
               }
               catch (Exception e)
               {
                    Debug.LogException(e);
                    throw;
               }

               if (initialTeleport == 1)
               {

                    GameObject mCam = GameObject.Find("Main Camera");
                    //Debug.Log($"PreTele:Main Camera is {mCam.transform.position}");
                 
                    TeleportHMDToClient();

                    //Debug.Log($"PostTele:Main Camera is {mCam.transform.position}");
                    
               }

               if ((initialTeleport == 2) & appSettings.framewiseClientMatching)
               {
                    AdjustClientToHmd();
               }

          }


          public void DestroyAvatar(int clientIdx)
          {
               // Nuke the gameobject

               if (clientIdx == localClientUserIdx)
               {
                    initialTeleport = 0;

               }

               if (clients[clientIdx] != null)
               {
                    clients[clientIdx].SetActive(false);
                    clients[clientIdx] = null;
               }


               if (localAvatarObjects[clientIdx] != null)
               {
                    localAvatarObjects[clientIdx].SetActive(false);
                    localAvatarObjects[clientIdx] = null;
               }


               // We don't care what the transform is, we just need a reference to one
               clientInfo.clients[clientIdx] = new ClientData
               {
                    rootPos = Vector3.zero,
                    rootOri = new Quaternion(0f, 0f, 0f, 1f),
                    lastSeen = -1f,
                    headTrans = World.instance.transform
               };
               Debug.Log($"Destroying avatar {clientIdx}");

               // Nuke any signal handlers

               // Is it the local client?
               // If so, deal with localClient, initialTeleport
          }


          private void OnGUI()
          {
               // only display this gui if we're connected
               if (localClientUserIdx == -1)
                    return;

               GUILayout.BeginArea(new Rect(10, 10, 200, 600));
               foreach (var entry in userSignalCountMap.elements)
               {
                    GUILayout.Label($"user={entry.userIdx} pck/sec={1/entry.ma.average}");
               }

               GUILayout.Label("-------------------");
               foreach (var entry in envSignalCountMap.elements)
               {
                    GUILayout.Label($"type={entry.sigType} user={entry.userIdx} pck/sec={1/entry.ma.average}");
               }

#if false
               if (GUILayout.Button("Music toggle"))
               {
                    var m = musicPlayer.GetComponent<MusicController>();
                    m.OnTogglePlay();
                    scene.GetComponent<EnvSignal>().EnvMusicState_SetIsPlaying(m.isPlaying);
               }
#endif


               GUILayout.EndArea();
          }

          // public void _SendStruct<T>(string name, T s) where T : struct
          // {
          //     int size = Marshal.SizeOf(s);

          //     byte[] arr = new byte[size];

          //     // We should be able to do this without the alloc and copy
          //     IntPtr ptr = Marshal.AllocHGlobal(size);
          //     Marshal.StructureToPtr(s, ptr, true);
          //     Marshal.Copy(ptr, arr, 0, size);
          //     Marshal.FreeHGlobal(ptr);
          //     DanceGraphMinimalCpp.SendSignal(arr, size);
          // }
          /*
          // This doesn't work due to reference types
          // Even when the types are inlined in the struct

          Span<T> bsp = MemoryMarshal.CreateSpan<T>(ref s, 1);
          var bytespan = MemoryMarshal.Cast<T, byte>(bsp);

          DanceNetCpp.SendSignal(bytespan, size);
          */

     }

}
