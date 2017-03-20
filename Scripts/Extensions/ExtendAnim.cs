using UnityEngine;
using System.Collections;

public static class ExtendAnim
{
    public static IEnumerator OnAnimComplete(this Animator anim, string animName)
    {
        yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(0).IsName(animName));
    }
}
