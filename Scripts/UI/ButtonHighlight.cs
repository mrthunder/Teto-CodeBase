using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour {

    public void CursorHighlight()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("CursorOver");
        }
    }
}
