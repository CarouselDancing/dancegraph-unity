using System;
using UnityEngine;
using UnityEditor;

namespace DanceGraph
{
    // Just a class that interrupts the Application Quit process to tell the 
    // user why their application has bombed out
    
    public class QuitDialog : MonoBehaviour
    {
        private bool displayed = false;

        private string quitMessage = "";

        public void QuitWithMessage(String s) {
            quitMessage = s;
            displayed = true;

            Time.timeScale = 0;
            if (World.instance) {
                World.instance.paused = true;
                World.instance.enabled = false; // Suppresses World.Update() even harder

                ClientConnection cc = World.instance.gameObject.GetComponent<ClientConnection>();
                cc.enabled = false;
            }
        }

        private void OnGUI() {
            if (!displayed)
                return;

            GUILayout.BeginArea(new Rect (100, 100, 800, 400));
            GUILayout.BeginVertical();
            GUIStyle gs = new GUIStyle();
            gs.richText = true;
            GUILayout.Label($"Application Quit: <color=red>{quitMessage}</color>", gs);
            
            bool b = GUILayout.Button("QUIT");
            DateTime dt = System.DateTime.Now;
            
            if (b) {
                Debug.Log("Application final Quit upon GUI interaction");
#if UNITY_EDITOR                            
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

    }
}
