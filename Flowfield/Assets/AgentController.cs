using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    Vector3 heading;
    Vector3 nextHeading;
    [SerializeField] float speed = 5.0f;
    [SerializeField] float headingLerpAmount = .1f;

    // Update is called once per frame
    void Update()
    {

        nextHeading = FlowfieldController.instance.GetDirection(FlowfieldController.instance.WorldToGrid(transform.position));
        heading = Vector3.Lerp(heading, nextHeading, headingLerpAmount);
        transform.Translate(heading.normalized * Time.deltaTime * speed);
    }



    public void SetHeading(Vector3 nextHeading)
    {
        this.heading = nextHeading;
    }
}
