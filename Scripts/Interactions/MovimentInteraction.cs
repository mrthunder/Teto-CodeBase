using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class MovimentInteraction : MonoBehaviour
{
    

    [Tooltip("When the player press foward this event is going to trigger all method")]
    /// <summary>
    /// When the player press foward this event is going to trigger all method
    /// </summary>
    public UnityEvent OnFoward = new UnityEvent();

    [Tooltip("When the player press backward this event is going to trigger all method")]
    /// <summary>
    /// When the player press backward this event is going to trigger all method
    /// </summary>
    public UnityEvent OnBackward = new UnityEvent();

    [Tooltip("When the player press right this event is going to trigger all method")]
    /// <summary>
    /// When the player press right this event is going to trigger all method
    /// </summary>
    public UnityEvent OnRight = new UnityEvent();

    [Tooltip("When the player press left this event is going to trigger all method")]
    /// <summary>
    /// When the player press left this event is going to trigger all method
    /// </summary>
    public UnityEvent OnLeft = new UnityEvent();

    Vector3 direction;

    string dirName = string.Empty;

    

    public void OnTriggerStay(Collider other)
    {
        GetRigidbody(other);
    }

    public void GetRigidbody(Collider col)
    {
        PlayerContoller player = col.GetComponent<PlayerContoller>();
        if(player)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            CheckDirection(rb);
        }
    }

    void CheckDirection(Rigidbody rb)
    {
        
         direction = Camera.main.transform.InverseTransformDirection(rb.velocity.normalized);

       

        if(direction.z > 0.5f && !dirName.Equals("Foward"))
        {
            dirName = "Foward";
            if(OnFoward.GetPersistentEventCount() >0)
            {
                OnFoward.Invoke();
            }

        }else if (direction.z < -0.5f && !dirName.Equals("Backward"))
        {
            dirName = "Backward";
            if (OnBackward.GetPersistentEventCount() > 0)
            {
                OnBackward.Invoke();
            }
        }
        else if (direction.x > 0.5f && !dirName.Equals("Right"))
        {
            dirName = "Right";
            if (OnRight.GetPersistentEventCount() > 0)
            {
                OnRight.Invoke();
            }
        }
        else if (direction.x < -0.5f && !dirName.Equals("Left"))
        {
            dirName = "Left";
            if (OnLeft.GetPersistentEventCount() > 0)
            {
                OnLeft.Invoke();
            }
        }

    }

   
    

}
