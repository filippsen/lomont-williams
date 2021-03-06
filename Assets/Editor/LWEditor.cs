﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;


public class LWEditor : EditorWindow
{
	byte[] byteParam = {0x28, 1, 0x00, 0x08, 0x81, 0xFF, 0xFF} ;
	ushort shortParam = 0x200;
	AudioClip clip;
	string filename = "soundFile";

	[MenuItem("Window/LWEditor")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(LWEditor));
	}

	void OnGUI()
	{
		for(int i=0;i<7;i++)
		{
			byteParam[i] = (byte)EditorGUILayout.Slider ("Slider "+i, byteParam[i], 0, 255);
		}
		shortParam = (ushort)EditorGUILayout.Slider ("Slider 16bit", shortParam, 0, 16384);
		GUILayout.BeginHorizontal("box");
		if(GUILayout.Button("Play", GUILayout.ExpandWidth (false)))
		{
			clip = PlayLW(byteParam, shortParam);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		if(GUILayout.Button( "Save"))
		{
			if(clip.length > 0.0f)
			{
				filename = EditorUtility.SaveFilePanel("Choose Wav File", "", filename, "wav");
				if(filename.Length!=0)
				{
					Debug.Log(filename);
					SavWav.Save(filename,clip);
				}
			}
		}
		GUILayout.Label (filename);
		GUILayout.EndHorizontal();
	}
	// Call Lomont-Williams action
	AudioClip PlayLW (byte[] byteParam, ushort shortParam) 
	{

		// Call actual Lomont-Williams method
		List<byte> rawSoundList = Sound1 (byteParam[0],
		                                  byteParam[1],
		                                  byteParam[2],
		                                  byteParam[3],
		                                  byteParam[4], 
		                                  byteParam[5], 
		                                  byteParam[6], 
		                                  shortParam
		                                  );
		
		// Convert wave data to byte array and then to float
		byte[] rawSound = rawSoundList.ToArray ();
		float[] soundData = ConvertByteToFloat (rawSound);
		
		// Create clip, attach to new audio source and play.
		const int williamsSampleRate = 894750;
		AudioClip audioClip = AudioClip.Create ("sound", soundData.Length, 1,  williamsSampleRate/ 4, false, false);
		audioClip.SetData (soundData, 0);
		PlayClip (audioClip);
		return audioClip;
	}
	
	// standard byte to float conversion
	float[] ConvertByteToFloat(byte[] byteArray) 
	{
		int len = byteArray.Length / 4;
		float[] floatArray = new float[len];
		
		for (int i = 0; i < byteArray.Length-4; i+=4) 
		{
			//TODO: FIXME: ugly division
			// Convert to float and to Unity's [-1,1] data range (crucial)
			floatArray[ (int)(i/4)] = Mathf.Clamp01((127 - BitConverter.ToSingle(byteArray, i))/128f);
		}
		
		return floatArray;
	}
	
	// Taken from http://www.lomont.org/Software/Misc/Robotron/
	/// <summary>
	/// This function reproduces an algorithm from the Williams Sound ROM, addresses 0xF503 to 0xF54F
	/// It takes 7 byte parameters and one 16-bit parameter, and returns a list of sound values 0-255
	/// sampled at 894750 samples per second
	/// </summary>
	List<byte> Sound1(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, ushort u1)
	{
		ushort count;   // internal counter
		byte c1, c2; // internal storage
		byte sound = 0; // current sound level
		var wave = new List<byte>();
		// copy the current sound value this many times into the output
		Action<int> dup = d =>
		{
			while (d-- > 0)
			wave.Add(sound);
		};
		
		dup(8);
		sound = b7;
		do
		{
			dup(14);
			c1 = b1;
			c2 = b2;
			do
			{
				dup(4);
				count = u1;
				while (true)
				{
					dup(9);
					sound = (byte) ~sound;
					
					ushort t1 = (c1 != 0 ? c1 : (ushort)256);
					dup(Mathf.Min(count, t1)*14 - 6);
					if (count <= t1)
						break;
					dup(12);
					count -= t1;
					
					sound = (byte) ~sound;
					
					ushort t2 = (c2 != 0 ? c2 : (ushort)256);
					dup(Mathf.Min(count, t2) * 14 - 3);
					if (count <= t2)
						break;
					dup(10);
					count -= t2;
				}
				
				dup(15);
				
				if (sound < 128)
				{
					dup(2);
					sound = (byte)~sound;
				}
				
				dup(27);
				c1 += b3;
				c2 += b4;
			} while (c2 != b5);
			
			dup(7);
			if (b6 == 0) break;
			dup(11);
			b1 += b6;
		} while (b1 != 0);
		return wave;
	}


	public static void PlayClip(AudioClip clip) 
	{
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo method = audioUtilClass.GetMethod(
			"PlayClip",
			BindingFlags.Static | BindingFlags.Public,
			null,
			new System.Type[] 
			{
			typeof(AudioClip)
		},
		null
		);
		
		method.Invoke(
			null,
			new object[] 
			{
				clip
			}
		);
	}
}

