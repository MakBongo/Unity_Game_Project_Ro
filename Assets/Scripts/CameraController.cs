using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*
    public float followAhead = 5f;
    private GameObject player;
    private Vector3 targetPosition;
    public float smoothing = 5f;
    private void Start()
    {
        player = GameObject.Find("Player");
    }


    void Update()
    {
        if (player.transform.localScale.x > 0f)
        {
            targetPosition = new Vector3(player.transform.position.x + followAhead, transform.position.y, transform.position.z);
        }
        else
        {
            targetPosition = new Vector3(player.transform.position.x - followAhead, transform.position.y, transform.position.z);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);

    }
    */

    public Transform target;
    public float smoothing;


    void LateUpdate()
    {
        if (target != null)
        {
            if (transform.position != target.position)
            {
                Vector3 targetPos = target.position;
                transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
            }
        }
    }
}

