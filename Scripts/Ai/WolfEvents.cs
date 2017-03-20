using UnityEngine;
using System.Collections;

public class WolfEvents : MonoBehaviour
{
    public void FootSteps()
    {
        GetComponentInParent<Wolf>().SoundWolfFootSteps();
    }
}