using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Test : MonoBehaviour {

	[DllImport("testUnity")]
	private static extern float foofoo(float a, float b);

	void Awake(){
		Debug.Log(foofoo(5,foofoo(5,20)));
	}
}
