// @filippsen
// public domain

using UnityEngine;
using System.Collections.Generic;
using System;

public class LomontWilliams : MonoBehaviour 
{
	// Call Lomont-Williams action
	void PlayLW (string parameterSet) 
	{
		// Remove spaces if any, split commas
		string[] parameters = parameterSet.Replace(" ", string.Empty).Split(',');

		// check parameters
		if (parameters.Length == 8) 
		{
			// Call actual Lomont-Williams method
			List<byte> rawSoundList = Sound1 (Convert.ToByte(parameters[0], 16), 
			                                  Convert.ToByte(parameters[1], 16), 
			                                  Convert.ToByte(parameters[2], 16), 
			                                  Convert.ToByte(parameters[3], 16), 
			                                  Convert.ToByte(parameters[4], 16), 
			                                  Convert.ToByte(parameters[5], 16), 
			                                  Convert.ToByte(parameters[6], 16), 
			                                  Convert.ToUInt16(parameters[7], 16)
			                                  );

			// Convert wave data to byte array and then to float
			byte[] rawSound = rawSoundList.ToArray ();
			float[] soundData = ConvertByteToFloat (rawSound);

			// Create clip, attach to new audio source and play.
			const int williamsSampleRate = 894750;
			AudioClip audioClip = AudioClip.Create ("sound", soundData.Length, 1,  williamsSampleRate/ 4, false, false);
			audioClip.SetData (soundData, 0);
			AudioSource audioSource = this.gameObject.AddComponent<AudioSource> ();
			audioSource.clip = audioClip;
			audioSource.Play ();
		} 
		else 
		{
			//TODO: FIXME: 
			Debug.LogWarning("wrong params");
		}
	}

	// standard byte to float conversion
	public float[] ConvertByteToFloat(byte[] byteArray) 
	{
		int len = byteArray.Length / 4;
		float[] floatArray = new float[len];

		for (int i = 0; i < byteArray.Length-4; i+=4) 
		{
			//TODO: FIXME: ugly division
			// Convert to float and to Unity's [-1,1] data range (crucial)
			floatArray[ (int)(i/4)] = (127 - BitConverter.ToSingle(byteArray, i))/128f;
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
		byte c1, c2, t; // internal storage
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

	// Front end
	public string parameterSet = "0x40, 0x01, 0x00, 0x10, 0xE1, 0xFF, 0xFF, 0x0080";
	void OnGUI() 
	{
		parameterSet = GUI.TextField(new Rect(10, 10, 330, 20), parameterSet, 50);
		if (GUI.Button (new Rect (10, 50, 50, 30), "Play")) 
		{
			PlayLW(parameterSet);
		}

		//premades
		if (GUI.Button (new Rect (100, 50, 100, 30), "Load #1")) 
		{
			parameterSet = "0x40, 0x01, 0x00, 0x10, 0xE1, 0xFF, 0xFF, 0x0080";
		}
		if (GUI.Button (new Rect (100, 100, 100, 30), "Load #2")) 
		{
			parameterSet = "0x28, 0x01, 0x00, 0x08, 0x81, 0xFF, 0xFF, 0x0200";
		}
		if (GUI.Button (new Rect (100, 150, 100, 30), "Load #3")) 
		{
			parameterSet = "0x28, 0x81, 0x00, 0xFC, 0x01, 0xFC, 0xFF, 0x0200";
		}
		if (GUI.Button (new Rect (100, 200, 100, 30), "Load #4")) 
		{
			parameterSet = "0xFF, 0x01, 0x00, 0x18, 0x41, 0x00, 0xFF, 0x0480";
		}
		if (GUI.Button (new Rect (100, 250, 100, 30), "Load #5")) 
		{
			parameterSet = "0x00, 0xFF, 0x08, 0xFF, 0x68, 0x00, 0xFF, 0x0480";
		}
	}
}
