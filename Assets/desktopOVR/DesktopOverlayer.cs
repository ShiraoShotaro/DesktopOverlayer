/**
 * OpenVR Overlay samlpe by gpsnmeajp v0.2
 * 2018/08/25
 * 
 * v0.1 公開
 * v0.2 エラーチェックが不完全だった問題を修正。RenderTextureが無効なままセットしていた問題を修正
 * 
 * 2DのテクスチャをVR空間にオーバーレイ表示します。
 * 現在動作中のアプリケーションに関係なくオーバーレイすることができます。
 * 
 * 入力機能は正常に動作していないようなので省いています。
 * ダッシュボードオーバーレイは省略しています。
 * 
 * 各メソッドの詳細はValveSoftwareのIVROverlay_Overviewを確認してください。
 * https://github.com/ValveSoftware/openvr/wiki/IVROverlay_Overview
 *
 * These codes are licensed under CC0.
 * http://creativecommons.org/publicdomain/zero/1.0/deed.ja
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR; //Steam VR

public class DesktopOverlayer : MonoBehaviour
{
	//エラーメッセージの名前
	const string Tag = "[OverlaySample]";

	//グローバルキー(システムのオーバーレイ同士の識別名)。
	//ユニークでなければならない。乱数やUUIDなどを勧める
	const string OverlayKey = "DesktopOverlayer_v0.0.1";

	//ユーザーが確認するためのオーバーレイの名前
	const string OverlayFriendlyName = "DesktopOverlayer_v0.0.1";

	//オーバーレイのハンドル(整数)
	ulong overlayHandle = 0;

	//OpenVRシステムインスタンス
	CVRSystem openvr;

	//Overlayインスタンス
	CVROverlay overlay;

	//オーバーレイに渡すネイティブテクスチャ
	Texture_t overlayTexture;

	//上下反転フラグ
	int textureYflip = 1;

	HmdMatrix44_t transform_matrix;

	//HMD視点位置変換行列
	HmdMatrix34_t pose;

	//取得元のRenderTexture
	public RenderTexture renderTexture;

	[Range(0.1f,10f)]
	public float scale = 1.0f;

	[Range(-10f, 10f)]
	public float posX;

	[Range(-10f, 10f)]
	public float posY;

	[Range(-10f, 10f)]
	public float posZ = -0.2f;

	[Range(-360f, 360f)]
	public float rotH = 0.0f;

	[Range(-90f, 90f)]
	public float rotV = 0.0f;

	[Range(-180f, 180f)]
	public float rotDisp = 0.0f;

	public bool enable;
	public void DisplayEnable()
	{
		//オーバーレイを表示する
		if (overlay != null)
		{
			overlay.ShowOverlay(overlayHandle);
		}
	}
	public void DisplayDisable()
	{
		//オーバーレイを非表示する
		if (overlay != null)
		{
			overlay.HideOverlay(overlayHandle);
		}
	}

	public bool zoom_enable;
	public void ZoomEnalbe() { zoom_enable = true; }
	public void ZoomDisable() { zoom_enable = false; }

	public Slider ScaleSlider;
	public Slider PosXSlider;
	public Slider PosYSlider;
	public Slider PosZSlider;
	public Slider RotHSlider;
	public Slider RotVSlider;
	public Slider RotDispSlider;

	HmdMatrix44_t mul(HmdMatrix44_t a, HmdMatrix44_t b)
	{
		HmdMatrix44_t ret;
		ret.m0 = a.m0 * b.m0 + a.m1 * b.m4 + a.m2 * b.m8  + a.m3 * b.m12;
		ret.m1 = a.m0 * b.m1 + a.m1 * b.m5 + a.m2 * b.m9  + a.m3 * b.m13;
		ret.m2 = a.m0 * b.m2 + a.m1 * b.m6 + a.m2 * b.m10 + a.m3 * b.m14;
		ret.m3 = a.m0 * b.m3 + a.m1 * b.m7 + a.m2 * b.m11 + a.m3 * b.m15;
		ret.m4 = a.m4 * b.m0 + a.m5 * b.m4 + a.m6 * b.m8  + a.m7 * b.m12;
		ret.m5 = a.m4 * b.m1 + a.m5 * b.m5 + a.m6 * b.m9  + a.m7 * b.m13;
		ret.m6 = a.m4 * b.m2 + a.m5 * b.m6 + a.m6 * b.m10 + a.m7 * b.m14;
		ret.m7 = a.m4 * b.m3 + a.m5 * b.m7 + a.m6 * b.m11 + a.m7 * b.m15;

		ret.m8  = a.m8  * b.m0 + a.m9  * b.m4 + a.m10 * b.m8  + a.m11 * b.m12;
		ret.m9  = a.m8  * b.m1 + a.m9  * b.m5 + a.m10 * b.m9  + a.m11 * b.m13;
		ret.m10 = a.m8  * b.m2 + a.m9  * b.m6 + a.m10 * b.m10 + a.m11 * b.m14;
		ret.m11 = a.m8  * b.m3 + a.m9  * b.m7 + a.m10 * b.m11 + a.m11 * b.m15;
		ret.m12 = a.m12 * b.m0 + a.m13 * b.m4 + a.m14 * b.m8  + a.m15 * b.m12;
		ret.m13 = a.m12 * b.m1 + a.m13 * b.m5 + a.m14 * b.m9  + a.m15 * b.m13;
		ret.m14 = a.m12 * b.m2 + a.m13 * b.m6 + a.m14 * b.m10 + a.m15 * b.m14;
		ret.m15 = a.m12 * b.m3 + a.m13 * b.m7 + a.m14 * b.m11 + a.m15 * b.m15;

		return ret;
	}

	public void ResetDisplaySettings()
	{
		ScaleSlider.value = 1f;
		PosXSlider.value = 0f;
		PosYSlider.value = 0f;
		PosZSlider.value = -2f;
		RotHSlider.value = 0f;
		RotVSlider.value = 0f;
		RotDispSlider.value = 0f;
		ChangedSlidersValue();
	}

	public void ChangedSlidersValue()
	{
		scale = ScaleSlider.value;
		posX = PosXSlider.value;
		posY = PosYSlider.value;
		posZ = PosZSlider.value;
		rotH = RotHSlider.value;
		rotV = RotVSlider.value;
		rotDisp = RotDispSlider.value;

		//行列の再計算

		// 拡大行列
		HmdMatrix44_t scale_mat;
		scale_mat.m0 = scale; scale_mat.m1 = 0; scale_mat.m2 = 0; scale_mat.m3 = 0;
		scale_mat.m4 = 0; scale_mat.m5 = scale * textureYflip; scale_mat.m6 = 0; scale_mat.m7 = 0;
		scale_mat.m8 = 0; scale_mat.m9 = 0; scale_mat.m10 = scale; scale_mat.m11 = 0;
		scale_mat.m12 = 0; scale_mat.m13 = 0; scale_mat.m14 = 0; scale_mat.m15 = 1;

		// 平行移動行列
		HmdMatrix44_t translate_mat;
		translate_mat.m0 = 1; translate_mat.m1 = 0; translate_mat.m2 = 0; translate_mat.m3 = posX;
		translate_mat.m4 = 0; translate_mat.m5 = 1; translate_mat.m6 = 0; translate_mat.m7 = posY;
		translate_mat.m8 = 0; translate_mat.m9 = 0; translate_mat.m10 = 1; translate_mat.m11 = posZ;
		translate_mat.m12 = 0; translate_mat.m13 = 0; translate_mat.m14 = 0; translate_mat.m15 = 1;

		// 水平回転行列(Y)
		HmdMatrix44_t rotH_mat;
		float roth_rad = Mathf.Deg2Rad * rotH;
		rotH_mat.m0 = Mathf.Cos(roth_rad); rotH_mat.m1 = 0; rotH_mat.m2 = Mathf.Sin(roth_rad); rotH_mat.m3 = 0;
		rotH_mat.m4 = 0; rotH_mat.m5 = 1; rotH_mat.m6 = 0; rotH_mat.m7 = 0;
		rotH_mat.m8 = -Mathf.Sin(roth_rad); rotH_mat.m9 = 0; rotH_mat.m10 = Mathf.Cos(roth_rad); rotH_mat.m11 = 0;
		rotH_mat.m12 = 0; rotH_mat.m13 = 0; rotH_mat.m14 = 0; rotH_mat.m15 = 1;

		// 垂直回転行列(X)
		HmdMatrix44_t rotV_mat;
		float rotv_rad = Mathf.Deg2Rad * rotV;
		rotV_mat.m0 = 1; rotV_mat.m1 = 0; rotV_mat.m2 = 0; rotV_mat.m3 = 0;
		rotV_mat.m4 = 0; rotV_mat.m5 = Mathf.Cos(rotv_rad); rotV_mat.m6 = -Mathf.Sin(rotv_rad); rotV_mat.m7 = 0;
		rotV_mat.m8 = 0; rotV_mat.m9 = Mathf.Sin(rotv_rad); rotV_mat.m10 = Mathf.Cos(rotv_rad); rotV_mat.m11 = 0;
		rotV_mat.m12 = 0; rotV_mat.m13 = 0; rotV_mat.m14 = 0; rotV_mat.m15 = 1;
		
		// ディスプレイ回転行列(Z)
		HmdMatrix44_t rotDisp_mat;
		float rotdisp_rad = Mathf.Deg2Rad * rotDisp;
		rotDisp_mat.m0 = Mathf.Cos(rotdisp_rad); rotDisp_mat.m1 = -Mathf.Sin(rotdisp_rad); rotDisp_mat.m2 = 0; rotDisp_mat.m3 = 0;
		rotDisp_mat.m4 = Mathf.Sin(rotdisp_rad); rotDisp_mat.m5 = Mathf.Cos(rotdisp_rad); rotDisp_mat.m6 = 0; rotDisp_mat.m7 = 0;
		rotDisp_mat.m8 = 0; rotDisp_mat.m9 = 0; rotDisp_mat.m10 = 1; rotDisp_mat.m11 = 0;
		rotDisp_mat.m12 = 0; rotDisp_mat.m13 = 0; rotDisp_mat.m14 = 0; rotDisp_mat.m15 = 1;

		HmdMatrix44_t ret = translate_mat;
		ret = mul(ret, scale_mat);
		ret = mul(ret, rotH_mat);
		ret = mul(ret, rotV_mat);
		ret = mul(ret, rotDisp_mat);
		//ret = mul(ret, rotH_mat);
		//ret = mul(ret, rotDisp_mat);
		//ret = mul(ret, translate_mat);
		//ret = mul(ret, rot_mat);

		pose.m0 = ret.m0; pose.m1 = ret.m1; pose.m2 = ret.m2; pose.m3 = ret.m3;
		pose.m4 = ret.m4; pose.m5 = ret.m5; pose.m6 = ret.m6; pose.m7 = ret.m7;
		pose.m8 = ret.m8; pose.m9 = ret.m9; pose.m10 = ret.m10; pose.m11 = ret.m11;

	}

	void Start()
	{
		var openVRError = EVRInitError.None;
		var overlayError = EVROverlayError.None;


		//OpenVRの初期化
		openvr = OpenVR.Init(ref openVRError, EVRApplicationType.VRApplication_Overlay);
		if (openVRError != EVRInitError.None)
		{
			Debug.LogError(Tag + "OpenVRの初期化に失敗." + openVRError.ToString());
			ApplicationQuit();
			return;
		}

		//オーバーレイ機能の初期化
		overlay = OpenVR.Overlay;
		overlayError = overlay.CreateOverlay(OverlayKey, OverlayFriendlyName, ref overlayHandle);
		if (overlayError != EVROverlayError.None)
		{
			Debug.LogError(Tag + "Overlayの初期化に失敗. " + overlayError.ToString());
			ApplicationQuit();
			return;
		}


		//オーバーレイの大きさ設定(幅のみ。高さはテクスチャの比から自動計算される)
		var width = 2.0f;
		overlay.SetOverlayWidthInMeters(overlayHandle, width);
		//オーバーレイの不透明度を設定
		var alpha = 0.9f;
		overlay.SetOverlayAlpha(overlayHandle, alpha);

		//オーバーレイに渡すテクスチャ種類の設定
		var isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
		if (isOpenGL)
		{
			//pGLuintTexture
			overlayTexture.eType = ETextureType.OpenGL;
			//上下反転しない
			textureYflip = 1;
		}
		else
		{
			//pTexture
			overlayTexture.eType = ETextureType.DirectX;
			//上下反転する
			textureYflip = -1;
		}

		//変換行列の初期設定
		ChangedSlidersValue();

		//オーバーレイを表示する
		overlay.ShowOverlay(overlayHandle);

		Debug.Log(Tag + "初期化完了しました");
	}

	void Update()
	{
		//初期化失敗するなどoverlayが無効な場合は実行しない
		if (overlay == null)
		{
			return;
		}

		//オーバーレイが表示されている時
		if (overlay.IsOverlayVisible(overlayHandle))
		{
			//HMD視点位置変換行列に書き込む。
			//ここでは回転なし、平行移動ありのHUD的な状態にしている。
			

			//回転行列を元に、HMDからの相対的な位置にオーバーレイを表示する。
			//代わりにSetOverlayTransformAbsoluteを使用すると、ルーム空間に固定することができる
			uint indexunTrackedDevice = 0;//0=HMD(他にControllerやTrackerにすることもできる)
			//overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, indexunTrackedDevice, ref pose);
			overlay.SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref pose);

			//RenderTextureが生成されているかチェック
			if (!renderTexture.IsCreated())
			{
				Debug.Log(Tag + "RenderTextureがまだ生成されていない");
				return;
			}

			//RenderTextureからネイティブテクスチャのハンドルを取得
			try
			{
				overlayTexture.handle = renderTexture.GetNativeTexturePtr();
			}
			catch (UnassignedReferenceException e)
			{
				Debug.LogError(Tag + "RenderTextureがセットされていません");
				ApplicationQuit();
				return;
			}

			//オーバーレイにテクスチャを設定
			var overlayError = EVROverlayError.None;
			overlayError = overlay.SetOverlayTexture(overlayHandle, ref overlayTexture);
			if (overlayError != EVROverlayError.None)
			{
				Debug.LogError(Tag + "Overlayにテクスチャをセットできませんでした. " + overlayError.ToString());
				ApplicationQuit();
				return;
			}
		}


	}

	void OnApplicationQuit()
	{
		//アプリケーション終了時にOverlayハンドルを破棄する
		if (overlay != null)
		{
			overlay.DestroyOverlay(overlayHandle);
		}
		//VRシステムをシャットダウンする
		OpenVR.Shutdown();
	}

	//アプリケーションを終了させる
	void ApplicationQuit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}