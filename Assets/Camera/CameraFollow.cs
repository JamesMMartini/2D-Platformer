using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float cameraSpeed, switchDirectionThreshold, forwardLead;

    Vector3 targetPos;
    bool goingRight = true;
    float farthestReach;

    private void Start()
    {
        targetPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (goingRight)
        {
            if (player.position.x > farthestReach)
            {
                farthestReach = player.position.x;
                targetPos.x = player.position.x + forwardLead;
            }

            if (player.position.x < farthestReach - switchDirectionThreshold)
            {
                goingRight = false;
            }
        }
        else
        {
            if (player.position.x < farthestReach)
            {
                farthestReach = player.position.x;
                targetPos.x = player.position.x - forwardLead;
            }

            if (player.position.x > farthestReach + switchDirectionThreshold)
            {
                goingRight = true;
            }
        }

        //targetPos.x = player.position.x;
        targetPos.y = player.position.y;

        float percToMove = cameraSpeed * Time.fixedDeltaTime;
        transform.position = targetPos * percToMove + transform.position * (1 - percToMove);
    }
}
