using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;

[System.Serializable]
public struct GrassPoints
{
    public Vector3 pointsPos;

    public Vector2 rnd;
    public float opacity;
    

}


public class GrassBillboard : MonoBehaviour, IPulseInteract, IVeinInteract
{

    public enum DrawShape { Square, Circle, Custom };
    [Header("Area Shape:")]
    [Tooltip("Shape of the area where the grass will be draw")]
    public DrawShape m_Shape;
    [Header("Square")]
    public Vector2 m_vSize;
    public Vector2 m_vOffset;
    internal Bounds m_GrassBounds;
    [Header("Circle")]
    public float m_Radius = 2f;
    [Header("Custom")]
    public Vector3[] m_DrawPoints = new Vector3[] { new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f) };

    private int m_iTextureLength = 0;
    [Tooltip("Shader for the grass.")]
    private Shader m_Shader;

    private Material m_mat;

    public float m_fDensity = 0.4f;
    ComputeBuffer m_OutputBuffer;

    public Texture m_grassTexture;
    public Texture m_grassCutOff;

    public GrassPoints[] m_Points;
    public GrassPoints[] m_ShowPoints;
    int index = 0;
    float lerptex = 0;


    [Header("Environmental Transition")]
    public LightVein m_LightVein;

    private PlayerContoller m_player;

    private LightPulse m_pulse;

    private bool m_bShow = false;

    private bool _Activated = false;
    /// <summary>
    /// If the grass area is already activated by a light vein or not
    /// </summary>
    internal bool m_bActivated
    {
        get
        {
            return _Activated;
        }
    }

    public bool m_bIsTransparent = true;

    /// <summary>
    /// Index of all grass point that is shown
    /// </summary>
    List<int> grassIndex = new List<int>();

    private bool m_bIsRendering = false;
    private int m_iLOD = 100;

    /// <summary>
    /// The index where should be instanciating the grass
    /// </summary>
    private int m_iShowIndex = 0;

    /// <summary>
    /// Make an area not have grass
    /// </summary>
    [Header("Exclude Area:"),Tooltip("If you need to not have grass in one place inside your polygon")]
    public bool m_bWillExcludePoints = false;

    /// <summary>
    /// area that will not have grass
    /// </summary>
    [Tooltip("Area")]
    public Vector3[] m_ExcludePoints = new Vector3[] { new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f) };

    // Use this for initialization
    void Awake()
    {
        transform.eulerAngles = Vector3.zero;
        if (!m_bIsTransparent)
        {
            _Activated = true;

        }
        m_Shader = Shader.Find("Custom/Waving");
        if (m_Shader == null)
        {
            throw new System.Exception("You forgot the shader.");
        }
        
        m_mat = new Material(m_Shader);

        m_player = GameManager.Instance.m_Player;
        if (m_player)
        {
            m_pulse = m_player.GetComponent<LightPulse>();
        }


      
        

        SetBounds();
        m_Points = CalculatePoints();

        m_ShowPoints = new GrassPoints[m_Points.Length];

        m_ShowPoints = (GrassPoints[])m_Points.Clone();

        for (int i = 0; i < m_ShowPoints.Length; i++)
        {
            m_ShowPoints[i].opacity = 1;
        }



        m_OutputBuffer = new ComputeBuffer(m_Points.Length, 24);
        m_OutputBuffer.SetData(m_Points);
        if (m_bIsTransparent && m_player != null)
        {
            StartCoroutine(ShowGrass());
        }



    }


    void SetBounds()
    {
        Vector3 size = new Vector3(m_vSize.x, 1, m_vSize.y);
        Vector3 center = Vector3.zero;
        if (m_Shape == DrawShape.Custom)
        {
            IOrderedEnumerable<float> xPoints = m_DrawPoints.Select(x => x.x).OrderBy(x => x);
            IOrderedEnumerable<float> yPoints = m_DrawPoints.Select(x => x.y).OrderBy(x => x);
            IOrderedEnumerable<float> zPoints = m_DrawPoints.Select(x => x.z).OrderBy(x => x);


            var rect = new Rect();
            rect.xMin = xPoints.Min();
            rect.xMax = xPoints.Max();
            rect.yMin = zPoints.Min();
            rect.yMax = zPoints.Max();

            size.x = rect.size.x;
            size.y = (yPoints.Max() - yPoints.Min()) + 1;
            size.z = rect.size.y;

            center = new Vector3(rect.center.x, 0, rect.center.y);
        }
        RaycastHit hit;

        m_GrassBounds = new Bounds(center, size);

        if (Physics.Raycast(center + transform.position, Vector3.down, out hit, 100f, LayerMask.GetMask("EnvironmentalTransition")))
        {
            center = (hit.point - transform.position) + Vector3.up;
        }
        if (m_Shape == DrawShape.Circle)
        {
            SphereCollider collider = this.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = m_Radius;
        }
        else
        {
            BoxCollider collider = this.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.center = center;
        }


    }





    #region Environmental Transition


    public void OnPulseEnter(float distance)
    {

        if (!m_bActivated)
        {
            m_bShow = true;

        }
    }

    IEnumerator ShowGrass()
    {
        yield return new WaitUntil(() => m_bShow);
        Vector3 playerpos = m_player.transform.position;
        Vector3 objPosition = transform.position;
        float pulseRange = Mathf.Pow(m_pulse.m_fPulseMaxRange, 2);
        while (Application.isPlaying)
        {

            playerpos = m_player.transform.position;
            for (int i = 0; i < m_Points.Length; i++)
            {
                float distance = ((m_Points[i].pointsPos + objPosition) - playerpos).sqrMagnitude;
                if (distance < pulseRange)
                {
                    grassIndex.Add(i);
                    m_Points[i].opacity = 1;
                }
            }
            m_OutputBuffer.SetData(m_Points);

            yield return null;
            yield return new WaitUntil(() => m_bShow || m_bActivated);
            if (m_bActivated)
            {
                yield break;
            }

        }
    }

    IEnumerator LightVeinsActivation()
    {

        Vector3 lvPos = m_LightVein.transform.position;
        float increment = m_LightVein.m_fIncrement;
        float range = 0;
        bool lvShow = true;
        do
        {
            Vector3 grassPos = m_Points[m_iShowIndex].pointsPos+transform.position;

            if(Vector3.Distance(lvPos,grassPos) < range && m_Points[m_iShowIndex].opacity == 0)
            {
                m_Points[m_iShowIndex].opacity = 1;
            }
            else
            {
                lvShow = true;
            }
           
            
            m_iShowIndex = (m_iShowIndex+1)% m_Points.Length;
            if (m_iShowIndex == 0)
            {
                lvShow = false;
                range += increment;
                m_OutputBuffer.SetData(m_Points);
                yield return null;
            }
        } while (lvShow || m_iShowIndex+1 < m_Points.Length);

        yield return null;
       

    }

    public void OnPulseExit()
    {
        m_bShow = false;
        if (!m_bActivated)
        {
            StartCoroutine(HideGrass());
        }
    }

    IEnumerator HideGrass()
    {
        grassIndex = grassIndex.Distinct().ToList();
        while (grassIndex.Count != 0)
        {
            m_Points[grassIndex[0]].opacity = 0;
            grassIndex.RemoveAt(0);
            m_OutputBuffer.SetData(m_Points);
            yield return null;
        }
    }

    public void OnLightVeinInteract(float distance)
    {
        if (m_bActivated) return;
        _Activated = true;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Restoration_Grass, this.gameObject);
        StopAllCoroutines();
        if(m_LightVein)
        {
            StartCoroutine(LightVeinsActivation());
        }
        else
        {
            m_OutputBuffer.SetData(m_ShowPoints);
        }
        

    }





    #endregion

    #region Render

    public void OnRenderObject()
    {
            m_mat.SetPass(0);
            m_mat.SetTexture("_MainTex", m_grassTexture);
            m_mat.SetTexture("_CuttOffTex", m_grassCutOff);
            m_mat.SetBuffer("buf_Points", m_OutputBuffer);
            m_mat.SetFloat("_height", m_fDensity);
            m_mat.SetVector("_Size", m_vSize);
            m_mat.SetVector("_WorldPos", transform.position);

            Graphics.DrawProcedural(MeshTopology.Points, m_OutputBuffer.count);
    }

    public void OnDestroy()
    {
        StopAllCoroutines();
        m_OutputBuffer.Release();
    }


    GrassPoints[] CalculatePoints()
    {
        if (m_Shape == DrawShape.Circle)
        {
            return CalculateCirclePoints();
        }
        else if (m_Shape == DrawShape.Square)
        {
            return CalculateSquarePoints((int)m_vSize.x, (int)m_vSize.y, m_GrassBounds.center);
        }
        else
        {
            return CalculateCustomPoints();
        }
    }

    GrassPoints[] CalculateSquarePoints(int sizeX, int sizeZ, Vector3 center)
    {
        int x = (int)(sizeX / m_fDensity);

        int y = (int)(sizeZ / m_fDensity);

        List<GrassPoints> grass = new List<GrassPoints>();

        Vector3 worldCenter = transform.TransformPoint(center);

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                GrassPoints grassPoint = new GrassPoints();
                grassPoint.pointsPos = new Vector3((i * m_fDensity) + Random.Range(-0.2f, 0.2f), 0, (j * m_fDensity) + Random.Range(-0.2f, 0.2f)) - (new Vector3(x * m_fDensity, 0, y * m_fDensity) * 0.5f) + center;
                RaycastHit hit;

                if (Physics.Raycast(grassPoint.pointsPos + transform.position, Vector3.down, out hit, 100f, LayerMask.GetMask("EnvironmentalTransition")))
                {
                    Vector3 height = hit.point - transform.position;
                    grassPoint.pointsPos = height;
                    grassPoint.rnd = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                    grassPoint.opacity = (m_bIsTransparent ? 0 : 1);
                    grass.Add(grassPoint);

                }


            }
        }


        return ExcludingGrass(grass);
    }

    GrassPoints[] CalculateCirclePoints()
    {
        List<GrassPoints> grassList = new List<GrassPoints>(CalculateSquarePoints((int)m_Radius, (int)m_Radius, Vector3.zero));


        grassList.RemoveAll(grassObj => Vector3.Distance(Vector3.zero, grassObj.pointsPos) > m_Radius);

        return ExcludingGrass(grassList);
    }

    GrassPoints[] CalculateCustomPoints()
    {
        List<GrassPoints> grassList = new List<GrassPoints>(CalculateSquarePoints((int)m_GrassBounds.size.x, (int)m_GrassBounds.size.z, m_GrassBounds.center));


        grassList.RemoveAll(grassObj => !PointInPolygon(grassObj.pointsPos, m_DrawPoints));

        return ExcludingGrass(grassList);
    }

    GrassPoints[] ExcludingGrass(List<GrassPoints> array)
    {
        if(m_bWillExcludePoints)
            array.RemoveAll(grassObj => PointInPolygon(grassObj.pointsPos, m_ExcludePoints));
        return array.ToArray();
    }


    /// <see href="http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/">Website</see>
    public bool PointInPolygon(Vector3 point, Vector3[] array)
    {
        // Get the angle between the point and the
        // first and last vertices.
        int max_point = array.Length - 1;

        float total_angle = GetAngle(
            array[max_point].x, array[max_point].z,
            point.x, point.z,
            array[0].x, array[0].z);

        // Add the angles from the point
        // to each other pair of vertices.
        for (int i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(
                array[i].x, array[i].z,
                point.x, point.z,
                array[i + 1].x, array[i + 1].z);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.

        return (Mathf.Abs(total_angle) > 0.000001);
    }

    /// <see href="http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/">Website</see>
    public static float GetAngle(float Ax, float Ay,
     float Bx, float By, float Cx, float Cy)
    {
        // Get the dot product.
        float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

        // Get the cross product.
        float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        // Calculate the angle.
        return (float)Mathf.Atan2(cross_product, dot_product);
    }

    /// <see href="http://csharphelper.com/blog/2014/07/determine-whether-a-point-is-inside-a-polygon-in-c/">Website</see>
    private static float DotProduct(float Ax, float Ay,
    float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the dot product.
        return (BAx * BCx + BAy * BCy);
    }

    /// <see href="http://csharphelper.com/blog/2014/07/determine-whether-a-polygon-is-convex-in-c/">Website</see>
    public static float CrossProductLength(float Ax, float Ay,
    float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the Z coordinate of the cross product.
        return (BAx * BCy - BAy * BCx);
    }

#if UNITY_EDITOR
    GUIStyle style = new GUIStyle();
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        if (m_Shape == DrawShape.Square)
        {
            Gizmos.DrawWireCube(center, new Vector3(m_vSize.x, 1, m_vSize.y));
        }
        else if (m_Shape == DrawShape.Circle)
        {
            Gizmos.DrawWireSphere(center, m_Radius);
        }
        else
        {
            if (m_DrawPoints.Length > 0)
            {
                for (int i = 0; i < m_DrawPoints.Length; i++)
                {

                    Gizmos.DrawSphere(m_DrawPoints[i] + center, 0.3f);
                    if (i == 0)
                    {
                        Gizmos.DrawLine(m_DrawPoints[m_DrawPoints.Length - 1] + center, m_DrawPoints[i] + center);
                    }
                    else
                    {
                        Gizmos.DrawLine(m_DrawPoints[i - 1] + center, m_DrawPoints[i] + center);
                    }
                }
            }
        }

        if (m_bWillExcludePoints && m_ExcludePoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < m_ExcludePoints.Length; i++)
            {
                Gizmos.DrawSphere(m_ExcludePoints[i] + center, 0.3f);
                if (i == 0)
                {
                    Gizmos.DrawLine(m_ExcludePoints[m_ExcludePoints.Length - 1] + center, m_ExcludePoints[i] + center);
                }
                else
                {
                    Gizmos.DrawLine(m_ExcludePoints[i - 1] + center, m_ExcludePoints[i] + center);
                }
            }
        }

        if (Application.isPlaying && m_GrassBounds != null)
        {
            Gizmos.DrawSphere(transform.TransformPoint(m_GrassBounds.center), 0.5f);
        }
        else
        {
            Gizmos.DrawSphere(center, 0.1f);
        }


    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        style.fontSize = 20;
        if (m_DrawPoints.Length > 0)
        {
            
            for (int i = 0; i < m_DrawPoints.Length; i++)
            {
                

                UnityEditor.Handles.Label((m_DrawPoints[i] + center + Vector3.up), i.ToString(), style);

            }
        }
        if(m_bWillExcludePoints && m_ExcludePoints.Length>0)
        {
            for (int i = 0; i < m_ExcludePoints.Length; i++)
            {
                UnityEditor.Handles.Label((m_ExcludePoints[i] + center + Vector3.up), i.ToString(), style);
            }
        }
    }

#endif
    #endregion

    #region Collision

    public void OnTriggerEnter(Collider other)
    {
        if (_Activated)
            SoundManager.Instance.SetSwitch(Surfaces.Grass_High, other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        if (_Activated)
            SoundManager.Instance.SetSwitch(Surfaces.Rock, other.gameObject);
    }

    #endregion


}
