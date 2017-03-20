using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MeshFaceCamera : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        try
        {

            //transform.LookAt(Camera.main.transform, Vector3.up);
            transform.forward = Camera.main.transform.forward;
        }
        catch
        {

        }
        
    }

}
