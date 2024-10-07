 #define ENVADAPTER
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using DanceGraph;
using UnityEngine;
using UnityEngine.Rendering;

//using Unity.Mathematics;

public class DanceGraphMinimalCpp
{
#if UNITY_EDITOR

    // Handle to the C++ DLL
    public IntPtr libraryHandle;

    public delegate void InitBindingsDelegate(IntPtr debugLog);
    public delegate void RegisterSignalCallbackDelegate(IntPtr callback);
        
    // delegate pairs
        
    public delegate void InitializeDelegate();
    public static InitializeDelegate Initialize;
        
    public delegate void DisconnectDelegate();
    public static DisconnectDelegate Disconnect;
        
    public delegate bool ConnectDelegate(string name, string serverIp, int serverPort, int localPort, string scene_name, string user_role);
    public static ConnectDelegate Connect;
    
    public delegate bool ConnectJsonDelegate(string jsonString);
    public static ConnectJsonDelegate ConnectJson;
        
    public delegate void SetZedReplayDelegate(string name);
    public static SetZedReplayDelegate SetZedReplay;
        
    public delegate void SetLogLevelDelegate(int level);
    public static SetLogLevelDelegate SetLogLevel;
        
    public delegate void SetLogToFileDelegate(string logFilename);
    public static SetLogToFileDelegate SetLogToFile;

    public delegate void NativeDebugLogDelegate(string logFilename);
    public static NativeDebugLogDelegate NativeDebugLog;
        
    public delegate int GetUserIdxDelegate();
    public static GetUserIdxDelegate GetUserIdx;
    
        
#if ENVADAPTER
    public delegate void SendSignalDelegate(byte[] stream, int numBytes);
    public static SendSignalDelegate SendSignal;
        
    public delegate int GetEnvSignalDelegate(byte[] stream);
    public static GetEnvSignalDelegate GetEnvSignal;
        
    public delegate bool ReadLocalEnvDataDelegate();
    public static ReadLocalEnvDataDelegate ReadLocalEnvData;
#endif
    public delegate void SendEnvSignalDelegate(byte[] stream, int numBytes, int sigIdx);
    public static SendEnvSignalDelegate SendEnvSignal;

    public delegate void TickDelegate();
    public static TickDelegate Tick;

    public delegate void GetLastDanceGraphErrorDelegate(StringBuilder str, int len);
    public static GetLastDanceGraphErrorDelegate GetLastDanceGraphError;
    
#else
        
#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        static extern void InitBindings(IntPtr debugLog);


#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        static extern void RegisterSignalCallback(IntPtr cb);

    // player delegates
        #if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void Initialize();
    
        #if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void Disconnect();
    
        #if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern bool Connect(string name, string serverIp, int serverPort, int localPort, string scene_name, string user_role);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern bool ConnectJson(string jsonString);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void SetZedReplay(string name);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void SetLogLevel(int level);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void SetLogToFile(string logFilename);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern int GetUserIdx();
    
#if ENVADAPTER

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void SendSignal(byte[] stream, int numBytes);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern int GetEnvSignal(byte[] stream);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern bool ReadLocalEnvData();
#endif

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
        #endif
        public static extern void SendEnvSignal(byte[] stream, int numBytes, int sigIdx);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
#endif
        public static extern void Tick();
    
#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
#endif
        public static extern void NativeDebugLog(string s);

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    #else
        [DllImport ("dancegraph-minimal")]
#endif
    //public static extern void GetLastDanceGraphError(string str, int len);
        public static extern void GetLastDanceGraphError(StringBuilder str, int len);

#endif


#if UNITY_EDITOR_WIN

    [DllImport("kernel32")]
    public static extern IntPtr LoadLibrary(
        string path);

    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(
        IntPtr libraryHandle,
        string symbolName);

    [DllImport("kernel32")]
    public static extern bool FreeLibrary(
        IntPtr libraryHandle);

    public static IntPtr OpenLibrary(string path)
    {
        Debug.Log("Attempting to open native library at " + Application.dataPath);
        IntPtr handle = LoadLibrary(path);
        if (handle == IntPtr.Zero)
        {
            throw new Exception("Couldn't open native library: " + path);
        }
        return handle;
    }

    public static void CloseLibrary(IntPtr libraryHandle)
    {
        FreeLibrary(libraryHandle);
    }

    public static T GetDelegate<T>(
        IntPtr libraryHandle,
        string functionName) where T : class
                                       {
                                           IntPtr symbol = GetProcAddress(libraryHandle, functionName);
                                           if (symbol == IntPtr.Zero)
                                           {
                                               throw new Exception("Couldn't get function: " + functionName);
                                           }
                                           return Marshal.GetDelegateForFunctionPointer(
                                               symbol,
                                               typeof(T)) as T;
        }

#else
 
 
#endif

    /*
      C# functions callable from c++
    */
    delegate void DebugLogDelegate(string str);
    unsafe delegate void ProcessSignalDataDelegate(byte* data, int len, dg.sig.SignalMetadata sigMeta);

    public void Awake()
    {
        // Copy file from DanceGraph to plugins folder
        var dll_filename = "dancegraph-minimal.dll";
        var plugins_folder = Application.dataPath + "/Plugins/x86_64";
        var dll_src = World.DANCEGRAPH_PATH + "/modules/" + dll_filename;
        var dll_dst = plugins_folder + "/" + dll_filename;
        System.IO.File.Copy(dll_src, dll_dst,true);
        Debug.Log($"Copying DLL from {dll_src} to {dll_dst}");
        
#if UNITY_EDITOR

        /*
          c++-call-from-c# declarations as usual
        */
        // Open native library
        libraryHandle = OpenLibrary(dll_dst);
        
        InitBindingsDelegate InitBindings = GetDelegate<InitBindingsDelegate>(
            libraryHandle,
            "InitBindings");
            
        RegisterSignalCallbackDelegate RegisterSignalCallback = GetDelegate<RegisterSignalCallbackDelegate>(
            libraryHandle,
            "RegisterSignalCallback");
                        
        Initialize = GetDelegate<InitializeDelegate>(
            libraryHandle,
            "Initialize");
    
        Disconnect = GetDelegate<DisconnectDelegate>(
            libraryHandle,
            "Disconnect");
    
        Connect = GetDelegate<ConnectDelegate>(
            libraryHandle,
            "Connect");
        
        ConnectJson = GetDelegate<ConnectJsonDelegate>(
            libraryHandle,
            "ConnectJson");
    
        SetZedReplay = GetDelegate<SetZedReplayDelegate>(
            libraryHandle,
            "SetZedReplay");
    
        SetLogLevel = GetDelegate<SetLogLevelDelegate>(
            libraryHandle,
            "SetLogLevel");
    
        SetLogToFile = GetDelegate<SetLogToFileDelegate>(
            libraryHandle,
            "SetLogToFile");
    
        GetUserIdx = GetDelegate<GetUserIdxDelegate>(
            libraryHandle,
            "GetUserIdx");
    
#if ENVADAPTER
        SendSignal = GetDelegate<SendSignalDelegate>(
            libraryHandle,
            "SendSignal");
        
        ReadLocalEnvData = GetDelegate<ReadLocalEnvDataDelegate>(
            libraryHandle,
            "ReadLocalEnvData");
        
        GetEnvSignal = GetDelegate<GetEnvSignalDelegate>(
            libraryHandle,
            "GetEnvSignal");
#endif
        SendEnvSignal = GetDelegate<SendEnvSignalDelegate>(
            libraryHandle,
            "SendEnvSignal");

        NativeDebugLog = GetDelegate<NativeDebugLogDelegate>(
            libraryHandle,
            "NativeDebugLog");

        Tick = GetDelegate<TickDelegate>(
            libraryHandle,
            "Tick");

        GetLastDanceGraphError = GetDelegate<GetLastDanceGraphErrorDelegate>(
            libraryHandle,
            "GetLastDanceGraphError");
    
#else
        

#endif

        // Init C++ library: Call C++ function to register c#-call-from-c++ funcs
        InitBindings(
            Marshal.GetFunctionPointerForDelegate(new DebugLogDelegate(DebugLog))
        );
        
        unsafe
        {
            callback  = new ProcessSignalDataDelegate(ProcessSignalData);    
        }
        if(callback == null)
            Debug.LogError("ProcessSignalData is null");
        fptr = Marshal.GetFunctionPointerForDelegate(callback);
        if(fptr == null)
            Debug.LogError("ProcessSignalData fptr is null");
        RegisterSignalCallback( fptr);
    }
    
    private IntPtr fptr;
    private ProcessSignalDataDelegate callback ;

    public void Close()
    {
        Disconnect();
#if UNITY_EDITOR
        CloseLibrary(libraryHandle);
        libraryHandle = IntPtr.Zero;
#endif
    }

    ////////////////////////////////////////////////////////////////
    // C# functions callable from C++
    ////////////////////////////////////////////////////////////////

    
    internal class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute() { }
    }
    
    [MonoPInvokeCallback]
    static void DebugLog(string str)
    {
        Debug.Log(str);
    }    
    
    [MonoPInvokeCallback]
    unsafe static void ProcessSignalData(byte* data_ptr, int len, dg.sig.SignalMetadata sigMeta)
    {
        //*p *= *p;
        ReadOnlySpan<byte> data = new ReadOnlySpan<byte>((byte*)data_ptr, len);
        dg.sig.CallbackHandler.ProcessSignalData(data, in sigMeta);
    }
}

public class DanceGraphMinimalCppBehaviour : MonoBehaviour
{
    DanceGraphMinimalCpp dll = new DanceGraphMinimalCpp();
    void Awake() => dll.Awake();
    void OnApplicationQuit() => dll.Close();
}
