using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Zinnia.Cast;
using UnityEngine.UI;
using TMPro;

public class Evaluation : MonoBehaviour
{
    public Transform rightControllerAlias;
    public Transform headsetAlias;
    public Transform playArea;
    public Transform canvas;
    public TextMesh textController;
    public TextMesh textNetwork;
    public TextMeshProUGUI textFPS;
    public TextMeshProUGUI textNumParticles;
    public TextMeshProUGUI textNumLines;
    public GameObject line;

    private ParticleSystem ps;
    private Dictionary<string, ParticleSystem.Particle> particles;
    private Dictionary<string, Color32> particleColor;
    private Dictionary<string, Color32> cat_color;
    private List<GameObject> lines;
    private string currentNode;
    private int numColors = 5;
    private int numLines = 30;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize Color Dictionary
        InitializeColors();

        // Get Particle System
        ps = GetComponent<ParticleSystem>();

        lines = new List<GameObject>();

        particles = new Dictionary<string, ParticleSystem.Particle>();
        particleColor = new Dictionary<string, Color32>();

        particles = InitializeNetwork(200);

        // Start network with the particles from the blood dataset
        var main = ps.main;
        main.maxParticles = particles.Values.Count();
        ps.SetParticles(particles.Values.ToArray());

        textNumParticles.text = "Particles: " + particles.Values.Count().ToString();
        textNumLines.text = "Num lines: " + numLines.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rightHeader = Vector3.Cross(playArea.up, headsetAlias.forward);
        Vector3 forwardHeader = Vector3.Cross(rightHeader, playArea.up);
        Quaternion headsetRotation = Quaternion.LookRotation(forwardHeader, playArea.up);

        textNetwork.transform.rotation = headsetRotation;

        textFPS.text = "FPS: " + (1.0f / Time.smoothDeltaTime).ToString();
    }

    private void InitializeColors()
    {
        // Cluster colours
        cat_color = new Dictionary<string, Color32>
        {
            { "blue", new Color32(0, 0, 255, 255) },
            { "white", new Color32(255, 255, 255, 255) },
            { "yellow", new Color32(255, 255, 0, 255) },
            { "cyan", new Color32(0, 255, 255, 255) },
            { "lightcyan", new Color32(244, 255, 255, 255) },
            { "darkgreen", new Color32(0, 128, 0, 255) },
            { "darkgrey", new Color32(50, 50, 50, 255) },
            { "darkred", new Color32(128, 0, 0, 255) },
            { "darkturquoise", new Color32(0, 128, 128, 255) },
            { "green", new Color32(0, 255, 0, 255) },
            { "grey60", new Color32(128, 128, 128, 255) },
            { "magenta", new Color32(255, 0, 255, 255) },
            { "midnightblue", new Color32(0, 0, 128, 255) },
            { "lightyellow", new Color32(255, 255, 128, 255) },
            { "pink", new Color32(255, 128, 255, 255) }
        };
    }

    private IEnumerator Haptics(float frequency, float amplitude, float duration, bool rightHand, bool leftHand)
    {
        if (rightHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(duration);

        if (rightHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    //Similar to HandleData from Zinnia ObjectPointer
    public void SelectNode(PointsCast.EventData data)
    {
        StartCoroutine(WaitCoroutine());

        Dictionary<string, List<string>> particle_relations;
        int max_particles = particles.Values.Count();

        string gene_string = "";
        Vector3 gene_pos = new Vector3();
        float min_distance = 8.0f;

        Vector3 rayVector = data.Points[1] - data.Points[0];
        Vector3 forwardRay = rayVector;

        Ray controllerRay = new Ray(data.Points[0], forwardRay);


        foreach (KeyValuePair<string, ParticleSystem.Particle> item in particles)
        {
            Vector3 pos = transform.TransformPoint(item.Value.position);
            float size = item.Value.startSize * 50;

            float distance = Vector3.Cross(controllerRay.direction, pos - controllerRay.origin).magnitude;

            if (distance < size)
            {
                float current_distance = (pos - controllerRay.origin).magnitude;
                if (current_distance < min_distance)
                {
                    min_distance = current_distance;
                    gene_pos = pos;
                    gene_string = item.Key;
                }
            }
        }

        if (min_distance < 50.0f && gene_string != "" && currentNode != gene_string)
        {
            // Controller gene text
            textController.text = gene_string;

            textNetwork.text = gene_string;
            textNetwork.transform.position = gene_pos;

            currentNode = gene_string;

            // Haptics right controller vibration
            StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

            foreach (GameObject line in lines)
            {
                Destroy(line);
            }
            lines.Clear();
;
            for (int i = 0; i < numLines; i++)
            {
                Vector3[] vs = new Vector3[2];
                GameObject clone;
                LineRenderer clone_line;
                int randomNode = UnityEngine.Random.Range(1, max_particles);
                string remote_gene = particles.ElementAt(randomNode).Key;


                vs[0] = transform.TransformPoint(particles[remote_gene].position);
                vs[1] = transform.TransformPoint(particles[gene_string].position);

                clone = Instantiate(line);
                clone.transform.parent = this.transform;
                clone_line = clone.GetComponent<LineRenderer>();

                clone_line.SetPositions(vs);

                lines.Add(clone);
            }
        }
    }

    IEnumerator WaitCoroutine()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.5f);
    }

    private Dictionary<string, ParticleSystem.Particle> InitializeNetwork(int numParticles)
    {
        // We load the network and categories files and save the information in arrays
        Dictionary<string, ParticleSystem.Particle> particleDict = new Dictionary<string, ParticleSystem.Particle>();

        for (int cat = 0; cat < numColors; cat++)
        {
            Color32 color = cat_color.ElementAt(cat).Value;
            string color_name = cat_color.ElementAt(cat).Key;

            for (int particle = 0; particle < numParticles; particle++)
            {
                ParticleSystem.Particle new_particle = new ParticleSystem.Particle
                {
                    remainingLifetime = 100000.0f,
                    startLifetime = 100000.0f,
                    startSize = 0.1f,
                    startColor = color,
                    position = new Vector3((UnityEngine.Random.value * 50) - 25, (UnityEngine.Random.value * 10) - 5, (UnityEngine.Random.value * 50) - 25)
                };
                particleDict[particle.ToString() + '-' +  color_name] = new_particle;
                particleColor[particle.ToString() + '-' + color_name] = color;
            }
        }

        //List<string> keys = Enumerable.ToList(particleDict.Keys);
        //int size = keys.Count;
        //for (int it = 0; it < 50; it++)
        //{
        //    for (int conn = 1; conn < numParticles; conn++)
        //    {
        //        string gene1 = particleDict.ElementAt(UnityEngine.Random.Range(0, size)).Key;
        //        string gene2 = particleDict.ElementAt(UnityEngine.Random.Range(0, size)).Key;

        //        try
        //        {
        //            int randint = (int)(UnityEngine.Random.value * size);
        //            ParticleSystem.Particle particle = particleDict[gene1];
        //            Vector3 avoid_direction = particle.position - particleDict[keys[randint]].position;
        //            particle.position += avoid_direction.normalized / 10;
        //            Vector3 direction = particleDict[gene2].position - particle.position;
        //            particle.position += direction.normalized / 5;
        //            particleDict[gene1] = particle;
        //        }
        //        catch
        //        {
        //            continue;
        //        }
        //    }
        //}

        //return particlesReal;
        return particleDict;
    }

    public void AddParticles()
    {
        int num_particles = particles.Values.Count();

        for (int particle = num_particles - 1; particle < num_particles + 499; particle++)
        {
            int numColor = UnityEngine.Random.Range(0, 14);
            Color32 color = cat_color.ElementAt(numColor).Value;
            string colorName = cat_color.ElementAt(numColor).Key;
            ParticleSystem.Particle new_particle = new ParticleSystem.Particle
            {
                remainingLifetime = 100000.0f,
                startLifetime = 100000.0f,
                startSize = 0.1f,
                startColor = color,
                position = new Vector3((UnityEngine.Random.value * 50) - 25, (UnityEngine.Random.value * 10) - 5, (UnityEngine.Random.value * 50) - 25 )
            };
            particles[particle.ToString() + '-' + colorName] = new_particle;
            particleColor[particle.ToString() + '-' + colorName] = color;
        }

        var main = ps.main;
        main.maxParticles = particles.Values.Count();
        ps.SetParticles(particles.Values.ToArray());

        textNumParticles.text = "Particles: " + particles.Values.Count().ToString();
    }

    public void AddLines()
    {
        numLines += 10;
        textNumLines.text = "Num lines: " + numLines.ToString();
    }

    public void RemoveLines()
    {
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
    }
}
