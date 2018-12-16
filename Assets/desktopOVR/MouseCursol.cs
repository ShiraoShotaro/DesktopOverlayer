using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;

public class MouseCursol : MonoBehaviour {

	public GameObject CursorObject;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//var position = Input.mousePosition;
		var position = new Vector3(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, -0.1f);
		
		//1920px : 2
		position = position * 3.2f / 1920.0f;
		position.x -= 1.6f;
		position.y -= 0.9f;
		position.y *= -1;
		Debug.Log(position);
		CursorObject.transform.position = position;

	}
}
