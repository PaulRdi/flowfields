using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class FlowfieldController : MonoBehaviour
{
    public static FlowfieldController instance;

    Vector3[,] flowfield;

    [SerializeField]
    Transform bottomLeft, topRight;

    [SerializeField]
    int resolution = 3;

    Vector3Int size;

    [SerializeField] float perlinScale = 1.0f;

    [SerializeField] GameObject fieldPrefab;

    private void Awake()
    {
        instance = this;
        Vector3 inWorldSize = topRight.position - bottomLeft.position;
        size = new Vector3Int(
            Mathf.RoundToInt(Math.Abs(inWorldSize.x)) * resolution,
            Mathf.RoundToInt(Math.Abs(inWorldSize.y)) * resolution,
            Mathf.RoundToInt(Math.Abs(inWorldSize.z)) * resolution);


        flowfield = new Vector3[size.x, size.z];

        for (int z = 0; z < size.z; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                float theta = Mathf.PerlinNoise(x * perlinScale, z * perlinScale);
                Vector3 dir = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
                flowfield[x, z] = dir;

                GameObject go = Instantiate(fieldPrefab, GridToWorld(new Vector3Int(x, 0, z)), Quaternion.identity);
                go.transform.localScale = Vector3.one / (float)resolution;
                Transform child = go.transform.GetChild(0);
                child.LookAt(child.position + dir);


            }
        }
    }

    public Vector3 GetDirection(Vector3Int gridPos)
    {
        if (gridPos.x < 0 ||
            gridPos.x >= size.x ||
            gridPos.z < 0 ||
            gridPos.z >= size.z)
            return Vector3.zero;
        return flowfield[gridPos.x, gridPos.z];
    }

    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        Vector3 output = new Vector3(
            (float)gridPos.x / (float)resolution,
            (float)gridPos.y / (float)resolution,
            (float)gridPos.z / (float)resolution);

        return bottomLeft.position + output;
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        worldPos -= bottomLeft.position;

        return new Vector3Int(
            Mathf.RoundToInt(Math.Abs(worldPos.x)) * resolution,
            Mathf.RoundToInt(Math.Abs(worldPos.y)) * resolution,
            Mathf.RoundToInt(Math.Abs(worldPos.z)) * resolution);
    }
}
