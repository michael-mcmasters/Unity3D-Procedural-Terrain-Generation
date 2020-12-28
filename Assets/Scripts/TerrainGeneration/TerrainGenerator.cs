using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TerrainGenerator : MonoBehaviour
{
    public PerlinNoise PerlinNoise;

    public GameObject ChunkPrefab;
    public GameObject cubeCollider;
    public event Action UpdateCollidersEvent;


    public Texture2D HeightMapTexture;
    public bool useHeightMapTexture;
    public bool usePerlinNoise;
    public float noise = 4;
    public float perlinHeightMultiplyer = 20;



    // 256x256 is good size
    public int terrainWidth = 32;
    public int terrainLength = 32;
    public int chunkWidth = 16;
    public int chunkLength = 16;

    private float minXMapBounds;
    private float maxXMapBounds;
    private float minZMapBounds;
    private float maxZMapBounds;


    private Voxel[,] voxels;
    public static Voxel[,] Voxels { get => TerrainGenerator.Instance.voxels; }
    public float HalfVoxelSize { get => 0.5f; }
    public float MinXMapBounds { get => minXMapBounds; }
    public float MaxXMapBounds { get => maxXMapBounds; }
    public float MinZMapBounds { get => minZMapBounds; }
    public float MaxZMapBounds { get => maxZMapBounds; }


    // Used for terraforming. Won't log anything the first time the terrain generates.
    public bool debugChunksBeingRegenerated;







    private static TerrainGenerator instance;
    public static TerrainGenerator Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TerrainGenerator>();

            return instance;
        }
    }








    protected void Awake()
    {
        if (terrainWidth % chunkWidth != 0)
            Debug.LogError("Terrain width must be perfectly divisible by chunk width. Same goes for length.");

        HeightMapTexture = GetHeightMap();
        voxels = CreateVoxels(HeightMapTexture);
        SpawnChunks();

        SetMapBounds();
    }








    // Returns true if position is inside of map boundaries.
    public bool PositionInMapBounds(Vector3 point)
    {
        bool positionInBound = true;

        if (point.x < minXMapBounds)
            positionInBound = false;

        else if (point.x > maxXMapBounds)
            positionInBound = false;

        if (point.z < minZMapBounds)
            positionInBound = false;

        else if (point.z > maxZMapBounds)
            positionInBound = false;

        return positionInBound;
    }

    // Assign new height to voxels. Then update the chunks they belong to.
    public void AssignHeightToVoxels(List<Vector3> clickPositions, int height)
    {
        List<Chunk> chunksToUpdate = UpdateVoxelData(clickPositions, height);
        UpdateChunks(chunksToUpdate);
    }

    public void UpdateColliders()
    {
        if (UpdateCollidersEvent != null)
            UpdateCollidersEvent();
    }


    // If voxel has a visible face, road can snap on to it.
    // This calculates the voxel's snap points if it has any and then determines which point is closest to the mouse.
    public Vector3 GetVoxelColliderConnectionPoint(Collider terrainCollider, Vector3 clickPosition)
    {
        if (GetCoordinatesFromMouse(out int x, out int z, terrainCollider.transform.position))
        {
            List<Vector3> terrainBlockSnapPoints = GetSnapPoints(terrainCollider, x, z);
            if (terrainBlockSnapPoints.Count > 0)
                return GetClosestSnapPointToMouse(terrainBlockSnapPoints, clickPosition);
            else
                Debug.Log("This voxel has no snap points to connect to");
        }

        return Vector3.zero;
    }

    public bool GetVoxel(out Voxel voxel, Vector3 position)
    {
        if (GetCoordinatesFromMouse(out int x, out int z, position))
        {
            voxel = Voxels[x, z];
            return true;
        }
        voxel = Voxels[0, 0];
        return false;
    }













    private Texture2D GetHeightMap()
    {
        if (useHeightMapTexture && HeightMapTexture != null)
        {
            if (HeightMapTexture.width > 256)
                Debug.LogError("Terrain's size is set to texture's pixel size, but this texture is very large, meaning the terrain will be very large. Comment this error out if you want to proceed.");

            terrainWidth = HeightMapTexture.width;
            terrainLength = HeightMapTexture.width;
        }
        else if (usePerlinNoise)
        {
            HeightMapTexture = PerlinNoise.GeneratePerlinNoise(terrainWidth, terrainLength, noise);
        }

        return HeightMapTexture;
    }


    // Voxels hold the data for where cubes will spawn. Such as their position on map and which chunk they belong to.
    private Voxel[,] CreateVoxels(Texture2D heightMapTexture)
    {
        if ((usePerlinNoise || useHeightMapTexture) && heightMapTexture == null)
        {
            Debug.LogError("Height map texture is null. Generating flat terrain instead.");
            usePerlinNoise = false;
            useHeightMapTexture = false;
        }

        Voxel[,] voxels = new Voxel[terrainWidth, terrainLength];
        for (int z = 0; z < terrainLength; z++)
        {
            for (int x = 0; x < terrainWidth; x++)
            {
                // Default to flat terrain
                voxels[x, z].position = new Vector3(x, 0, z);

                if (usePerlinNoise || useHeightMapTexture)
                {
                    float y = heightMapTexture.GetPixel(x, z).grayscale * perlinHeightMultiplyer;
                    y = Mathf.FloorToInt(y);
                    voxels[x, z].position.y = y;
                }
            }
        }
        return voxels;
    }

    private void SpawnChunks()
    {
        int spawnCounter = 0;
        for (int z = 0; z < terrainLength; z += chunkLength)
        {
            for (int x = 0; x < terrainWidth; x += chunkWidth)
            {
                GameObject ChunkObj = Instantiate(ChunkPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                ChunkObj.transform.name = ("Terrain Chunk" + spawnCounter);
                Chunk chunk = ChunkObj.GetComponent<Chunk>();
                chunk.CreateChunk(chunkWidth, chunkLength);

                spawnCounter++;
            }
        }
    }









    // The map boundaries that player is allowed to build and move camera inside of.
    private void SetMapBounds()
    {
        minXMapBounds = transform.position.x;
        maxXMapBounds = transform.position.x + terrainWidth * ChunkPrefab.transform.localScale.x;
        minZMapBounds = transform.position.z;
        maxZMapBounds = transform.position.z + terrainLength * ChunkPrefab.transform.localScale.z;
    }














    private List<Chunk> UpdateVoxelData(List<Vector3> clickPositions, int height)
    {
        List<Chunk> chunksToUpdate = new List<Chunk>();
        for (int i = 0; i < clickPositions.Count; i++)
        {
            if (GetCoordinatesFromMouse(out int x, out int z, clickPositions[i]))
            {
                // If this voxel already has given height, go to next voxel in loop
                if (voxels[x, z].position.y == height)
                    continue;

                voxels[x, z].position.y = height;

                chunksToUpdate.Add(voxels[x, z].chunk);

                int leftCubeIndex = x - 1;
                if (leftCubeIndex >= 0)
                {
                    if (voxels[x, z].chunk != voxels[leftCubeIndex, z].chunk)
                        chunksToUpdate.Add(voxels[leftCubeIndex, z].chunk);
                }
                int prevRowIndex = z - 1;
                if (prevRowIndex >= 0)
                {
                    if (voxels[x, z].chunk != voxels[x, prevRowIndex].chunk)
                        chunksToUpdate.Add(voxels[x, prevRowIndex].chunk);
                }
                // GetLength(0) gets length of first dimension in multidimensional array.
                int rightCubeIndex = x + 1;
                if (rightCubeIndex < voxels.GetLength(0))
                {
                    if (voxels[x, z].chunk != voxels[rightCubeIndex, z].chunk)
                        chunksToUpdate.Add(voxels[rightCubeIndex, z].chunk);
                }
                // GetLength(1) gets length of second dimension in multidimensional array.
                int nextRowIndex = z + 1;
                if (nextRowIndex < voxels.GetLength(1))
                {
                    if (voxels[x, z].chunk != voxels[x, nextRowIndex].chunk)
                        chunksToUpdate.Add(voxels[x, nextRowIndex].chunk);
                }
            }
        }

        return chunksToUpdate;
    }

    private bool VoxelInList(Voxel voxel, List<Vector3> clickPositions)
    {
        for (int i = 0; i < clickPositions.Count; i++)
        {
            if (voxel.position == clickPositions[i])
                return true;
        }

        return false;
    }




    private void UpdateChunks(List<Chunk> chunksToUpdate)
    {
        List<Chunk> chunksAlreadyUpdated = new List<Chunk>();
        for (int c = 0; c < chunksToUpdate.Count; c++)
        {
            // If voxels share a chunk it only needs to be updated once. Go to next loop if this chunk has already updated.
            if (ChunkAlreadyUpdated(chunksToUpdate[c], chunksAlreadyUpdated))
                continue;

            chunksToUpdate[c].UpdateChunk(chunkWidth, chunkLength);
            chunksAlreadyUpdated.Add(chunksToUpdate[c]);

            if (debugChunksBeingRegenerated)
            {
                Vector3 centerOfChunkPos = VecCalc.TransformPoint(chunksToUpdate[c].gameObject, new Vector3(chunkWidth * 0.5f, 0, chunkLength * 0.5f));
                DebugExtension.DebugWireSphere(centerOfChunkPos, Color.red, 4, 0.1f);
            }
        }
    }

    private bool ChunkAlreadyUpdated(Chunk chunkToUpdate, List<Chunk> chunksAlreadyUpdated)
    {
        for (int j = 0; j < chunksAlreadyUpdated.Count; j++)
            if (chunkToUpdate == chunksAlreadyUpdated[j])
                return true;

        return false;
    }






























    private List<Vector3> GetSnapPoints(Collider terrainCube, int x, int z)
    {
        Vector3 centerOfCubePos = terrainCube.transform.position;
        centerOfCubePos.y = terrainCube.transform.localScale.y - 0.1f;

        List<Vector3> terrainBlockSnapPoints = new List<Vector3>();
        if (voxels[x, z].facingPrevRow)
            terrainBlockSnapPoints.Add(new Vector3(centerOfCubePos.x, centerOfCubePos.y, centerOfCubePos.z - 0.5f));

        if (voxels[x, z].facingNextRow)
            terrainBlockSnapPoints.Add(new Vector3(centerOfCubePos.x, centerOfCubePos.y, centerOfCubePos.z + 0.5f));

        if (voxels[x, z].facingLeft)
            terrainBlockSnapPoints.Add(new Vector3(centerOfCubePos.x - 0.5f, centerOfCubePos.y, centerOfCubePos.z));

        if (voxels[x, z].facingRight)
            terrainBlockSnapPoints.Add(new Vector3(centerOfCubePos.x + 0.5f, centerOfCubePos.y, centerOfCubePos.z));

        return terrainBlockSnapPoints;
    }

    private Vector3 GetClosestSnapPointToMouse(List<Vector3> terrainBlockSnapPoints, Vector3 clickPosition)
    {
        Vector3 closestSnapPoint = terrainBlockSnapPoints[0];

        float lowestDistance = Mathf.Infinity;
        for (int i = 0; i < terrainBlockSnapPoints.Count; i++)
        {
            float distance = Vector3.Distance(clickPosition, terrainBlockSnapPoints[i]);
            if (distance < lowestDistance)
            {
                closestSnapPoint = terrainBlockSnapPoints[i];
                lowestDistance = distance;
            }
        }

        return closestSnapPoint;
    }





















    // Mouse x and z coordinates are the same coordinates that cubes are at.
    // x and z values are also the index values for multidimensional array for that cube. Function returns true if coordinate are in array range. False if not.
    // (Casting float to int rounds down. But should not matter since passed parameters should already be rounded to whole number before passing).
    private bool GetCoordinatesFromMouse(out int x, out int z, Vector3 position)
    {
        x = (int)position.x;
        z = (int)position.z;

        // Make sure index values are in bounds of array
        if (x >= 0 && x < voxels.GetLength(0) && z >= 0 && z < voxels.GetLength(1))
            return true;

        return false;
    }
}
