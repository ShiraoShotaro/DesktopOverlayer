using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class DesktopCapture : MonoBehaviour
{
	private Grabber grabber;
	private Renderer rend;
	private Texture2D texture;

	// Use this for initialization
	void Start()
	{
		Debug.Log("[DesktopCapture]Start Func.");
		Debug.Log("[DesktopCapture]Get Render Component.");
		rend = GetComponent<Renderer>();
		rend.enabled = true;
		var materials = rend.materials;

		Debug.Log("[DesktopCapture]create a default 2D texture.");
		texture = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);

		Debug.Log("[DesktopCapture]create new Grabber instance.");
		grabber = new Grabber(texture.GetNativeTexturePtr());
		Debug.Log ("[DesktopCapture]Monitor = Width: " + grabber.width + " Height: " + grabber.height);

		IntPtr dest_tex = grabber.texture;
		Debug.Log ("[DesktopCapture]destination texture address = " + dest_tex);

		Debug.Log("[DesktopCapture]create external texture.");
		texture = Texture2D.CreateExternalTexture(grabber.width, grabber.height, TextureFormat.BGRA32, false, false, grabber.texture);

		Debug.Log("[DesktopCapture]Attach the texture to main material's texture.");
		materials[0].mainTexture = texture;
		//texture.EncodeToPNG ();
		//texture.Apply (true);

		Debug.Log("[DesktopCapture]Finish Start Func.");
	}

	// OnWillRenderObject is called once for each camera if the object is visible.
	void OnWillRenderObject()
	{
		try
		{
			grabber.GetNextFrame(texture.GetNativeTexturePtr());
		}catch(NullReferenceException e)
		{
			Debug.LogError(e.Message);
			Debug.LogError(e.StackTrace);
		}
		//Texture2D.CreateExternalTexture (grabber.width, grabber.height, TextureFormat.BGRA32, 0, true, nativeTex);
		//texture.UpdateExternalTexture(grabber.texture);
		//texture.Apply ();
	}

	class Grabber
	{
		[DllImport("NativeLibTest")]
		private static extern IntPtr grabber_create(IntPtr texture);
		[DllImport("NativeLibTest")]
		private static extern void grabber_destroy(IntPtr grabber);
		[DllImport("NativeLibTest")]
		private static extern int grabber_get_next_frame(IntPtr grabber, IntPtr texture);
		[DllImport("NativeLibTest")]
		private static extern int grabber_get_width(IntPtr grabber);
		[DllImport("NativeLibTest")]
		private static extern int grabber_get_height(IntPtr grabber);
		[DllImport("NativeLibTest")]
		private static extern IntPtr grabber_get_dest_tex(IntPtr grabber);

		private IntPtr grabber;

		internal Grabber(IntPtr nativeTex)
		{
			grabber = grabber_create(nativeTex);
			if (grabber.ToInt64() == 0)
			{
				throw new Exception("grabber_create failed");
			}
		}

		~Grabber()
		{
			grabber_destroy(grabber);
		}

		internal int GetNextFrame(IntPtr nativeTex)
		{
			return grabber_get_next_frame(grabber, nativeTex);
		}

		internal int width
		{
			get { return grabber_get_width(grabber); }
		}

		internal int height
		{
			get { return grabber_get_height(grabber); }
		}

		internal IntPtr texture
		{
			get { return grabber_get_dest_tex(grabber); }
		}
	}
}
