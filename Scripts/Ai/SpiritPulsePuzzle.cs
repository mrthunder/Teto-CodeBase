using UnityEngine;
using System.Collections;

public class SpiritPulsePuzzle : AiController, IPulseInteract
{
    public GameObject m_WayPointObj;
    private GameObject[] m_Waypoints;
    private int m_iCurrentWaypoint = 0;

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        m_Waypoints = GameObjsChildrenToArray(m_WayPointObj);
        SetState(OnNothing);
    }

    protected override IEnumerator OnStart()
    {
        yield return null;
        SetState(Puzzle);
    }

    public IEnumerator Puzzle()
    {
        while (true)
        {
            yield return new WaitUntil(() => m_Agent.SetDestination(m_Waypoints[m_iCurrentWaypoint].transform.position));
            yield return new WaitUntil(() => m_Agent.pathStatus == NavMeshPathStatus.PathComplete && m_Agent.remainingDistance == 0);
            m_iCurrentWaypoint = (m_iCurrentWaypoint + 1) % m_Waypoints.Length;
        }
    }

    public void OnPulseEnter(float pulseDistance)
    {
        FlashColorChange(true);
        SetState(this.OnWaiting, this.Puzzle);
    }

    public void OnPulseExit()
    {
        FlashColorChange(false);
    }
}