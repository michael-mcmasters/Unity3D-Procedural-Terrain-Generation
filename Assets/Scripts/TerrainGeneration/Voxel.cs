using UnityEngine;

public struct Voxel
{
    // public Chunk chunk;
    // public Vector3 position;
    // public bool isRamp;

    // // Also serve as ramp direction, because ramps only spawn if one cube is lower than it.
    // public bool facingPrevRow;
    // public bool facingNextRow;
    // public bool facingLeft;
    // public bool facingRight;
    // public bool facingNextLeft;
    // public bool facingPrevLeft;
    // public bool facingPrevRight;
    // public bool facingNextRight;








    // /* Returns true if left voxel exists. False if not.
    //    Subtract/add from x and z indexes to get next voxels in array */


    // public bool GetRightVoxel(out Voxel rightVoxel)
    // {
    //     int x = (int)position.x + 1;
    //     if (x < TerrainGenerator.Voxels.GetLength(0))
    //     {
    //         rightVoxel = TerrainGenerator.Voxels[x, (int)position.z];
    //         return true;
    //     }
    //     rightVoxel = this;
    //     return false;
    // }

    // public bool GetLeftVoxel(out Voxel leftVoxel)
    // {
    //     int x = (int)position.x - 1;
    //     if (x >= 0)
    //     {
    //         leftVoxel = TerrainGenerator.Voxels[x, (int)position.z];
    //         return true;
    //     }
    //     leftVoxel = this;
    //     return false;
    // }

    // public bool GetPrevRowVoxel(out Voxel prevRowVoxel)
    // {
    //     int z = (int)position.z - 1;
    //     if (z >= 0)
    //     {
    //         prevRowVoxel = TerrainGenerator.Voxels[(int)position.x, z];
    //         return true;
    //     }
    //     prevRowVoxel = this;
    //     return false;
    // }

    // public bool GetNextRowVoxel(out Voxel nextRowVoxel)
    // {
    //     int z = (int)position.z + 1;
    //     if (z < TerrainGenerator.Voxels.GetLength(1))
    //     {
    //         nextRowVoxel = TerrainGenerator.Voxels[(int)position.x, z];
    //         return true;
    //     }
    //     nextRowVoxel = this;
    //     return false;
    // }

    // public bool GetTopLeftVoxel(out Voxel topLeftVoxel)
    // {
    //     int x = (int)position.x - 1;
    //     int z = (int)position.z + 1;
    //     if (x >= 0 && z < TerrainGenerator.Voxels.GetLength(1))
    //     {
    //         topLeftVoxel = TerrainGenerator.Voxels[x, z];
    //         return true;
    //     }
    //     topLeftVoxel = this;
    //     return false;
    // }

    // public bool GetTopRightVoxel(out Voxel topRightVoxel)
    // {
    //     int x = (int)position.x + 1;
    //     int z = (int)position.z + 1;
    //     if (x < TerrainGenerator.Voxels.GetLength(0) && z < TerrainGenerator.Voxels.GetLength(1))
    //     {
    //         topRightVoxel = TerrainGenerator.Voxels[x, z];
    //         return true;
    //     }
    //     topRightVoxel = this;
    //     return false;
    // }

    // public bool GetBottomLeftVoxel(out Voxel bottomLeftVoxel)
    // {
    //     int x = (int)position.x - 1;
    //     int z = (int)position.z - 1;
    //     if (x >= 0 && z >= 0)
    //     {
    //         bottomLeftVoxel = TerrainGenerator.Voxels[x, z];
    //         return true;
    //     }
    //     bottomLeftVoxel = this;
    //     return false;
    // }

    // public bool GetBottomRightVoxel(out Voxel bottomRightVoxel)
    // {
    //     int x = (int)position.x + 1;
    //     int z = (int)position.z - 1;
    //     if (x < TerrainGenerator.Voxels.GetLength(0) && z >= 0)
    //     {
    //         bottomRightVoxel = TerrainGenerator.Voxels[x, z];
    //         return true;
    //     }
    //     bottomRightVoxel = this;
    //     return false;
    // }
}
