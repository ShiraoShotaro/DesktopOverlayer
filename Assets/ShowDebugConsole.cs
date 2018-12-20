using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDebugConsole : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Debug.LogError("WARNING - This is Development Build.");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
