using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] GameObject Halo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "HaloTag"){
            Halo.SetActive(false);

        }
       
    }
      void OnTriggerExit(Collider other)
    {
        if(other.tag == "HaloTag"){
            Halo.SetActive(true);

        }
    }
}
