using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirCamera : MonoBehaviour
{
    void Awake()
    {

    }

    void Update()
    {
        Vector3 dir = (Camera.main.transform.position - this.transform.position);
        dir.x = 40;
        dir.z = 0;
        // Quaternion qt = Quaternion.LookRotation(dir);
        this.transform.LookAt(Camera.main.transform.position - dir);
    }
}
