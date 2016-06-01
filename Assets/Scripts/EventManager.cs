using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

    public delegate void StartGeneration(Dictionary<string, object> parameters);
    public static event StartGeneration OnStartGeneration;



	void Awake () {
	
	}



    public static void InitParameters(Dictionary<string, object> parameters)
    {
        OnStartGeneration(parameters);
    }


}
