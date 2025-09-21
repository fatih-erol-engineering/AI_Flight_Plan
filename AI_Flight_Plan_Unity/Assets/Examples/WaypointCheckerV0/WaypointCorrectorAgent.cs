using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class WaypointCorrectorAgent : Agent
{
    public BSplineDrawer bSplineDrawer;

    [Tooltip("Bölüm süresi (simülasyon saniyesi)")]
    public float episodeDuration = 1.5f;

    private float episodeStartTime;


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveScale = 20f;
        float moveX = actions.ContinuousActions[0] * moveScale;
        float moveZ = actions.ContinuousActions[1] * moveScale;
        Vector3 pos = bSplineDrawer.p1.localPosition + new Vector3(moveX, 0f, moveZ) * Time.deltaTime;
        float posX = Mathf.Clamp(pos.x, -10, 10);
        float posZ = Mathf.Clamp(pos.z, -5, 5);
        bSplineDrawer.p1.localPosition = new Vector3(posX, 0f, posZ);

        moveX = actions.ContinuousActions[2] * moveScale;
        moveZ = actions.ContinuousActions[3] * moveScale;
        pos = bSplineDrawer.p2.localPosition + new Vector3(moveX, 0f, moveZ) * Time.deltaTime;
        posX = Mathf.Clamp(pos.x, -10, 10);
        posZ = Mathf.Clamp(pos.z, -5, 5);
        bSplineDrawer.p2.localPosition = new Vector3(posX, 0f, posZ);

        if (Time.time - episodeStartTime >= episodeDuration)
        {
            EndEpisode();
        }

        //float moveScale = 20f;
        ////float moveX = actions.ContinuousActions[0] * moveScale;
        //float moveZ = actions.ContinuousActions[0] * moveScale;
        ////Vector3 pos = bSplineDrawer.p1.localPosition + new Vector3(0f, 0f, moveZ) * Time.deltaTime;
        //Vector3 pos = bSplineDrawer.p1.localPosition + new Vector3(0f, 0f, moveZ);
        ////float posX = Mathf.Clamp(pos.x, -10, 10);
        //float posZ = Mathf.Clamp(pos.z, -5, 5);
        //bSplineDrawer.p1.localPosition = new Vector3(0f, 0f, posZ);

        ////moveX = actions.ContinuousActions[2] * moveScale;
        //moveZ = actions.ContinuousActions[1] * moveScale;
        ////pos = bSplineDrawer.p2.localPosition + new Vector3(0f, 0f, moveZ) * Time.deltaTime;
        //pos = bSplineDrawer.p2.localPosition + new Vector3(0f, 0f, moveZ) ;
        ////posX = Mathf.Clamp(pos.x, -10, 10);
        //posZ = Mathf.Clamp(pos.z, -5, 5);
        //bSplineDrawer.p2.localPosition = new Vector3(0f, 0f, posZ);

        //if (Time.time - episodeStartTime >= episodeDuration)
        //{
        //    EndEpisode();
        //}
    }
    public override void OnEpisodeBegin() 
    { 
        bSplineDrawer.p0.localPosition = new Vector3(-13f,0f,0f);
        bSplineDrawer.p3.localPosition = new Vector3(+13f,0f,0f);
        float randX = Random.Range(-6f, +6f);
        float randZ = Random.Range(-3f, +3f);
        bSplineDrawer.restrictedAreas[0].localPosition = new Vector3(randX, 0f, randZ);

        randX = Random.Range(-10f,0f);
        randZ = Random.Range(-10f, +10f);
        bSplineDrawer.p1.localPosition = new Vector3(randX, 0f, randZ);

        randX = Random.Range(0f, +10f);
        randZ = Random.Range(-10f, +10f);
        bSplineDrawer.p2.localPosition = new Vector3(randX, 0f, randZ);

        episodeStartTime = Time.time; // simülasyon zamaný (timeScale'dan etkilenir)
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(new Vector2(bSplineDrawer.p1.localPosition.x, bSplineDrawer.p1.localPosition.z) / 10f);
        sensor.AddObservation(new Vector2(bSplineDrawer.p2.localPosition.x, bSplineDrawer.p2.localPosition.z) / 10f);
        sensor.AddObservation(new Vector2(bSplineDrawer.restrictedAreas[0].localPosition.x, bSplineDrawer.restrictedAreas[0].localPosition.z)/10f);

        if (bSplineDrawer.isCollide)
        {
            AddReward(-0.02f);
        }
        else
        {
            AddReward(+0.1f);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contAction = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                contAction[0] = +1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                contAction[0] = -1;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                contAction[1] = +1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                contAction[1] = -1;
            }
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                contAction[2] = +1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                contAction[2] = -1;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                contAction[3] = +1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                contAction[3] = -1;
            }
        }

    }

}
