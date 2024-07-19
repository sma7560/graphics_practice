using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GPUGraph : MonoBehaviour
{
    const int maxRes = 1000;

    [SerializeField, Range(10, maxRes)]
    int resolution = 200;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material mat;

    [SerializeField]
    Mesh mesh;

    static readonly int 
        positionsID = Shader.PropertyToID("_Positions"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time");

    ComputeBuffer positionsBuffer;

    void UpdateFunctionOnGPU()
    {
        var kernelIndex = (int)function;
        float step = 2f / resolution;
        computeShader.SetInt(resolutionID, resolution);
        computeShader.SetFloat(stepID, step);
        computeShader.SetFloat(timeID, Time.time);
        computeShader.SetBuffer(kernelIndex, positionsID, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        mat.SetBuffer(positionsID, positionsBuffer);
        mat.SetFloat(stepID, step);

        RenderParams rp = new RenderParams(mat);
        rp.worldBounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.RenderMeshPrimitives(rp, mesh, 0, resolution * resolution);
    }

    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxRes * maxRes, 3 * 4);
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        UpdateFunctionOnGPU();
    }
}
