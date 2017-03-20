using UnityEngine;
using System.Collections;

public class PlayerAnimSounds : MonoBehaviour {

    PlayerCinematic cinematic;

    public bool parent = true;

    public bool playSound = false;

    public void FootSteps()
    {
        if (!playSound) return;
        GameObject obj = this.gameObject;
        if (parent) obj = this.transform.parent.gameObject;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Footsteps, obj);
    }
    public void Sniff()
    {
        if (!playSound) return;
        GameObject obj = this.gameObject;
        if (parent) obj = this.transform.parent.gameObject;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Sniff,obj);
    }
}
