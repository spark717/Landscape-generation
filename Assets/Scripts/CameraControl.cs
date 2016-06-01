using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour {

    private float center = 0;
    public float speed = 50.0f;


    void Awake () {
        EventManager.OnStartGeneration += ResetCamera;
    }

    void OnApplicationQuit()
    {
        EventManager.OnStartGeneration -= ResetCamera;
    }


    void Update () {
        transform.LookAt(new Vector3(center, 0, center));
        transform.RotateAround(new Vector3(center, 0, center), Vector3.up, speed * Time.deltaTime);
    }




    void ResetCamera(Dictionary<string, object> parameters)
    {
        center = (int)parameters["chunkSize"]/2;
        transform.position = new Vector3(center*2, center*2, center*2);
    }


}
