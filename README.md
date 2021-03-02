## Unity3D-Procedural-Terrain-Generation

[![Gif](https://j.gifs.com/ANv7R9.gif)](https://www.youtube.com/watch?v=a14CbcjMOIs&feature=youtu.be)

## About

This is a small portion of a larger project I'm working on for the Oculus Virtual Reality family of systems. The project generates a random terrain at runtime with a button to generate a new one. Or you can give it a heightmap texture, [such as the one seen here](https://i0.wp.com/www.studica.com/blog/storage/2018/08/Heightmap.png?ssl=1), and it will generate a terain based off of that. The camera panning, zomming and rotations were created by me using only the Unity library.
In the future I'll create terraforming, enabling you to click and drag along the terrain to raise and lower it.

I'm expecially proud of this project because it entered me into Facebook's Oculus Start program, in which they paid for my Unity Plus subscription and gave me an Oculus Rift at no charge, along with a hoodie and notebook and some other cool items. 

## How it works

I use a multidimensional array to represent the x, y, and z coordinates of the terrain.
I then split the terrain into chunks. So a 64x64 terrain would be made up of 16*16 chunks (as seen in the gif above).
Splitting the terrain into chunks improves performance because when terraforming (will be added soon), you only need to regenerate that one chunk instead of the entire terrain.

The multidimensional array that makes up the terrain holds a collection of voxels (C# structs), which each represents one block.
To reference a voxel, you simply pass in its x and z coordinates as index values.<br>
For example, to get the block at coordinates (x: 50, z: 50), you pass in those coordinates to the array.<br>
`Voxel voxel = voxels[50, 50];`

This system allows us to read and update terrain data using random access in constant time O(1).

Each voxel holds its own y position, its neighboring voxels, and the chunk it belongs to.
To update a voxel's height you simply assign its y property a new value and then regenerate the mesh.
`voxel.y += 10;`

The chunk the voxel belongs to and all neighboring chunks will regenerate.
The reason for this is because only the sides of the blocks visible to the player are drawn. If a face is going to be covered by another block, there is no reason to draw it. But when moving a block, its neighbords need to be redrawn to make sure there are no holes in the terrain.

To draw the cubes, a list of vertice coordinates are created, offset by the voxel's position. A list of triangles are then created in a clockwise order to make sure the normals are facing the player. The face would be invisible otherwise. And then finally a list of UVs are created to tell the vertices how to place the texture.
