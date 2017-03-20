using UnityEngine;
using DG.Tweening;

public class TemplePassageClose : MonoBehaviour
{
    public float m_fTime = 1;

    public void CloseTemple()
    {
        transform.DOLocalMoveX(-32, m_fTime);
    }
}
