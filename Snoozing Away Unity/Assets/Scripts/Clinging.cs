using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clinging : MonoBehaviour
{
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {  
    }

    // Update is called once per frame
    void Update()
    {
        var direction =  target.transform.position - transform.position;
        Debug.DrawRay(transform.position,direction,Color.yellow);
        transform.rotation = Quaternion.LookRotation( direction );
    }
}
