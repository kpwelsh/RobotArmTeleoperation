using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    public List<GameObject> Robots = new List<GameObject>();
    public GameObject EndEffector;
    public GameObject Box;

    void Start() {
        Vector3 offset = Vector3.zero;
        foreach (var bot in Robots) {
            GameObject box = Instantiate(Box, transform);
            box.transform.position += offset;
            offset += new Vector3(1, 0, 0);
            
            GameObject robot = Instantiate(bot, box.transform);
            Transform command = box.transform.Find("InputCommand");
            RobotController controller = robot.GetComponent<RobotController>();
            controller.SetEE(EndEffector);
            controller.TrackTransform(command);
        }
    }

}
