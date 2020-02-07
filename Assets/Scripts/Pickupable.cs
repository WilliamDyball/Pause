using UnityEngine;
using System.Collections;

public class Pickupable : MonoBehaviour {

    public Vector3 storedForce;
    public bool bForceStored;

    GameObject player1;
    // Use this for initialization
    void Start() {
        player1 = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update() {
        if (player1.GetComponent<FirstPersonController>().GetTimeStopped() && gameObject == player1.GetComponent<FirstPersonController>().carriedObj) {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        } else if (player1.GetComponent<FirstPersonController>().GetTimeStopped()) {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        } else {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            if (bForceStored) {
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                gameObject.GetComponent<Rigidbody>().AddForce(storedForce);
                bForceStored = false;
            }
        }
    }

    public void StoreForce() {
        Debug.Log("StoreForce");
        bForceStored = true;
        storedForce = gameObject.GetComponent<Rigidbody>().velocity;
    }
}
