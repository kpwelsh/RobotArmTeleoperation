using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TestTracingScore : MonoBehaviour
{
    private class ParallelSummer {
        int threads_per_group = 512;
        ComputeShader Shader;
        int kernel_index;
        public ParallelSummer(ComputeShader shader) {
            Shader = shader;
            kernel_index = Shader.FindKernel("Sum");
        }

        public float Sum(float[] data) {
            ComputeBuffer InputBuf = new ComputeBuffer(data.Length, sizeof(float));
            InputBuf.SetData(data);

            float result = Sum(InputBuf);

            InputBuf.Release();
            return result;
        }


        public float Sum(ComputeBuffer InputBuf) {
            int length = InputBuf.count;
            int nGroups = Mathf.CeilToInt(length / (float)threads_per_group);

            ComputeBuffer OutputBuf = new ComputeBuffer(length, sizeof(float));
            (InputBuf, OutputBuf) = (OutputBuf, InputBuf);
            while (length > 1) {
                (InputBuf, OutputBuf) = (OutputBuf, InputBuf);
                Shader.SetInt("length", length);
                Shader.SetBuffer(kernel_index, "InputBuf", InputBuf);
                Shader.SetBuffer(kernel_index, "OutputBuf", OutputBuf);

                Shader.Dispatch(kernel_index, nGroups, 1, 1);

                length = nGroups;
                nGroups = Mathf.CeilToInt(nGroups / (float)threads_per_group);
            }
            
            float[] result = new float[1];
            OutputBuf.GetData(result);
            OutputBuf.Release();
            return result[0];
        }
    }

    private class ParallelScorer {
        int threads_per_group = 512;
        ComputeShader Shader;
        int kernel_index;

        public ParallelScorer(ComputeShader shader) {
            Shader = shader;
            kernel_index = Shader.FindKernel("Score");
        }

        public ComputeBuffer Score(Texture2D marking, Texture2D scoreMap) {
            int length = marking.width * marking.height;
            int nGroups = Mathf.CeilToInt(length / (float)threads_per_group);
            ComputeBuffer Result = new ComputeBuffer(length, sizeof(float));

            Shader.SetInt("res_x", marking.width);
            Shader.SetInt("res_y", marking.height);
            Shader.SetTexture(kernel_index, "Marking", marking);
            Shader.SetTexture(kernel_index, "ScoreMap", scoreMap);
            Shader.SetBuffer(kernel_index, "Result", Result);

            Shader.Dispatch(kernel_index, nGroups, 1, 1);

            return Result;
        }
    }

    public ComputeShader shader;
    public Texture2D StencilSDF;
    public DrawableMesh Mesh;
    ParallelSummer summer;
    ParallelScorer scorer;

    void Start()
    {
        StartCoroutine(Init());
    }

    void Update() {
        Debug.Log(Score());
    }

    IEnumerator Init() {
        while (Mesh.MarkerTex == null) {
            yield return new WaitForEndOfFrame();
        }
        if (Mesh.MarkerTex.width != StencilSDF.width
            || Mesh.MarkerTex.height != StencilSDF.height) {
            
            throw new System.Exception(
                String.Format(
                    "The dimensions of the marker texture must be the same as those for the SDF stencil to score. ({0},{1}) != ({2}, {3})",
                    Mesh.MarkerTex.width, Mesh.MarkerTex.height,
                    StencilSDF.width, StencilSDF.height
                )
            );
        }

        summer = new ParallelSummer(shader);
        scorer = new ParallelScorer(shader);
    }


    float? Score() {
        if (scorer == null || summer == null) return null;
        var scores = scorer.Score(Mesh.MarkerTex, StencilSDF);
        float result = summer.Sum(scores);
        scores.Release();
        return result;
    }
}