using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodVFXRenderer : MonoBehaviour
{
    public int instanceCount = 0;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    public List<MeshProperties> meshData = new List<MeshProperties>();

    public static BloodVFXRenderer Instance { get; private set; }

    public struct MeshProperties
    {
        public Matrix4x4 mat;
        public Vector4 color;

        public MeshProperties(Matrix4x4 mat, Vector4 color)
        {
            this.mat = mat;
            this.color = color;
        }

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 4;      // color;
        }
    }

    void Start()
    {
        Instance = this;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    public void Add(Vector3 position, Quaternion rotation, Vector3 size)
    {
        Vector4 color = Vector4.zero;
        do
        {
            color = new Vector4(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        } while (color == Vector4.one || color == Vector4.zero);
        meshData.Add(new MeshProperties(Matrix4x4.TRS(position, rotation, size), color));
        instanceCount++;
    }

    void Update()
    {
        if (instanceCount == 0) return;
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)), argsBuffer);
    }

    void UpdateBuffers()
    {
        // Positions
        if (positionBuffer != null)
            positionBuffer.Release();

        positionBuffer = new ComputeBuffer(instanceCount, 16 * sizeof(float) + 4 * sizeof(float));
        
        positionBuffer.SetData(meshData.ToArray());
        instanceMaterial.SetBuffer("_Properties", positionBuffer);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void OnDisable()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}
