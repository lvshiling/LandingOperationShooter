using UnityEngine;
using System.Collections;

public class Resizer : MonoBehaviour {

	public GameObject[] btn;
	public float w = 4f;
	public float h = 3f;
	float k = 1f;

	Vector3[] vbtn; 
	float cw;
	float ch;
	float ck;

	public void ResizeGUI() {
		k = w / h;
		cw = Screen.width;
		ch = Screen.height;
		ck = cw / ch;
		for (int i = 0; i < btn.Length; i++) {
			btn[i].transform.position = new Vector3(vbtn[i].x * ck / k, vbtn[i].y, vbtn[i].z);
		}
	}

	public void Awake() {
		vbtn = new Vector3[btn.Length];
		for (int i = 0; i < btn.Length; i++) {
			vbtn[i] = btn[i].transform.position;
		}
		ResizeGUI ();

	}

	public void Update() {
		if (Input.GetKeyDown (KeyCode.U)) {
			ResizeGUI();
		}
	}

}