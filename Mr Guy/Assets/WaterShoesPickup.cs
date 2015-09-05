using UnityEngine;
using System.Collections;

public class WaterShoesPickup : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        var waters = GameObject.FindGameObjectsWithTag("Water");
        foreach (GameObject w in waters)
        {
            w.GetComponent<Collider2D>().isTrigger = false;
            w.tag = "Ground";
        }
        Destroy(this.gameObject);
    }

}
