using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RotateAround : MonoBehaviour
{
    [Tooltip("Circle Size"), Space(2f), Header("Scanning & Location")]
    public float m_fCircleSize = 10;

    [Tooltip("Flying Speed")]
    public float m_fFlyingSpeed = 10;

    private Vector3 m_PivotPoint;
    internal bool m_bPaused = false;

    float posInCircle;


    // Use this for initialization
    void Start ()
    {
        m_PivotPoint = new Vector3(0,0, m_fCircleSize) + transform.position;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_bPaused)
        {
            //posInCircle += m_fFlyingSpeed * Time.deltaTime;
            transform.RotateAround(m_PivotPoint, Vector3.up, m_fFlyingSpeed * Time.deltaTime);
            //transform.position = m_PivotPoint + Quaternion.Euler(0, posInCircle, 0) * new Vector3(0, 0, m_fCircleSize);
            //transform.rotation = Quaternion.Euler(0, posInCircle, 0);
        }
    }

#if (UNITY_EDITOR)

    public List<Vector3> MakeCirclePath(Vector3 axis, float size = 4, int totalCircumferencePoints = 12)
    {
        List<Vector3> pathS = new List<Vector3>();
        Vector3 path;

        for (int i = 0; i < totalCircumferencePoints; i++)
        {
            float stepSize = Mathf.PI * 2 / totalCircumferencePoints;
            float posX = Mathf.Cos(i * stepSize);
            float posZ = Mathf.Sin(i * stepSize);
            path = new Vector3((posX * size) + axis.x, axis.y, (posZ * size) + axis.z);
            pathS.Add(path);
        }
        return pathS;
    }

    List<Vector3> m_GizmoPaths;
    float m_fGizmoCircleSize;

    private void Awake()
    {
        CircleGizmoPath();
        m_fGizmoCircleSize = m_fCircleSize;
    }

    private void LateUpdate()
    {
        if (m_fCircleSize != m_fGizmoCircleSize)
        {
            m_fGizmoCircleSize = m_fCircleSize;
            CircleGizmoPath();
        }
    }

    private void CircleGizmoPath()
    {
        m_GizmoPaths = MakeCirclePath(transform.position + new Vector3(0, 0, m_fGizmoCircleSize), Mathf.RoundToInt(m_fGizmoCircleSize));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach(Vector3 path in m_GizmoPaths)
        {
            Gizmos.DrawSphere(path, 1);
        }
    }

#endif

}
