using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (plane.Raycast(cameraRay, out rayDistance))
        {
            Vector3 lookPoint = cameraRay.GetPoint(rayDistance);

            //transform.LookAt(new Vector3(lookPoint.x, transform.position.y, lookPoint.z));

            transform.position = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        }
    }
}
