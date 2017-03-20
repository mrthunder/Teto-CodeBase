using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
* Singleton Ai Manager
* By Gordon Niemann
* Beta Build - Nov 26th 2016
*/

public class AiManager : Singleton<AiManager>
{
    internal List<Unit>     m_AiList;
    internal List<Unit>     m_AiChasingPlayer;
    internal Unit           m_Player;
    internal Rigidbody      m_playerRB;
    internal Vector3        m_AvgPlayerLocation;
    internal Vector3        m_AvgTargetLocationFuture;

    private Unit            m_AlerterTokenHolder;
    private Unit            m_AttackTokenHolder;
    private float           m_fAttackTokenTimer     = 0;
    private float           m_fAlerterTokenTimer    = 0;
    private float           m_fAttackTokenCooldown  = 1.5f;
    private float           m_fAlerterTokenCooldown = 10;
    public float            m_futureForecast        = 0.5f;

    void Awake()
    {
        Unit[] units                = FindObjectsOfType<Unit>();
        IEnumerable<Unit> aiList    = from Unit unit in units where !unit.m_bIsPlayer select unit;
        m_AiList                    = aiList.ToList();
        m_AiChasingPlayer           = new List<Unit>();
        m_AvgPlayerLocation         = Vector3.zero;
        m_Player                    = GameManager.Instance.m_Player;
        m_playerRB                  = m_Player.GetComponent<Rigidbody>();

        requestQueue = new Queue<RequestData>();
        StartCoroutine(Process());

        StartCoroutine(TokenTimer());
        StartCoroutine(AvgForecastedPosition(m_futureForecast, m_Player, m_playerRB));
        StartCoroutine(ForecastedPosition(m_futureForecast));
        StartCoroutine(ChaseCamera());
    }

    List<Unit> AiNearMe(float distance, Unit me)
    {
        List<Unit> nearMe = new List<Unit>();

        foreach (Unit ai in m_AiList)
        {
            if (me.transform.position.FastDistance(ai.transform.position) < distance)
            {
                nearMe.Add(ai);
            }
        }
        return nearMe;
    }

    public Unit GetAttackTokenHolder()
    {
        return m_AttackTokenHolder;
    }

    public bool SetAttackTokenHolder(Unit newTokenHolder)
    {
        if (m_fAttackTokenTimer <= 0)
        {
            m_AttackTokenHolder = newTokenHolder;
            m_fAttackTokenTimer = m_fAttackTokenCooldown;
            return true;
        }
        return false;
    }

    public bool ReleaseAttackToken(Unit TokenHolder)
    {
        if (m_AttackTokenHolder == TokenHolder)
        {
            m_fAttackTokenTimer = 0;
            return true;
        }
        return false;
    }

    public Unit GetAlerterTokenHolder()
    {
        return m_AlerterTokenHolder;
    }

    public bool SetAlerterTokenHolder(Unit newTokenHolder)
    {
        if (m_fAlerterTokenTimer <= 0)
        {
            m_AlerterTokenHolder = newTokenHolder;
            m_fAlerterTokenTimer = m_fAlerterTokenCooldown;
            return true;
        }
        return false;
    }

    public void ChasingPlayer(Unit moster)
    {
        if (!m_AiChasingPlayer.Contains(moster))
        {
            m_AiChasingPlayer.Add(moster);
        }
    }

    public void StoppedChasingPlayer(Unit moster)
    {
        if (m_AiChasingPlayer.Contains(moster))
        {
            m_AiChasingPlayer.Remove(moster);
        }
    }

    // Removes Monster(Ai) from master list
    public void RemoveMoster(Unit moster)
    {
        m_AiList.Remove(moster);
    }

    // Creates a list of all game Monsters for quick reference
    private List<Vector3> objectLocations<T>(T[] objects) where T: MonoBehaviour
    {
        if (objects.Length > 0)
        {
            List<Vector3> objectLocations = new List<Vector3>();

            for (int x = 0; x < objects.Length; x++)
            {
                objectLocations.Add(objects[x].transform.position);
            }
            return objectLocations;
        }
        return null;
    }

    IEnumerator TokenTimer()
    {
        while (Application.isPlaying)
        {
            m_fAttackTokenTimer = m_fAttackTokenTimer - Time.deltaTime;
            m_fAlerterTokenTimer = m_fAlerterTokenTimer - Time.deltaTime;

            if (m_fAttackTokenTimer < 0)
                m_fAttackTokenTimer = 0;

            if (m_fAlerterTokenTimer < 0)
                m_fAlerterTokenTimer = 0;

            yield return null;
        }
    }

    IEnumerator AvgForecastedPosition(float sec, Unit target, Rigidbody rb)
    {
        Vector3 avgTargetVelocity;
        Vector3 targetForward;
        float granularityLevel = 0.1f;
        sec /= granularityLevel;
        int maxSteps = Mathf.RoundToInt(sec);
        int steps;

        while (Application.isPlaying)
        {
            targetForward = target.transform.forward;
            steps = maxSteps;
            avgTargetVelocity = Vector3.zero;

            while (steps-- > 0)
            {
                avgTargetVelocity += rb.velocity;
                yield return new WaitForSeconds(granularityLevel);
            }

            avgTargetVelocity /= maxSteps;
            m_AvgTargetLocationFuture = (target.transform.position + targetForward + avgTargetVelocity * 1f) - new Vector3(0, 0, 1);
            yield return null;
        }
    }

    IEnumerator ForecastedPosition(float sec)
    {
        Vector3 playerVelocity;
        Vector3 targetForward;

        while (Application.isPlaying)
        {
            targetForward = m_Player.transform.forward;
            playerVelocity = m_playerRB.velocity;
            m_AvgPlayerLocation = m_Player.transform.position + targetForward + playerVelocity * sec;
            yield return null;
        }
    }

    IEnumerator ChaseCamera()
    {
        while(Application.isPlaying)
        {
            if (m_AiChasingPlayer.Count > 0)
            {
                GameManager.Instance.m_PlayerCamera.WolfsChasing(true);
            }
            else
            {
                yield return new WaitForSeconds(3);
                if (m_AiChasingPlayer.Count == 0)
                    {
                        GameManager.Instance.m_PlayerCamera.WolfsChasing(false);
                    }
                }
            yield return null;
        }
        yield return null;
    }

    public Vector3[] MakeCheaterCirclePath(Unit unit, int lane)
    {
        Vector3[] circlePath;
        
        float Offset = ((lane + 1) * 1.5f) + 2;

        circlePath = new Vector3[8];
        circlePath[0] = unit.transform.position + (unit.transform.forward * Offset);
        circlePath[1] = unit.transform.position + ((unit.transform.forward + unit.transform.right) * (Offset - 2));
        circlePath[2] = unit.transform.position + (unit.transform.right * Offset);
        circlePath[3] = unit.transform.position + ((-unit.transform.forward + unit.transform.right) * (Offset - 2));
        circlePath[4] = unit.transform.position + (-unit.transform.forward * Offset);
        circlePath[5] = unit.transform.position + ((-unit.transform.forward + -unit.transform.right) * (Offset - 2));
        circlePath[6] = unit.transform.position + (-unit.transform.right * Offset);
        circlePath[7] = unit.transform.position + ((unit.transform.forward + -unit.transform.right) * (Offset - 2));

        //circlePath = Vec3ArrayGroundSnap(circlePath);

        return circlePath;
    }

    public Vector3[] Vec3ArrayGroundSnap(Vector3[] pathArray)
    {
        int arraySize = pathArray.Length;
        Vector3[] updatedPaths = new Vector3[arraySize];

        Vector3 path;
        NavMeshHit hit;
        RaycastHit adjustedLocation;
        Ray groundCheck;
        float offset = 0.5f;

        for (int i = 0; i < arraySize; i++)
        {
            path = pathArray[i];

            groundCheck = new Ray(path + Vector3.up, Vector3.down);
            Physics.Raycast(groundCheck, out adjustedLocation, float.MaxValue, 16);

            if (adjustedLocation.transform != null && NavMesh.SamplePosition(adjustedLocation.transform.position, out hit, 1f, NavMesh.AllAreas))
            {
                updatedPaths[i] = new Vector3(path.x, adjustedLocation.transform.position.y + offset, path.z);
            }
            else
            {
                updatedPaths[i] = Vector3.zero;
            }
        }
        return updatedPaths;
    }

    //------------------------------------- Function Time Slicing START

    public delegate void RequestCallbackDelegate(bool boolean, Vector3 vector);
    Queue<RequestData> requestQueue;

    public void Request(AiController controller, float minRange, float maxRange, RequestCallbackDelegate callback) {

        bool result = false;

        foreach (RequestData data in requestQueue)
            if (data.controller == controller)
            {
                data.Update(minRange, maxRange);
                result = true;
            }

        if(!result)
            requestQueue.Enqueue(new RequestData(controller, minRange, maxRange, callback));
    }

    private IEnumerator Process()
    {
        Vector3 randomPoint;
        NavMeshHit hit;

        RequestData data;

        Vector3 vectorResult = Vector3.zero;
        bool boolResult = false;
        int attempts = 0;

        while (Application.isPlaying)
        {
            yield return new WaitUntil(() => requestQueue.Count > 0);

            data = requestQueue.Dequeue();
            attempts = 0;
            
            /*  Future Idea
             *  heap through  all the queue (now it needs to be a list!) find the nearest wolf,
             *  resolve it and then remove it from the list
             *  this way only the farthest wolfs will wait.
             *  
             * */

            yield return new WaitUntil(() =>
            {
                attempts++;
                boolResult = false;

                if (!data.controller)
                {
                    return true;
                }

                for (int i = 0; i < 10 && !boolResult; i++)
                {
                    randomPoint = data.controller.gameObject.transform.position + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(data.minRange, data.maxRange);
                    if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        vectorResult = hit.position;
                        boolResult = true;
                    }
                }
                return boolResult || (attempts >= 6);
            });
            data.callback(boolResult, vectorResult);
        }
    }
  
    private class RequestData
    {
        public AiController controller;
        public float minRange;
        public float maxRange;
        public RequestCallbackDelegate callback;

        public RequestData(AiController controller, float minRange, float maxRange, RequestCallbackDelegate callback)
        {
            this.controller = controller;
            this.minRange = minRange;
            this.maxRange = maxRange;
            this.callback = callback;
        }
        public void Update(float minRange, float maxRange)
        {
            this.minRange = minRange;
            this.maxRange = maxRange;
        }
    }
    //-------------------------------------  Function Time Slicing END
}