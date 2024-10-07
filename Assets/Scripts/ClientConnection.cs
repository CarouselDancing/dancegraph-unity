//#define DEVELOPER_MODE
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using cfg.rt;
using DanceGraph;
using Newtonsoft.Json;
using UnityEngine;

public class ClientConnection : MonoBehaviour
{
    public float startTime;
    public bool connected = false;
    private GUIStyle labelStyle;

#if DEVELOPER_MODE
    public string localIp;
    public int localPort;
    public cfg.rt.PresetDb presetDb;
#endif

    void OnEnable()
    {
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(net.SignalMetadata));
        Debug.Log("Size of metadata: " + size);
    }

    void OnDisable()
    {
    }

    private void Start()
    {

#if DEVELOPER_MODE
        var presetsFilename = World.DANCEGRAPH_PATH + "/dancegraph_rt_presets.json";
        if (System.IO.File.Exists(presetsFilename))
        {
            try
            {
                presetDb = Newtonsoft.Json.JsonConvert.DeserializeObject<cfg.rt.PresetDb>(System.IO.File.ReadAllText(presetsFilename));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
#endif
        startTime = Time.time;
    }

    private cfg.rt.Client attemptConnectionWithPreset;
    private void Update()
    {
        if (attemptConnectionWithPreset != null)
        {
            Debug.Log("Connecting");
            World.instance.InitScene(attemptConnectionWithPreset.scene);
            try
            {
                var presetText = JsonConvert.SerializeObject(attemptConnectionWithPreset, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                connected = DanceGraphMinimalCpp.ConnectJson(presetText);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                Debug.LogError("Plugin exception: DanceGraph.Connect");
            }

            if(!connected) {
                StringBuilder netErr = new StringBuilder(150);
                //DanceGraphMinimalCpp.GetLastDanceGraphError(netErr.ToString(), netErr.Capacity);
                DanceGraphMinimalCpp.GetLastDanceGraphError(netErr, netErr.Capacity);
                World.instance.QuitApplication(netErr.ToString());
              }
            else
                Destroy(this);
        }
            
    }

    private void OnGUI()
    {
        var appSettings = World.instance?.appSettings;
        // is world not initialized yet?
        if (appSettings == null)
            return;
        // are we trying to connect?
        if (attemptConnectionWithPreset != null)
            return;
        if (labelStyle == default)
        {
            labelStyle = GUI.skin.label;
            labelStyle.wordWrap = true;
        }

        string portString = "7777";
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 450));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");        
        appSettings.preset.username = GUILayout.TextField(appSettings.preset.username, 42);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Server IP: ");        
        appSettings.preset.server_address.ip = GUILayout.TextField(appSettings.preset.server_address.ip, 120);
        
        GUILayout.Label("Port: ");                
        portString = GUILayout.TextField(portString, 40);   
        GUILayout.EndHorizontal();

        if ((!appSettings.autoConnect && GUILayout.Button("Connect")) || (appSettings.autoConnect && ((Time.time - startTime) > 1.0f))) // allow a second with autoconnect
        {
            appSettings.preset.server_address.port = int.Parse(portString);
            attemptConnectionWithPreset = appSettings.preset;
        }

#if DEVELOPER_MODE

        foreach (var (preset_name, preset_data) in presetDb.client)
        {
            if (GUILayout.Button($"Load preset {preset_name}"))
            {
                World.instance.InitScene(preset_data.scene);
                try
                {
                    preset_data.include_ipc_consumers = false; // NO IPC!
                    var presetText = JsonConvert.SerializeObject(preset_data, Formatting.Indented, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    connected = DanceGraphMinimalCpp.ConnectJson(presetText);
                }
                catch (Exception e)
                {
                    Debug.LogError("Plugin exception: DanceGraph.ConnectJson");
                }
            }
        }
#endif

        GUILayout.EndArea();

        
    }
}


