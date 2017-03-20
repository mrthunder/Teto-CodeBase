using UnityEngine;

public class KillAllFlyingRavens : MonoBehaviour
{
    public void KillTheRavens()
    {
        FlyingRaven[] AllFlyingRavens;
        AllFlyingRavens = FindObjectsOfType<FlyingRaven>();
        foreach (FlyingRaven raven in AllFlyingRavens)
        {
            raven.Die();
        }
    }
}