using UnityEngine;
using System.Collections.Generic;

public class WaypointCarController : MonoBehaviour
{
    public List<Transform> waypoints;
    public float moveSpeed = 5f;
    public float turnSpeed = 2f;
    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Count == 0) return; // Exit if no waypoints

        // Get the position of the current target waypoint
        Vector3 targetWaypointPosition = waypoints[currentWaypointIndex].position;

        // Move the car towards the target waypoint
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Rotate the car to face the target waypoint
        Vector3 directionToTarget = (targetWaypointPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Check if the car has reached the waypoint
        // You can use Vector3.Distance or a smaller trigger collider on the car/waypoint
        if (Vector3.Distance(transform.position, targetWaypointPosition) < 1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0; // Loop back to the first waypoint
            }
        }
    }
}
