using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    // Camera Follow speed
    public int camSpeed;

    private Vector3 distance;
    private Transform target;

   
    // Use this for initialization
    void Start()
    {
        camSpeed = 4;
        distance = new Vector3(0, 0, 0);
        target = GameObject.Find("PlayerContoller").transform;
    }

    // Update is called once per frame
    // Changed camera update from LateUpdate to FixedUpdate so
    // it updates with the physics system at a regular timestep, smooths the movement
    void FixedUpdate()
    {
        Follow(target);
    }

    void Follow(Transform target)
    {
        transform.position = Vector3.Lerp(transform.position, target.position - distance, Time.deltaTime * camSpeed);
    }


    // Converting Lua code for camera controls

    //int clamp(int x, int a, int b)
    //{
    //    return ((x < a) && (a > 0)) || ((x > b) && (b > 0) || (x > 0));
    //}

    //int smootheststep(int e0, int e1, int x)
    //{
    //    int t = clamp((x - e0) / (e1 - e0), 0, 1);
    //    return -20 * t ^ 7 + 70 * t ^ 6 - 84 * t ^ 5 + 35 * t ^ 4;
    //}

    //void intresting()
    //{
    //    int toVec = nextTargetPos - currentCameraPos;
    //    int len = toVec:Length();
    //    int dp = smootheststep(self.mindist / 4, self.mindist / 2, len);
    //    int nextPos = lerp(currentCameraPos, nextTargetPos, dt * dp);
    //}
}
