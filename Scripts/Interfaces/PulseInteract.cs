using UnityEngine;
using System.Collections;

public interface IPulseInteract  {

    /// <summary>
    /// What will happend when the pulse interact
    /// </summary>
    void OnPulseEnter(float distance);

    void OnPulseExit();


}
