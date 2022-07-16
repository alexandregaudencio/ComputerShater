using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    public int numSphere = 10;
    private int parar = 0;
    public int MAX = 100;
    public float r = 1;
    public float speed = 100;
    public float ma = 10;
    public float forcaGravidade = 9.8f;
    public bool GPU = true;

    private int loop = 100;
    private float totalTime = 0;
    private int contador = 0;

    
    struct Sphere
    {
        public float radius;
        public float speed;
        public float massa;
        public float forca;
        public Vector4 color;
        public Vector3 position;
        
        
    }
    struct Cube
    {
        public Vector3 position;
        public Vector3 scale;
    }


    Sphere[] sphere;
    Cube[] cube;
    Transform[] positionSphere;
    public Transform[] transformCube;

    public ComputeShader computeShader;
    ComputeBuffer ScomputeBuffer;
    ComputeBuffer CcomputeBuffer;

    void Start()
    {

        sphere = new Sphere[numSphere];
        cube = new Cube[transformCube.Length];
        positionSphere = new Transform[numSphere];
        

        Spawn();

        for(int i = 0; i< numSphere; i++)
        {
            sphere[i] = new Sphere();
            sphere[i].radius = r;
            sphere[i].massa = Random.Range(0, ma);
            sphere[i].forca = forcaGravidade;
            sphere[i].position = new Vector3(Random.Range(-MAX, MAX), Random.Range(0, 60), Random.Range(-MAX, MAX));
            sphere[i].speed = Random.Range(speed/2, speed);
            sphere[i].color = new Vector4(0, 0, 1, 1);


            float diametro = sphere[i].radius * 2;
            positionSphere[i].localScale = new Vector3(diametro, diametro, diametro);
            positionSphere[i].GetComponent<Renderer>().material.color = sphere[i].color;

        }
        for (int i = 0; i < transformCube.Length; i++)
        {
            cube[i] = new Cube();
            cube[i].scale = transformCube[i].localScale;
            cube[i].position = transformCube[i].position;

        }



        int totalsize = sizeof(float) * 4 + sizeof(float) * 4 + sizeof(float) * 3 ;
        int totalsize2 = sizeof(float) * 6;

        ScomputeBuffer = new ComputeBuffer(sphere.Length,totalsize);
        ScomputeBuffer.SetData(sphere);

        computeShader.SetBuffer(0,"sphere",ScomputeBuffer);
        //------------------------------------------------------------
        CcomputeBuffer = new ComputeBuffer(cube.Length, totalsize2);
        CcomputeBuffer.SetData(cube);

        computeShader.SetBuffer(0, "cube", CcomputeBuffer);





        computeShader.SetInt("numSphere", numSphere);
        computeShader.SetInt("numCube", transformCube.Length);
        computeShader.SetInt("parar", parar);
        

    }

    
    void Update()
    {

        if(GPU == true)
        {

            UseGPU();
        }
        else
        {
            UseCPU();
        }
        

    }

    void Spawn()
    {
        for (int i = 0; i < numSphere; i++)
        {
            GameObject gb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            positionSphere[i] = gb.transform;

        }
        
    }

    private void OnDestroy()
    {
        ScomputeBuffer.Dispose();
        CcomputeBuffer.Dispose();
    }


    private void UseGPU()
    {
        float inicio = Time.realtimeSinceStartup;

        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(0, Mathf.CeilToInt(numSphere / 10f), 1, 1);
        ScomputeBuffer.GetData(sphere);
        CcomputeBuffer.GetData(cube);


        for (int i = 0; i < numSphere; i++)
        {
            positionSphere[i].position = sphere[i].position;
            positionSphere[i].GetComponent<Renderer>().material.color = sphere[i].color;
        }
        for (int i = 0; i < cube.Length; i++)
        {
            transformCube[i].position = cube[i].position;
        }

        totalTime += Time.realtimeSinceStartup - inicio;
        contador++;
        if (contador == loop)
        {
            float _mean = totalTime / numSphere;
            Debug.Log("GPU: " + _mean);

        }

    }
    private void UseCPU()
    {
        float inicio = Time.realtimeSinceStartup;

        for (int i = 0; i < numSphere; i++)
        {
            
            

            if ((sphere[i].position.y + sphere[i].radius >= cube[0].position.y - cube[0].scale.y / 2.0f) && (sphere[i].position.y - sphere[i].radius <= cube[0].position.y + cube[0].scale.y / 2.0f) && (sphere[i].position.x + sphere[i].radius >= cube[0].position.x - cube[0].scale.x / 2.0f) && (sphere[i].position.x - sphere[i].radius <= cube[0].position.x + cube[0].scale.x / 2.0f) && (sphere[i].position.y - sphere[i].radius <= cube[0].position.y + cube[0].scale.y / 2.0f) && (sphere[i].position.z + sphere[i].radius >= cube[0].position.z - cube[0].scale.z / 2.0f) && (sphere[i].position.z - sphere[i].radius <= cube[0].position.z + cube[0].scale.z / 2.0f) && sphere[i].forca != 0)
            {

                sphere[i].forca = 0;
                positionSphere[i].GetComponent<Renderer>().material.color = new Vector4(Random.RandomRange(0.0f,1.0f), Random.RandomRange(0.0f, 1.0f), Random.RandomRange(0.0f, 1.0f), 1);

            }
            else
            {
                if (sphere[i].forca != 0)
                {
                    sphere[i].speed += (sphere[i].forca / sphere[i].massa) * Time.deltaTime;
                    sphere[i].position.y -= sphere[i].speed * Time.deltaTime;
                    positionSphere[i].position = sphere[i].position;
                }
                else
                {
                    positionSphere[i].GetComponent<Renderer>().material.color = new Vector4(Random.RandomRange(0.0f, 1.0f), Random.RandomRange(0.0f, 1.0f), Random.RandomRange(0.0f, 1.0f), 1);

                }
            }

        }
       



        totalTime += Time.realtimeSinceStartup - inicio;
        contador++;
        if (contador == loop)
        {
            float _mean = totalTime / numSphere;
            Debug.Log("CPU: " + _mean);

        }

    }

   
}
