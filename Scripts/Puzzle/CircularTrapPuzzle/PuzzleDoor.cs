using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

/// <summary>
/// This door only works with the circular trap.
/// </summary>
[AddComponentMenu("Puzzle/Circular Trap Puzzle/Puzzle Door")]
public class PuzzleDoor : MonoBehaviour
{

    /// <summary>
    /// All the traps that will affect this door
    /// </summary>
    public CircularTrap[] m_Traps;

    /// <summary>
    /// Max percentage to open the door
    /// </summary>
    private float m_fMaxPercentage;

    /// <summary>
    /// Camera that will showcase the moment of the door activation.
    /// </summary>
    public GameObject m_CinematicCamera;

    /// <summary>
    /// Final position of the door.
    /// </summary>
    public Vector3 m_vDoorFinalPos = Vector3.zero;

    /// <summary>
    /// Duration of the door movement.
    /// </summary>
    public float m_fMovementDuration = 2f;

    // Use this for initialization
    void Start()
    {

        m_fMaxPercentage = 100 * m_Traps.Length;

        StartCoroutine(CheckTraps());
    }

    IEnumerator CheckTraps()
    {

        float percent = 0;
        do
        {
            yield return null;
            try
            {
                percent = m_Traps.Sum(x => x.GetPercentage());
            }
            catch
            {
                percent = 0;
            }
           

        } while (percent < m_fMaxPercentage);
        OpenDoor();

    }

    void OpenDoor()
    {
        transform.DOMove(m_vDoorFinalPos, m_fMovementDuration).SetRelative();
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + m_vDoorFinalPos, transform.localScale);
    }
#endif

}
