using UnityEngine;
using UnityEngine.InputSystem;

using System;
using System.IO;
using System.Collections.Generic;

using DanceGraph.SignalBehaviours;
using DanceGraph.SignalSerialization;

namespace DanceGraph
{
     public class MusicController : MonoBehaviour
     {
          public Key nextTrackKey;         
          public Key toggleMusicKey;

          private int currentTrack = 0;
          public readonly String [] trackList = {"DanceGraphPlayList/Del Rio Bravo", "DanceGraphPlayList/Cumbish", "DanceGraphPlayList/Notanico Merengue_Slow"};
        
          private AudioSource audioSource;
        
          private void Start()
          {

               audioSource = GetComponent<AudioSource>();
               currentTrack = 0;
               Debug.Log($"MusicController started with track {currentTrack}");

			   double audioStart = GetAudioStart(0.0);
			   // Provisional startup music state; we don't send it unless it's asked for
			   World.instance.scene.GetComponent<EnvSignal>().EnvMusicState_SetTrack(trackList[currentTrack], audioStart, false);
			   World.instance.scene.GetComponent<EnvSignal>().EnvMusicState_SetIsPlaying(true, false);
               PlayCurrentTrack();
          }

          void Update()
          {
               //ttps://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Migration.html
               if (Keyboard.current[toggleMusicKey].wasPressedThisFrame)
                    OnTogglePlay();
               if (Keyboard.current[nextTrackKey].wasPressedThisFrame)
                    OnNextTrack();
          }

          public void SetMusicTime(float t) {
			  Debug.Log($"MusicSource set to time {t}");
			  audioSource.time = t;
          }

          public double GetAudioStart() {
			  var m = World.instance.scene.GetComponent<EnvSignal>();
			  
			  TimeSpan currentTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));			  
			  double uSeconds = currentTime.TotalSeconds;

			  var startTime = TimeConverter.LocalToServerDoubleTime(uSeconds) - m.musicState._musicTime;	  
			  return startTime;
          }

          public double GetAudioStart(double audioOffset) {
			  TimeSpan currentTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));			  			  
			  double uSeconds = currentTime.TotalSeconds;
			  var startTime = TimeConverter.LocalToServerDoubleTime(uSeconds) - audioOffset;
			  return startTime;
          }

          // This should not be triggered by an environment signal
          public void OnTogglePlay()
          {
               if (audioSource.isPlaying)
               {
                    audioSource.Pause();
                    World.instance.scene.GetComponent<EnvSignal>().EnvMusicState_SetIsPlaying(false);
                    Debug.Log($"MusicController: Music paused at {audioSource.timeSamples}");
               }
               else
               {
                    // If we're restarting the music, concoct a fake start time based on the audioSource.time
                
                    // var audioOffset = (float) ( audioSource.timeSamples * audioSource.clip.length / audioSource.clip.samples);
                    // var fakeTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds - audioOffset;
					var fakeTime = GetAudioStart();
                    var m = World.instance.scene.GetComponent<EnvSignal>();
                    m.musicState._musicTime = TimeConverter.LocalToServerDoubleTime(fakeTime);
                    m.EnvMusicState_SetIsPlaying(true);
                    audioSource.Play();
                    Debug.Log($"Music restart using fake time ${fakeTime}");
               }
          }

          public void OnNextTrack() {
            
               currentTrack = (currentTrack + 1) % trackList.Length;
            
               PlayCurrentTrack();

               var m = World.instance.scene.GetComponent<EnvSignal>();
               var nowTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
               m.EnvMusicState_SetTrack(trackList[currentTrack], TimeConverter.LocalToServerDoubleTime(nowTime));
          }

          public void PlayCurrentTrack() {
               Debug.Log($"MusicController: Starting to play track {trackList[currentTrack]}");
               AudioClip res = Resources.Load(trackList[currentTrack]) as AudioClip;

            
               if (res is null) {
                    Debug.Log($"MusicController: Warning, Failed to load resource at {trackList[currentTrack]}");
               }


               audioSource.clip = res;
            
               audioSource.Play();

          }

        
          // Called when an environment has been updated from the network
          public void OnMusicEnvironmentUpdate(EnvMusicState musicState)
          {
               Debug.Log("MusicController: Music Environment Update");

               // TimeSpan currentTime = DateTime.Now.Subtract(new DateTime(1970,1,1,0,0,0));
               // ulong uMilliseconds = (ulong) currentTime.TotalMilliseconds;
               // var uSecOffset = musicState._musicTime - TimeConverter.LocalToServerTime(uMilliseconds);
               // var sampleOffset = (int) ( fSecOffset* audioSource.clip.samples / audioSource.clip.length);
               // sampleOffset = sampleOffset % audioSource.clip.samples;
               // audioSource.timeSamples = sampleOffset;

               var newTrackNum = Array.IndexOf(trackList, musicState._trackName);
            
               if (newTrackNum < 0) {
                    Debug.Log($"MusicController: Warning, remote attempt to set unknown track {musicState._trackName}");
                    return;
               }
               else if (currentTrack != newTrackNum) {
                    currentTrack = newTrackNum;
                    PlayCurrentTrack();
               }
            
               TimeSpan currentTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
               double uSeconds = currentTime.TotalSeconds;

               var m = World.instance.scene.GetComponent<EnvSignal>();
               var secOffset = TimeConverter.LocalToServerDoubleTime(uSeconds) - m.musicState._musicTime;
               var sampleOffset = (int) (secOffset * audioSource.clip.samples / audioSource.clip.length);

               // In Dancegraph, the music just never, ever, ever stops
               sampleOffset = sampleOffset % audioSource.clip.samples;

               if (sampleOffset >= 0) {
				   Debug.Log($"Music Sample set to offset {sampleOffset} ({secOffset} secs)");
				   audioSource.timeSamples = sampleOffset;                
               }
               else {
                    Debug.LogWarning($"MusicController: Impossible sample offset {secOffset}, setting to 0.0");
                    audioSource.timeSamples = 0;
               }

               // Also set audioSource.track
            
               if (!musicState._isPlaying)
               {
                    audioSource.Stop();
                    Debug.Log("MusicController: Music paused at {secOffset}");
               }
               else
               {
                    audioSource.Play();
                    Debug.Log($"MusicController: Music resumed at {secOffset}");                
               }

          }
     }
}

