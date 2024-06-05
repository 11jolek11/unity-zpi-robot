using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
 
//using TensorFlow;

public class RobotAgent : Agent
{
    public Transform wifiSignal; // Obiekt WiFi
    public float speed = 0.5f;
    public float rotationSpeed = 0.5f;
    public LayerMask obstacleLayer;

    private Rigidbody robotRigidbody;

    private float time = 0.0f;
    public float interpolationPeriod = 0.1f;

    //public TextAsset graphModel; // Tensorflow GRAPH

    void Start()
    {
        Application.targetFrameRate = -1;
        robotRigidbody = GetComponent<Rigidbody>();

        //var graph = new TFGra
    }

    public override void OnEpisodeBegin()
    {
        // Reset robota
        transform.localPosition = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Reset sygna�u Wi-Fi
        if (wifiSignal != null)
        {
            wifiSignal.localPosition = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
        }
        else
        {
            Debug.LogError("WiFi Signal is not assigned.");
        }

        robotRigidbody.velocity = Vector3.zero;
        robotRigidbody.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Odleg�o�� i kierunek do sygna�u Wi-Fi
        
        if (wifiSignal != null)
        {
            Vector3 directionToWifi = wifiSignal.localPosition - transform.localPosition;
            sensor.AddObservation(directionToWifi.normalized);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, wifiSignal.localPosition));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // Przeszkody
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, obstacleLayer))
        {
            sensor.AddObservation(hit.distance);
        }
        else
        {
            sensor.AddObservation(10f); // Maksymalna odleg�o�� dla braku przeszk�d
        }

        // Obserwacje pr�dko�ci
        sensor.AddObservation(robotRigidbody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        //time += Time.deltaTime;

        //if (time >= interpolationPeriod) {
          //  time -= interpolationPeriod;

            float move = actionBuffers.ContinuousActions[0];
            float rotate = actionBuffers.ContinuousActions[1];

            Vector3 moveVector = transform.forward * move * speed * Time.deltaTime;
            robotRigidbody.MovePosition(robotRigidbody.position + moveVector);

            Quaternion rotation = Quaternion.Euler(0, rotate * rotationSpeed * Time.deltaTime, 0);
            robotRigidbody.MoveRotation(robotRigidbody.rotation * rotation);

            // Nagrody
            float distanceToWifi = wifiSignal != null ? Vector3.Distance(transform.localPosition, wifiSignal.localPosition) : 0f;


            // Nagroda za zbli�enie si� do sygna�u Wi-Fi
            AddReward(-0.01f); // Negatywna nagroda za ka�d� klatk�
            AddReward(-0.001f * distanceToWifi);

            if (distanceToWifi < 1.5f)
            {
                AddReward(1.0f);
                EndEpisode();
            }

            // Kolizje z przeszkodami
            if (Physics.Raycast(transform.position, transform.forward, 1f, obstacleLayer))
            {
                AddReward(-1.0f);
                EndEpisode();
            }
        //}   
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }
}
