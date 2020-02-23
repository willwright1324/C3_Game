using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCube : MonoBehaviour {
    // Update is called once per frame
    void Update() {
        transform.Rotate(Time.deltaTime * 45, Time.deltaTime * -40, Time.deltaTime * 50);
    }
}
