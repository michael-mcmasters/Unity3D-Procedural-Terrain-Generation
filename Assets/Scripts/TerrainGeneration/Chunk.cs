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

    // Called from delegate event
    // Spawns colliders on each block that is at a hill (so blocks showing a visible face)
    private void UpdateColliders()
    {
        if (needToUpdateColliders)
        {
            //Debug.Log("Updating Colliders!!");
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
                voxels[x, z].facingNextLeft = false;
                voxels[x, z].facingNextRight = false;
                voxels[x, z].facingPrevLeft = false;
                voxels[x, z].facingPrevRight = false;
                Vector3 voxelPosition = voxels[x, z].position;
                int y = (int)voxels[x, z].position.y;



                voxels[x, z].GetLeftVoxel(out Voxel leftVoxel);
                voxels[x, z].GetRightVoxel(out Voxel rightVoxel);
                voxels[x, z].GetNextRowVoxel(out Voxel nextRowVoxel);
                voxels[x, z].GetPrevRowVoxel(out Voxel prevRowVoxel);
                voxels[x, z].GetTopLeftVoxel(out Voxel topLeftVoxel);
                voxels[x, z].GetTopRightVoxel(out Voxel topRightVoxel);
                voxels[x, z].GetBottomLeftVoxel(out Voxel bottomLeftVoxel);
                voxels[x, z].GetBottomRightVoxel(out Voxel bottomRightVoxel);


                // // !!! This comment is old and not true anymore.
                // // 0: Lower by more than 1 unit.
                // // 1: Lower by 1 unit
                // // 2: Same height or higher
                // int[] voxelHeights = GetVoxelHeights(topLeftVoxel, nextRowVoxel, topRightVoxel, leftVoxel, voxels[x, z], rightVoxel, bottomLeftVoxel, prevRowVoxel, bottomRightVoxel);

                // // x 2 x
                // // 1 2 2
                // // x 1 x
                // // top, left, right, down, corner
                // if (voxelHeights[1] == 2 && voxelHeights[3] == 1 && voxelHeights[5] == 2 && voxelHeights[7] == 1)
                // {
                //     voxels[x, z].facingPrevLeft = true;

                //     // Bottom left face
                //     // low bottom left, low bottom right, high top left, high bottom right
                //     AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));

                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     // Top floor face
                //     // top left, top right, bottom right
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));

                //     // Check left and prev row voxels and make floor the lower height of the two.
                //     float lowestVoxelHeight = GetLowerHeight(leftVoxel.position.y, prevRowVoxel.position.y);
                //     float groundPos = -voxelPosition.y + lowestVoxelHeight + 0.5f;
                //     // Bottom floor face
                //     // bottom left, top left, bottom right
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, -0.5f));
                // }

                // // x 2 x
                // // 2 2 1
                // // x 1 x
                // // bottom right corner
                // else if (voxelHeights[1] == 2 && voxelHeights[3] == 2 && voxelHeights[5] == 1 && voxelHeights[7] == 1)
                // {
                //     voxels[x, z].facingPrevRight = true;

                //     AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));

                //     // top face: bottom left, top left, top right
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));

                //     // bottom floor face: bottom left, top right, bottom right
                //     float lowerHeight = GetLowerHeight(leftVoxel.position.y, prevRowVoxel.position.y);
                //     float groundPos = -voxelPosition.y + lowerHeight + 0.5f;
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, -0.5f));
                // }

                // // x 1 x
                // // 1 2 2
                // // x 2 x
                // // top left corner
                // else if (voxelHeights[1] == 1 && voxelHeights[3] == 1 && voxelHeights[5] == 2 && voxelHeights[7] == 2)
                // {
                //     voxels[x, z].facingNextLeft = true;

                //     AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));

                //     // top face: bottom left, top right, bottom right
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));

                //     // bottom floor face: bottom left, top left, top right
                //     float lowerHeight = GetLowerHeight(leftVoxel.position.y, nextRowVoxel.position.y);
                //     float groundPos = -voxelPosition.y + lowerHeight + 0.5f;
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, 0.5f));
                // }

                // // x 1 x
                // // 2 2 1
                // // x 2 x
                // // top right corner
                // else if (voxelHeights[1] == 1 && voxelHeights[3] == 2 && voxelHeights[5] == 1 && voxelHeights[7] == 2)
                // {
                //     voxels[x, z].facingNextRight = true;

                //     AddTrisAndUVSForNewFace(vertices, triangles, uvs, 1);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -voxelPosition.y - 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -voxelPosition.y - 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));

                //     // top face: bottom left, top left, bottom right
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));

                //     // bottom floor face: top left, top right, bottom right
                //     float lowerHeight = GetLowerHeight(nextRowVoxel.position.y, rightVoxel.position.y);
                //     float groundPos = -voxelPosition.y + lowerHeight + 0.5f;
                //     AddTrisAndUVsForTriangle(vertices, triangles, uvs);
                //     vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, groundPos, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, 0.5f));
                //     vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, groundPos, -0.5f));
                // }


                // // Regular block
                // else
                // {
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
                //}
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









    // 0: Lower by more than 1 unit.
    // 1: Lower by 1 unit
    // 2: Same height or higher
    private int[] GetVoxelHeights(Voxel topLeftVoxel, Voxel nextRowVoxel, Voxel topRightVoxel, Voxel leftVoxel, Voxel thisVoxel, Voxel rightVoxel, Voxel bottomLeftVoxel, Voxel prevRowVoxel, Voxel bottomRightVoxel)
    {
        int[] numbers = new int[9];
        numbers[0] = GetVoxelHeight(thisVoxel, topLeftVoxel);
        numbers[1] = GetVoxelHeight(thisVoxel, nextRowVoxel);
        numbers[2] = GetVoxelHeight(thisVoxel, topRightVoxel);

        numbers[3] = GetVoxelHeight(thisVoxel, leftVoxel);
        numbers[4] = 2;
        numbers[5] = GetVoxelHeight(thisVoxel, rightVoxel);

        numbers[6] = GetVoxelHeight(thisVoxel, bottomLeftVoxel);
        numbers[7] = GetVoxelHeight(thisVoxel, prevRowVoxel);
        numbers[8] = GetVoxelHeight(thisVoxel, bottomRightVoxel);

        return numbers;
    }

    private int GetVoxelHeight(Voxel voxel, Voxel otherVoxel)
    {
        // If otherVoxel has the same position as this voxel, it means the other voxel doesn't exist. For example, there is no voxel to the left of the very first voxel (voxels[-x, 0])
        // When getting voxels, if one does not exist, it returns this voxel. So use this comparison to determine if voxel exists.
        // If it does not. Return 0 for very low, so that this voxel draws a face on that side.
        if (otherVoxel.position == voxel.position)
        {
            //Debug.Log("nulll");
            return 0;
        }


        if (otherVoxel.position.y < voxel.position.y - 0.1f)
        {
            // if (otherVoxel.position.y > voxel.position.y - 1.1f)
            // {
            //     // Lower by 1 unit
            //     return 1;
            // }
            // // Lower by more than 1 unit (really low)
            // return 0;

            return 1;
        }
        else if (otherVoxel.position.y >= voxel.position.y)
        {
            // Same height or higher
            return 2;
        }
        else
        {
            throw new System.Exception("Can't calculate other voxel's height");
        }
    }


    private bool CheckVoxelHeights(int[] voxelHeights, int a, int b, int c, int d, int e, int f, int g, int h, int i)
    {
        if (voxelHeights[0] == a && voxelHeights[1] == b && voxelHeights[2] == c
        && voxelHeights[3] == d && voxelHeights[4] == e && voxelHeights[5] == f
        && voxelHeights[6] == g && voxelHeights[7] == h && voxelHeights[8] == i)
        {
            return true;
        }
        return false;
    }

    private float GetLowerHeight(float a, float b)
    {
        if (a < b)
            return a;
        else
            return b;
    }










    private void DrawCube(ref List<Vector3> vertices, Vector3 voxelPosition, int faceDirection)
    {
        // left
        if (faceDirection == 0)
        {

        }
        // right
        else if (faceDirection == 1)
        {

        }
        // prev row
        else if (faceDirection == 2)
        {

        }
        // next row
        else if (faceDirection == 3)
        {

        }
        // top
        else if (faceDirection == 4)
        {
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    // Last parameter is direction ramp is facing
    private void DrawRamp(ref List<Vector3> vertices, Vector3 cubePosition, int direction)
    {
        float rampGroundPosition = 0.5f;
        float rampDistance = 0.5f;

        // left
        if (direction == 0)
        {
            vertices.Add(cubePosition - transform.position + new Vector3(-rampDistance, -rampGroundPosition, 0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(-rampDistance, -rampGroundPosition, -0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
        }
        // right
        else if (direction == 1)
        {
            vertices.Add(cubePosition - transform.position + new Vector3(rampDistance, -rampGroundPosition, -0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(rampDistance, -rampGroundPosition, 0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
        }
        // next row
        else if (direction == 2)
        {
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, -rampGroundPosition, rampDistance));
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, -rampGroundPosition, rampDistance));
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
        }
        // prev row
        else if (direction == 3)
        {
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, -rampGroundPosition, -rampDistance));
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, -rampGroundPosition, -rampDistance));
            vertices.Add(cubePosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
            vertices.Add(cubePosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    // Face direction is at an angle
    // Ramps draw two faces. One for the actual ramp, and one for the ground to fill the rest of the tile.
    private void DrawCorner(ref List<Vector3> vertices, Vector3 voxelPosition, int faceDirection)
    {
        // top left
        if (faceDirection == 0)
        {
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, 0.5f));

            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));
        }
        // top right
        else if (faceDirection == 1)
        {
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));

            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, -0.5f));
        }
        // bottom left
        else if (faceDirection == 2)
        {
            // vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            // vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            // vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));

            // vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            // vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
            // vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));

            // Top face
            // top left, top right, bottom right
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, 0.5f, -0.5f));


            // Bottom Floor
            // bottom left, top left, bottom right
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));

        }
        // bottom right
        else if (faceDirection == 3)
        {
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, -0.5f));

            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, -0.5f, -0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(-0.5f, 0.5f, 0.5f));
            vertices.Add(voxelPosition - transform.position + new Vector3(0.5f, -0.5f, 0.5f));
        }
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
            // uvs.Add(new Vector2(0.01f, 0.13f));
            // uvs.Add(new Vector2(0.12f, 0.13f));
            // uvs.Add(new Vector2(0.01f, 0.24f));
            // uvs.Add(new Vector2(0.12f, 0.24f));

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

        // ToDo: Set actual UVs. These are just copied and pasted from cube's uv coordinates.
        // grass
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

                    // y is center of cube. Add 0.06 so that z scale is slightly above cube so that road collides with it if trying to build off of a cliff.
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