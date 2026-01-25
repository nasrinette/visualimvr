using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterRoom : MonoBehaviour
{

    public GameObject supermarket;
    public GameObject street;
    public GameObject classroom;

    public GameObject supermarketFrame;
    public GameObject streetFrame;
    public GameObject classroomFrame;



    public GameObject baseRoom;

    public Collider supermarketCollider; 
    public Collider streetCollider; 
    public Collider classroomCollider;
    // Start is called before the first frame update
    void Start()
    {
        supermarket.SetActive(false);
        street.SetActive(false);
        classroom.SetActive(false);
        baseRoom.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (gameObject == supermarketCollider.gameObject)
            {
                supermarket.SetActive(true);
                street.SetActive(false);
                classroom.SetActive(false);

                baseRoom.SetActive(false);

                streetFrame.SetActive(false);
                classroomFrame.SetActive(false);
            }
            if (gameObject == streetCollider.gameObject)
            {
                Debug.Log("entered street");
                supermarket.SetActive(false);
                street.SetActive(true);
                classroom.SetActive(false);
                baseRoom.SetActive(false);

                supermarketFrame.SetActive(false);
                classroomFrame.SetActive(false);

            }
            if (gameObject == classroomCollider.gameObject)
            {
                Debug.Log("entered classroom");
                supermarket.SetActive(false);
                street.SetActive(false);
                classroom.SetActive(true);
                
                baseRoom.SetActive(false);

                supermarketFrame.SetActive(false);
                streetFrame.SetActive(false);
            
            }
        }
    }
   
}
