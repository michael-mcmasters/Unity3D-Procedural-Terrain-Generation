using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public const string TerrainBlockName = "Terrain Block";
    
    public enum RampDirection { None, Left, Right, Forward, Backward };

    private Voxel[,] voxels;

    public MeshFilter meshFilter;
    public Mesh mesh;
    public MeshCollider meshCollider;

    private bool needToUpdateColliders;

    public void CreateChunk(int chunkWidth, int chunkLength)
    {
        voxels = TerrainGenerator.Voxels;
        TerrainGenerator.Instance.UpdateCollidersEvent += UpdateColliders;

        mesh = new Mesh();
        meshFilter = transform.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshCollider = transform.GetComponent<MeshCollider>();

        GenerateCubes(chunkWidth, chunkLength);
        needToUpdateColliders = true;
        UpdateColliders();
    }

    public void UpdateChunk(int chunkWidth, int chunkLength)
    {
        GenerateCubes(chunkWidth, chunkLength);
        needToUpdateColliders = true;
    }

    // Spawns a collider at each block at the end of a hill.
    private void UpdateColliders()
    {
        if (needToUpdateColliders)
        {
            DestroyExistingColliders();
            PlaceCollidersOnHills();
            needToUpdateColliders = false;
        }
    }




    private void GenerateCubes(int chunkWidth, int chunkLength)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int z = (int)transform.position.z; z < chunkLength + transform.position.z; z++)
        {
            for (int x = (int)transform.position.x; x < chunkWidth + transform.position.x; x++)
            {
                voxels[x, z].chunk = this;
                voxels[x, z].facingPrevRow = false;
                voxels[x, z].facingNextRow = false;
                voxels[x, z].facingLeft = false;
                voxels[x, z].facingRight = false;
                Vector3 voxelPosition = voxels[x, z].position;
                int y = (int) voxels[x, z].position.y;

                // Always draw top face
                AddTrisAndUVSForNewFace(vertices, triangles, uvs, 0);
                vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
                vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));

                if (ShouldDrawFrontFace(x, y, z))
                {
                    voxels[x, z].facingPrevRow = true;
                    AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
                }
                if (ShouldDrawBackFace(x, y, z))
                {
                    voxels[x, z].facingNextRow = true;
                    AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                }
                if (ShouldDrawLeftFace(x, y, z))
                {
                    voxels[x, z].facingLeft = true;
                    AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                }
                if (ShouldDrawRightFace(x, y, z))
                {
                    voxels[x, z].facingRight = true;
                    AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, 0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
                    vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }









    // (add / subtract 0.05 to texture coordinates so that image doesn't bleed in to image beside it in atlas).
    private void AddTrisAndUVSForNewFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, int texture = 1)
    {
        triangles.Add(0 + vertices.Count);
        triangles.Add(2 + vertices.Count);
        triangles.Add(3 + vertices.Count);
        triangles.Add(3 + vertices.Count);
        triangles.Add(1 + vertices.Count);
        triangles.Add(0 + vertices.Count);

        // grass
        if (texture == 0)
        {
            uvs.Add(new Vector2(0.05f, 0.05f));
            uvs.Add(new Vector2(0.09f, 0.05f));
            uvs.Add(new Vector2(0.05f, 0.08f));
            uvs.Add(new Vector2(0.09f, 0.08f));
        }

        // dirt
        else if (texture == 1)
        {
            uvs.Add(new Vector2(0.05f, 0.13f));
            uvs.Add(new Vector2(0.09f, 0.13f));
            uvs.Add(new Vector2(0.05f, 0.24f));
            uvs.Add(new Vector2(0.09f, 0.24f));
        }
    }

    private void AddTrisAndUVsForTriangle(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        triangles.Add(0 + vertices.Count);
        triangles.Add(1 + vertices.Count);
        triangles.Add(2 + vertices.Count);

        uvs.Add(new Vector2(0.05f, 0.05f));
        uvs.Add(new Vector2(0.09f, 0.05f));
        uvs.Add(new Vector2(0.05f, 0.08f));
    }






    // ******* Note for all 4 functions ********
    // Use x and z coordinates to get other cube's index. (the cube next to current cube.) Works even if other cube is not in this chunk.
    // Use y to determine if current cube is higher than that cube. If so, draw face.
    // Otherwise, don't draw face because other cube is covering it which means player won't see it.

    private bool ShouldDrawFrontFace(int x, int y, int z)
    {
        int prevRowIndex = z - 1;

        // Means this is the first row in chunk
        if (prevRowIndex < 0)
            return true;

        if (y > voxels[x, prevRowIndex].position.y)
            return true;

        return false;
    }

    private bool ShouldDrawBackFace(int x, int y, int z)
    {
        int nextRowIndex = z + 1;

        // Get length of second value in multidimensional array by using index (1) as paremeter.
        // Condition is true if current cube is in last row of chunk
        if (nextRowIndex > voxels.GetLength(1) - 1)
            return true;

        // If current cube is higher than cube in next row, draw face
        if (y > voxels[x, nextRowIndex].position.y)
            return true;

        return false;
    }

    private bool ShouldDrawLeftFace(int x, int y, int z)
    {
        int prevCubeIndex = x - 1;

        // Means this is first cube in chunk
        if (prevCubeIndex < 0)
            return true;

        if (y > voxels[prevCubeIndex, z].position.y)
            return true;

        return false;
    }

    private bool ShouldDrawRightFace(int x, int y, int z)
    {
        int nextCubeIndex = x + 1;

        // Means this is last cube in chunk
        if (nextCubeIndex > voxels.GetLength(0) - 1)
            return true;

        if (y > voxels[nextCubeIndex, z].position.y)
            return true;

        return false;
    }




    private void DestroyExistingColliders()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i < transform.childCount)
                Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void PlaceCollidersOnHills()
    {
        for (int z = (int)transform.position.z; z < TerrainGenerator.Instance.chunkLength + transform.position.z; z++)
        {
            for (int x = (int)transform.position.x; x < TerrainGenerator.Instance.chunkWidth + transform.position.x; x++)
            {
                if (voxels[x, z].facingPrevRow || voxels[x, z].facingNextRow || voxels[x, z].facingLeft || voxels[x, z].facingRight
                    || voxels[x, z].facingNextLeft || voxels[x, z].facingPrevLeft || voxels[x, z].facingPrevRight || voxels[x, z].facingNextRight)
                {
                    Vector3 spawnPosition = voxels[x, z].position;

                    // y is center of cube. Add 0.06 so that z scale is slightly above cube so that game objects collides with it if at edge of cliff.
                    float yScale = voxels[x, z].position.y + 0.6f;
                    spawnPosition.y = yScale * 0.5f;
                    GameObject collider = Instantiate(TerrainGenerator.Instance.cubeCollider, spawnPosition, Quaternion.identity, transform);
                    collider.transform.localScale = new Vector3(1, yScale, 1);
                    collider.transform.name = TerrainBlockName;
                }
            }
        }
    }
}