using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class FlowfieldController : MonoBehaviour
{
    public static FlowfieldController instance;

    Vector3[,] flowfield;
    Dictionary<Vector3Int, GameObject> cellToObject;
    public byte[][] costField;
    int[][] integrationField;
    [SerializeField, Range(10.0f, 50.0f)]
    float stuff;

    [SerializeField]
    Transform bottomLeft, topRight;

    [SerializeField]
    int resolution = 3;

    Vector3Int size;

    [SerializeField] float perlinScale = 1.0f;

    [SerializeField] GameObject fieldPrefab;

    [SerializeField] Vector2Int targetTestPos;

    [SerializeField] Transform trg;

    FlowfieldObstacle[] obstacles;

    private void Awake()
    {
        cellToObject = new Dictionary<Vector3Int, GameObject>();
        instance = this;
        Vector3 inWorldSize = topRight.position - bottomLeft.position;
        obstacles = FindObjectsOfType<FlowfieldObstacle>();
        size = new Vector3Int(
            Mathf.RoundToInt(Math.Abs(inWorldSize.x)) * resolution,
            Mathf.RoundToInt(Math.Abs(inWorldSize.y)) * resolution,
            Mathf.RoundToInt(Math.Abs(inWorldSize.z)) * resolution);


        flowfield = new Vector3[size.x, size.z];
        //TestPopulateFlowfield();
        Setup(ref costField);
        //GenerateFlowfieldWithObstacles();
        SpawnPrefabs();
    }
    private void Update()
    {
        Vector3Int targetPos = WorldToGrid(trg.position);

        IntegrateForTarget(targetPos.x, targetPos.z);
        UpdatePrefabs();
    }

    private void UpdatePrefabs()
    {
        for (int z = 0; z < size.z; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                GameObject go = cellToObject[pos];
                Vector3 dir = flowfield[x, z];
                Transform child = go.transform.GetChild(0);
                child.LookAt(child.position + dir);
            }
        }
    }

    private void SpawnPrefabs()
    {
        for (int z = 0; z < size.z; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                InstantiateField(x, z, flowfield[x, z]);
            }
        }
    }

    /// <summary>
    /// Generate a flowfield for a target position saved in  targetTestPos
    /// </summary>
    private void GenerateFlowfieldWithObstacles()
    {
        IntegrateForTarget(targetTestPos.x, targetTestPos.y);
    }

    private void IntegrateForTarget(int targetX, int targetZ)
    {

        int integratedCost = 0;
        //reset all integration cost fields to max value
        List<Vector3Int> open = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                integrationField[x][z] = int.MaxValue;
            }
        }
        //set target field to 0
        integrationField[targetX][targetZ] = 0;
        open.Add(new Vector3Int(targetX, 0, targetZ));
        Vector3 destinationWorldPos = GridToWorld(new Vector3Int(targetX, 0, targetZ));

        //iterative solve dijkstra from target pos to get integration field
        while (open.Count > 0)
        {
            Vector3Int currField = open[0];
            open.RemoveAt(0);
            /**
             * oxo
             * xix
             * oxo
             * */
            for (int i = 0; i < 4; i++)
            {
                //expand up, down, left, right
                int xSign = Math.Sign(i % 2 * (i - 2));
                int zSign = Math.Sign((i + 1) % 2 * (i - 1));
                Vector3Int neighbour = currField + new Vector3Int(xSign, 0, zSign);

                if (!IsInBounds(neighbour))
                    continue; //terminate on grid edges
                if (costField[neighbour.x][neighbour.z] >= 255)
                    continue; //ignore neighbour if is wall.

                int newNeighbourCost =
                    integrationField[currField.x][currField.z] +
                    costField[neighbour.x][neighbour.z];

                if (newNeighbourCost < integrationField[neighbour.x][neighbour.z])
                {
                    integrationField[neighbour.x][neighbour.z] = newNeighbourCost;
                    open.Add(neighbour);
                }
            }
        }
        Debug.Log(integrationField.Length);

        //get flowfield from integration field
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                Vector3Int currField = new Vector3Int(x, 0, z);

                Vector3 agg = Vector3.zero;
                int min = int.MaxValue;
                for (int xOffset = x - 1; xOffset <= x + 1; xOffset++)
                {
                    for (int zOffset = z - 1; zOffset <= z + 1; zOffset++)
                    {
                        if (xOffset == x && zOffset == z) continue;
                        Vector3Int neighbour = new Vector3Int(xOffset, 0, zOffset);
                        if (!IsInBounds(neighbour))
                            continue;

                        if (integrationField[neighbour.x][neighbour.z] < min)
                        {
                            min = integrationField[neighbour.x][neighbour.z];
                            Vector3 neighbourWorldPos = GridToWorld(neighbour);
                            agg = neighbourWorldPos - GridToWorld(currField);
                        }
                    }
                }

                agg = agg.normalized;
                flowfield[x, z] = agg;
            }
        }
        Debug.Log(integrationField.Length);

    }


    /// <summary>
    /// Get cost field values and initialize integration field
    /// </summary>
    /// <param name="costField"></param>
    private void Setup(ref byte[][] costField)
    {
        costField = new byte[size.x][];
        integrationField = new int[size.x][];
        for (int x = 0; x < size.x; x++)
        {
            costField[x] = new byte[size.z];
            integrationField[x] = new int[size.z];
            for (int z = 0; z < size.z; z++)
            {
                costField[x][z] = GetCostFieldValue(x, z);
            }
        }
        Debug.Log(costField.Length);
    }

    private byte GetCostFieldValue(int x, int z)
    {
        if (obstacles.Any(o => o.collider.bounds.Contains(GridToWorld(new Vector3Int(x, 0, z))))) {
            Debug.Log("bla");
            return 255;
        }
        return 1;
    }

    private void TestPopulateFlowfield()
    {
        for (int z = 0; z < size.z; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                GetFlowfieldTileByPerlin(x, z);
            }
        }
    }

    private void GetFlowfieldTileByPerlin(int x, int z)
    {
        float theta = Mathf.PerlinNoise(x * perlinScale, z * perlinScale);
        Vector3 dir = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
        flowfield[x, z] = dir;
        InstantiateField(x, z, dir);
    }

    private void InstantiateField(int x, int z, Vector3 dir)
    {
        GameObject go = Instantiate(fieldPrefab, GridToWorld(new Vector3Int(x, 0, z)), Quaternion.identity);
        go.transform.localScale = Vector3.one / (float)resolution;
        Transform child = go.transform.GetChild(0);
        child.LookAt(child.position + dir);
        cellToObject.Add(new Vector3Int(x, 0, z), go);
    }

    public Vector3 GetDirection(Vector3Int gridPos)
    {
        if (!IsInBounds(gridPos))
            return Vector3.zero;
        return flowfield[gridPos.x, gridPos.z];
    }

    public bool IsInBounds(Vector3Int gridPos)
    {
        return gridPos.x >= 0 &&
               gridPos.x < size.x &&
               gridPos.z >= 0 &&
               gridPos.z < size.z;
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

    private void OnDrawGizmos()
    {
        if (integrationField != null)
        {
            for (int z = 0; z < size.z; z++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Gizmos.color = new Color(0.0f, Mathf.Clamp01(integrationField[x][z] / stuff), 0.0f);
                    Gizmos.DrawSphere(GridToWorld(new Vector3Int(x, 0, z)), 0.1f);
                }
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GridToWorld(new Vector3Int(targetTestPos.x, 0, targetTestPos.y)), .1f);

        }
    }
}
