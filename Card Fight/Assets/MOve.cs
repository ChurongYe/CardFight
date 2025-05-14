using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOve : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      
            GetComponent<NavMeshAgent2D>().destination = w;
        }
    }
}
