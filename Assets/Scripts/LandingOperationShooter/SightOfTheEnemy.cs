using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightOfTheEnemy : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    [Header("Он тебя видит")]
    public bool isVisible;

    public float height;

    public Transform target;

    Transform diraction;

    private void Start()
    {
        diraction = new GameObject().transform;
    }

    public void CheckSight()
    {
        diraction.position = transform.position;
        diraction.rotation = transform.rotation;
        diraction.LookAt(target.position);


        ray = new Ray(transform.position + new Vector3(0, height, 0), diraction.forward);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Allies"))
            {
                isVisible = true;
            }
            else
            {
                isVisible = false;
            }

            Debug.Log(hit.collider.gameObject.name);
            
        }

        Debug.DrawRay(ray.origin, ray.direction);
    }

    private void Update()
    {
        if (target != null)
        {
            CheckSight();
        }
    }
}