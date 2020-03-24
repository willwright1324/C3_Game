using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour {
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == "Cube") {
            BossController.Instance.attacks[int.Parse(name.Substring(name.Length - 1)) - 1] = other.gameObject;
        }
    }
}
