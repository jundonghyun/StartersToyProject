using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        Ammo,
        Coin,
        Grenade,
        Heart,
        Weapon
    };

    public Type type;
    public int value;

    void Awake()
    {

    }

    // private void OnTriggerStay(Collider other)
    // {
    //     if (other.tag == "Player")
    //         other.GetComponent<PlayerScript>().TriggerCheck(other.gameObject);
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.tag == "Player")
    //         other.GetComponent<PlayerScript>().TriggerOff(other.gameObject);
    // }

    void Update()
    {

    }
}
