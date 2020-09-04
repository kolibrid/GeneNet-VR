using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment1 : MonoBehaviour
{
    public Transform network;
    public Transform rightController;
    public TextMesh textController;
    public TextMesh textNetwork;
    private string currentNode;
    public GameObject line;
    private List<GameObject> lines;

    private Dictionary<string, ParticleSystem.Particle> particles;
    private IEnumerator selectNodeCoroutine;
    string[] keys;
    private int numKeys;
    private int num;

    // Start is called before the first frame update
    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();

        particles = LoadFile.particlesBlood;
        keys = particles.Keys.ToArray();
        numKeys = keys.Length;

        num = 0;
    }

    // Update is called once per frame
    void Update()
    {
        network.transform.Translate(Vector3.back * -0.35f * Time.deltaTime);

        selectNodeCoroutine = selectNode(keys[num], 1.0f);
        StartCoroutine(selectNodeCoroutine);
    }

    private IEnumerator selectNode(string gene_string, float waitTime)
    {
        Dictionary<string, List<string>> particle_relations = LoadFile.networkBlood;

        yield return new WaitForSeconds(waitTime);

        currentNode = gene_string;

        num += 10;

        // Haptics right controller vibration
        StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        // CPU usage - Evaluation
        foreach (string remote_gene in particle_relations[gene_string])
        {
            try
            {
                Vector3[] vs = new Vector3[2];
                GameObject clone;
                LineRenderer clone_line;

                vs[0] = transform.TransformPoint(particles[remote_gene].position);
                vs[1] = transform.TransformPoint(particles[gene_string].position);

                clone = Instantiate(line);
                clone.transform.parent = this.transform;
                clone_line = clone.GetComponent<LineRenderer>();

                clone_line.SetPositions(vs);

                lines.Add(clone);
            }
            catch (InvalidCastException e)
            {
                Debug.Log($"There was an error adding a line: {e}");
            }

        }
    }

    private IEnumerator Haptics(float frequency, float amplitude, float duration, bool rightHand, bool leftHand)
    {
        if (rightHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(duration);

        if (rightHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }
}
