using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class MoveToGoalAgent : Agent
{
    public float moveVelHeur = 1f;
    public GameObject goal;
    public GameObject floor;
    float _lastT;
    private int stepCounter;
    public override void OnEpisodeBegin()
    {
        Debug.Log(stepCounter++);
        moveVelHeur = 1f;
        float randX = Random.Range(-3f, +3f);
        float randZ = Random.Range(-3f, +3f);
        goal.transform.localPosition= new Vector3(randX, 0, randZ);
        randX = Random.Range(-3f, +3f);
        randZ = Random.Range(-3f, +3f);
        transform.localPosition = new Vector3(randX, 0, randZ);
        _lastT = Time.time;
        stepCounter = 0;

    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float dirX = 0f;
        float dirZ = 0f;
        float normDirX = 0f;
        float normDirZ = 0f;
        float moveVel = 0f;
        dirX = actions.ContinuousActions[0];
        dirZ = actions.ContinuousActions[1];        

        float dirLen = Mathf.Sqrt(dirX * dirX + dirZ * dirZ);

        if (dirLen == 0)
        {
            normDirX = 1;
            normDirZ = 0;
            moveVel = 0f;
        }
        else
        {
            normDirX = dirX / dirLen;
            normDirZ = dirZ / dirLen;
            moveVel = actions.ContinuousActions[2] * 10;
        }

        Vector3 moveVec = new Vector3(normDirX, 0f, normDirZ) * moveVel; 
        transform.localPosition += (moveVec)*Time.deltaTime;

        //float dt = Time.time - _lastT;
        //AddReward(-0.001f * dt);
        //_lastT = Time.time;
        stepCounter++;
        
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 deltaPosition = (transform.localPosition - goal.transform.localPosition);
        sensor.AddObservation(deltaPosition.x);
        sensor.AddObservation(deltaPosition.z);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[2] = moveVelHeur;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActions[0] = -1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            continuousActions[0] = +1f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            continuousActions[1] = +1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            continuousActions[1] = -1f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Finish")
        {
            AddReward(+1);
            EndEpisode();
            floor.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
        }
        if (other.tag == "Ground")
        {
            AddReward(-1);
            EndEpisode();
            floor.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
        }
    }

}
