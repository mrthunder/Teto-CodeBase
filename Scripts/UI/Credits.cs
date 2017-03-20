using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour {

    public GameObject CreditsText;
    public int speed = 1;
    public GameObject creditsCanvas;


    // Update is called once per frame
    void Update()
    {
        CreditsText.transform.Translate(Vector3.up * Time.deltaTime * speed);
    }

        
   

    IEnumerator waitFor()
    {
        yield return new WaitForSeconds(20);        
    }
}
