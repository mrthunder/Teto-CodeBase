using UnityEngine;
using System.Collections;

public class RayLookat : MonoBehaviour {

	// Update is called once per frame
	void Update ()
    {
        if (Camera.current == null) return;
        transform.LookAt(new Vector3(Camera.current.transform.position.x, transform.position.y, Camera.current.transform.position.z));
    }
}
