using UnityEngine;
using System.Collections;

public class DieAfter5Sec : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        Destroy(this, 5);
	}
}
