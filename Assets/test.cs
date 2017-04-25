using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //테스트용
        if (Input.GetKeyDown(KeyCode.A))
        {
            SceneChangeManager.Instance.OnSceneChange("ScTest001");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SceneChangeManager.Instance.OnSceneChange("ScTest002");
        }
    }
}
