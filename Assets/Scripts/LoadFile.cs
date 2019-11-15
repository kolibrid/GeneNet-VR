using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadFile : MonoBehaviour
{
    public Logger log;
    public Transform body;
    public TextMesh gene_name;
    public Transform gene_position;

    private string[] words;
    private int counter;
    private TextMesh tm;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] alive_particles;
    private Dictionary<string, ParticleSystem.Particle> particles;
    private Dictionary<string, List<string>> particle_relations;
    private List<LineRenderer> lines;

    void Start()
    {
        lines = new List<LineRenderer>();

        TextAsset ta = Resources.Load<TextAsset>("blood-network");
        words = ta.text.Split('\n');

        TextAsset categories_text = Resources.Load<TextAsset>("blood-categories");
        string[] categories = categories_text.text.Split('\n');

        counter = 1;

        ps = GetComponent<ParticleSystem>();
        Color32 particle_color = new Color32(255,0,0,255);

        Dictionary<string, Color32> cat_color = new Dictionary<string, Color32>();
        cat_color.Add("black", new Color32(0,0,0,255));
        cat_color.Add("blue", new Color32(0,0,255,255));
        cat_color.Add("white", new Color32(255,255,255,255));
        cat_color.Add("yellow", new Color32(255,255,0,255));
        cat_color.Add("cyan", new Color32(0,255,255,255));
        cat_color.Add("darkgreen", new Color32(0,128,0,255));
        cat_color.Add("darkgrey", new Color32(50,50,50,255));
        cat_color.Add("darkred", new Color32(128,0,0,255));
        cat_color.Add("darkturquoise", new Color32(0,128,128,255));
        cat_color.Add("green", new Color32(0,255,0,255));
        cat_color.Add("grey60", new Color32(128,128,128,255));
        cat_color.Add("magenta", new Color32(255,0,255,255));
        cat_color.Add("midnightblue", new Color32(0,0,128,255));
        cat_color.Add("lightyellow", new Color32(255,255,128,255));
        cat_color.Add("pink", new Color32(255,128,255,255));
        cat_color.Add("purple", new Color32(128,0,128,255));
        cat_color.Add("violet", new Color32(238,130,238,255));
        cat_color.Add("saddlebrown", new Color32(139,69,19,255));
        cat_color.Add("brown", new Color32(165,42,42,255));
        cat_color.Add("tan", new Color32(210,180,140,255));
        cat_color.Add("salmon", new Color32(233,150,122,255));
        cat_color.Add("greenyellow", new Color32(173,255,47,255));
        cat_color.Add("turquoise", new Color32(64,224,208,255));

        particles = new Dictionary<string, ParticleSystem.Particle>();

        Random rnd = new Random();

        for(int cat = 0; cat < categories.Length - 1; cat++) {
            string[] content = categories[cat].Split(',');
            string[] genes = content[1].Split(' ');
            particle_color = cat_color[content[0]];
            
            for(int gene = 0; gene < genes.Length; gene++) {
                ParticleSystem.Particle new_particle = new ParticleSystem.Particle();
                new_particle.remainingLifetime = 100000.0f;
                new_particle.startLifetime = 100000.0f;
                new_particle.startSize = 0.1f;
                new_particle.startColor =  particle_color;
                new_particle.position = new Vector3(Random.value * 50, Random.value * 10, Random.value * 50);
                particles[genes[gene]] = new_particle;
            }
        }
        
        List<string> keys = Enumerable.ToList(particles.Keys);
        particle_relations = new Dictionary<string, List<string>>();
        int size = keys.Count;
        for(int it = 0; it < 10; it++) {
            for(int conn = 1; conn < words.Length - 1; conn++) {
                string[] elems = words[conn].Split(',');
                string gene1 = elems[0].Replace("\"", string.Empty);
                gene1 = gene1.Replace(",", string.Empty);

                string gene2 = elems[1].Replace("\"", string.Empty);
                gene2 = gene2.Replace(",", string.Empty);

                if(!particle_relations.ContainsKey(gene1)) {
                    particle_relations[gene1] = new List<string>();                    
                }
                particle_relations[gene1].Add(gene2);

                if(!particle_relations.ContainsKey(gene2)) {
                    particle_relations[gene2] = new List<string>();                    
                }
                particle_relations[gene2].Add(gene1);


                try {
                    int randint = (int)(Random.value * size);
                    ParticleSystem.Particle particle = particles[gene1];
                    Vector3 avoid_direction = particle.position - particles[keys[randint]].position;
                    particle.position += avoid_direction.normalized / 10;
                    Vector3 direction = particles[gene2].position - particle.position;                    
                    particle.position += direction.normalized/5;
                    particles[gene1] = particle;
                } catch {
                    continue;
                }
            }
        }

        Dictionary<string, ParticleSystem.Particle>particles_real = new Dictionary<string, ParticleSystem.Particle>();
        foreach(var p in particles.Keys) {
            if(particle_relations.ContainsKey(p)) {
                particles_real[p] = particles[p];
            }
        }

        IEnumerable<ParticleSystem.Particle> vals = particles_real.Values;
        ps.SetParticles(vals.ToArray());
        alive_particles = new ParticleSystem.Particle[ps.main.maxParticles];
        int num_particles = ps.GetParticles(alive_particles);
    }

    void Update(){
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote) + body.position;
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote) * body.rotation;
        Vector3 controllerDirection = controllerRotation * Vector3.forward;
        Ray controllerRay = new Ray(controllerPosition, controllerDirection);

        string gene_string = "";
        Vector3 gene_pos = new Vector3();
        float min_distance = 1000.0f;

        foreach(KeyValuePair<string, ParticleSystem.Particle> item in particles) {
            Vector3 pos = item.Value.position;
            float size = item.Value.startSize;

            float distance = Vector3.Cross(controllerRay.direction, pos - controllerRay.origin).magnitude;

            if(distance < size){
                float current_distance = (pos - controllerRay.origin).magnitude;
                if(current_distance < min_distance) {
                    min_distance = current_distance;
                    gene_pos = pos;
                    gene_string = item.Key;
                }
            }
        }

        if(min_distance < 10.0f) {
            gene_position.position = gene_pos;
            gene_position.LookAt(2 * gene_position.position - body.position);
            gene_name.text = gene_string;
            if(lines.Count < 10) {
                foreach(string remote_gene in particle_relations[gene_string]) {
                    LineRenderer lr = gameObject.AddComponent<LineRenderer>() as LineRenderer;
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    Vector3[] vs = new Vector3[2];
                    vs[0] = particles[remote_gene].position;
                    vs[1] = particles[gene_string].position;
                    lr.positionCount = vs.Length;
                    lr.SetPositions(vs);
                    lines.Add(lr);
                }
            }
        } else {
            foreach(LineRenderer line in lines) {
                Destroy(line);
            }
            lines.Clear();
        }
    }
}
