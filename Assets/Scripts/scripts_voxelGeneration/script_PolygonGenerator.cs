﻿using UnityEngine;
using System.Collections.Generic;

public class script_PolygonGenerator : MonoBehaviour 
{
    public  int           worldsizeX     = 96;
    public  int           worldsizeY     = 128;
    
    public  List<Vector3> newVertices    = new List<Vector3>();                     // This first list contains every vertex of the mesh that we are going to render
    public  List<int>     newTriangles   = new List<int>();                         // The triangles tell Unity how to build each section of the mesh joining the vertices
    public  List<Vector2> newUV          = new List<Vector2>();                     // The UV list is unimportant right now but it tells Unity how the texture is aligned on each polygon

    private Mesh          mesh;                                                     // A mesh is made up of the vertices, triangles and UVs we are going to define, after we make them up we'll save them as this mesh
    private float         tUnit          = 0.25f;                                   // tUnit is the fraction of space 1 tile takes up out of the width of the texture. ## In this case it's 1/4 or 0.25 ## 
    private Vector2       tStone         = new Vector2( 3, 2 );                     // The coordinates for textures
    private Vector2       tDirt          = new Vector2( 1, 1 );
    private Vector2       tSand          = new Vector2( 0, 1 );
    private Vector2       tGrass         = new Vector2( 2, 1 );

    private int           squareCount;
    public byte[,]        blocks;                                                   // A 2d array to store block information so add a 2d byte array 
                                                                                    // A byte array is an easy choice for level information. It supports numbers 0-255 so that's a lot of blocks and it saves us the hassle of using enumerators. 
                                                                                    // What we'll do is have 0 be air, 1 is rock and 2 is grassand that should be enough for now.
    public  List<Vector3> colVertices    = new List<Vector3>();
    public  List<int>     colTriangles   = new List<int>();
    private int           colCount;
    private MeshCollider  col;

    public  bool          update         = false;

    void Start () 
    {
        mesh        = GetComponent<MeshFilter>().mesh;
        col         = GetComponent<MeshCollider>();

        GenTerrain  ();
        BuildMesh   ();
        UpdateMesh  ();
    }

    void Update()
    {
        if ( update )
        {
            BuildMesh();
            UpdateMesh();
            update = false;
        }
        
        GenTerrain();
        BuildMesh();
        UpdateMesh();
        
    }


    #region function BuildMesh
    void BuildMesh()                                                                // This runs through every block in the array and if the byte is 1 it creates runs the GenSquare function using the array index as the position and stone as the texture and if the byte is 2 it does the same with a grass texture.
    {
        for ( int px=0; px < blocks.GetLength( 0 ); px++ )
        {
            for ( int py=0; py < blocks.GetLength( 1 ); py++ )
            {
                if ( blocks[ px, py ] != 0 )                                        // If the block is not air
                {
                    GenCollider( px, py );                                          // GenCollider here, this will apply it to every block other than air

                    if      ( blocks[ px, py ] == 1 )       // 1 = Stone
                    {
                        GenSquare( px, py, tStone );   
                    }
                    else if ( blocks[ px, py ] == 2 )       // 2 = Dirt
                    {
                        GenSquare( px, py, tDirt );     
                    }
                    else if ( blocks[ px, py ] == 3 )       // 3 = Grass 
                    {
                        GenSquare( px, py, tGrass );  
                    }
                    else if ( blocks[ px, py ] == 4 )       // 4 = Sand
                    {
                        GenSquare( px, py, tSand  );  
                    }
                }
            }
        }
    }
    #endregion

    // Grass
    public float grass_scale    = 2;
    public float grass_mag      = 2.5f;
    // Dirt
    public float dirt_01_scale  = 100;
    public float dirt_01_mag    = 35;
    public float dirt_01_exp    = 1;

    public float dirt_02_scale  = 50;
    public float dirt_02_mag    = 30;
    public float dirt_02_exp    = 1;

    public int   dirt_height    = 75;
    // Stone
    public float stone_01_scale = 80;
    public float stone_01_mag   = 15;
    public float stone_01_exp   = 1;

    public float stone_02_scale = 50;
    public float stone_02_mag   = 30;
    public float stone_02_exp   = 1;

    public float stone_03_scale = 10;
    public float stone_03_mag   = 10;
    public float stone_03_exp   = 1;

    public int   stone_height   = 75;

    // Sand
    public float sand_01_scale  = 60;
    public float sand_01_mag    = 10;
    public float sand_01_exp    = 1;

    public float sand_02_scale  = 30;
    public float sand_02_mag    = 15;
    public float sand_02_exp    = 1;

    public int   sand_height    = 75;

    // Spots
    public int   spotSize       = 10;
    public int   spotWidth      = 2;

    public int   spot_stoneScale= 10;
    public int   spot_stoneMag  = 20;
    public int   spot_stoneExp  = 1;

    public int   spot_dirtScale = 12;
    public int   spot_dirtMag   = 16;
    public int   spot_dirtExp   = 1;

    public int   spot_sandScale = 20;
    public int   spot_sandMag   = 17;
    public int   spot_sandExp   = 1;

    public int   spot_caveScale = 16;
    public int   spot_caveMag   = 14;
    public int   spot_caveExp   = 1;


    #region function GenTerrain
    void GenTerrain()                                                                                               // This makes blocks a 10x10 array then goes through each block making any block with a y less that 5 into rock and the row at 5 into grass.
    {
        blocks = new byte[ worldsizeX, worldsizeY ];

        for ( int px = 0; px < blocks.GetLength( 0 ); px++ )
        {
            #region Description [Layers]
            /*
                We create a stone int and a dirt int and using a few layers of perlin noise they get more textured values. 
                Because this is essentially a 1d heightmap we only need x and the y variable can be used just to sample from 
                a different area to make sure the results aren't the same. You can see the stone is three noise layers with different values.
            
                Layer 1 has a scale of 80 making it quite smooth with large rolling hills, the magnitude is 15 so the hills are at most 
                15 high (but in practice they're usually around 12 at the most) and at the least 0 and the exponent is 1 so no change is applied exponentially.

                Layer 2 has a smaller scale so it's more choppy (but still quite tame) and has a larger magnitude so a higher max height. 
                This ends up being the most prominent layer making the hills.

                Layer 3 has an even smaller scale so it's even noisier but it's magnitude is 10 so its max height is lower, 
                it's mostly for adding some small noise to the stone to make it look more natural.
              
               The dirt layer has to be mostly higher than the stone so the magnitudes here are higher but the scales are 100 and 50 
               which gives us rolling hills with little noise. Again we add 75 to raise it up.
            */
            #endregion 

            int stone  = Noise( px, 0, stone_01_scale, stone_01_mag, stone_01_exp );                                //  [Layer 1] 
                stone += Noise( px, 0, stone_02_scale, stone_02_mag, stone_02_exp );                                //  [Layer 2]
                stone += Noise( px, 0, stone_03_scale, stone_03_mag, stone_03_exp );                                //  [Layer 3] 
                stone += stone_height;                                                                              // Lastly we add 75 to the stone to raise it up.

            int dirt   = Noise( px, 0, dirt_01_scale, dirt_01_mag, dirt_01_exp );
                dirt  += Noise( px, 0, dirt_02_scale, dirt_02_mag, dirt_02_exp );
                dirt  += dirt_height;

            int grass  = Noise( px, 0, dirt_01_scale              , dirt_01_mag            , dirt_01_exp );
                grass += Noise( px, 0, dirt_02_scale + grass_scale, dirt_02_mag + grass_mag, dirt_02_exp );
                grass += dirt_height;

            int sand   = Noise( px, 0, sand_01_scale, sand_01_mag, sand_01_exp );
                sand  += Noise( px, 0, sand_02_scale, sand_02_mag, sand_02_exp );
                sand  += sand_height;

            for ( int py = 0; py < blocks.GetLength( 1 ); py++ )                                                    // Loop will make stone a certain height
            {
                #region Description [Dirt spots, caves etc.]
                /*
                So you see inside the stone if ( if ( py < stone ) ) we also have an if that compares noise with 10 so if the noise we 
                return is larger than 10 it turns the block to dirt instead of stone. The magnitude of the noise value is 16 so it 
                reruns a over 10 only a little of the time and the scale is fairly low so the spots are pretty small and frequent. 
                We're using x and y here and running the if for every block so the dirt is distributed through the whole array. 
                
                After that we add caves with a similar function but we multiply y by two to stretch out the caves so they are wider 
                than they are tall and we use a larger scale to make larger less frequent caves and the magnitude is lower to reduce 
                the size of the caves that was increased by the scale. 
                */
                #endregion

                if ( py < stone )
                {
                    blocks[ px, py ] = 1;

                    if ( Noise( px, py            , spot_sandScale, spot_sandMag, spot_sandExp ) > spotSize )
                    {
                        blocks[ px, py ] = 4;   // block = sand
                    }
                    if ( Noise( px, py            , spot_dirtScale, spot_dirtMag, spot_dirtExp ) > spotSize )       // The next three lines make dirt spots in random places
                    {
                        blocks[ px, py ] = 2;   // block = Dirt
                    }
                    if ( Noise( px, py * spotWidth, spot_caveScale, spot_caveMag, spot_caveExp ) > spotSize )       // The next three lines remove dirt and rock to make caves in certain places
                    { 
                        blocks[ px, py ] = 0;   // block = air
                    }
                }
                else if ( py < dirt )
                {
                    blocks[ px, py ] = 2;       // block = Dirt

                    if ( Noise( px, py, spot_stoneScale, spot_stoneMag, spot_stoneExp ) > spotSize )
                    {
                        blocks[ px, py ] = 1;   // block = Stone
                    }
                    if ( Noise( px, py, spot_sandScale, spot_sandMag, spot_sandExp ) > spotSize )
                    {
                        blocks[ px, py ] = 4;   // block = sand
                    }
                    if ( Noise( px, py * spotWidth, spot_caveScale, spot_caveMag, spot_caveExp ) > spotSize )       // The next three lines remove dirt and rock to make caves in certain places
                    {
                        blocks[ px, py ] = 0;   // block = air
                    }
                }
                else if ( py < grass )
                {
                    blocks[ px, py ] = 3;       // block = Grass
                }
                else if ( py < sand )
                {
                    blocks[ px, py ] = 4;       // block = sand
                }
            }
        }
    }
    #endregion

    #region function UpdateMesh
    void UpdateMesh()
    {
        mesh.Clear();                                                               // Then last of all we clear anything in the mesh to begin with, we set the mesh's vertices to ours
        mesh.vertices       = newVertices.ToArray();                                // ... (But we have to convert the list to an array with the .ToArray() command) and set the mesh's triangles to ours.
        mesh.triangles      = newTriangles.ToArray();
        mesh.uv             = newUV.ToArray();
        mesh.Optimize();                                                            // Unity does some work for us when we call the optimize command (This usually does nothing but it doesn't use any extra time so don't worry)
        mesh.RecalculateNormals();                                                  // The recalculate normals command so that the normals are generated automatically.

        squareCount         = 0;
        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();

        Mesh newMesh        = new Mesh();
        newMesh.vertices    = colVertices.ToArray();
        newMesh.triangles   = colTriangles.ToArray();
        col.sharedMesh      = newMesh;

        colVertices.Clear();
        colTriangles.Clear();
        colCount            = 0;
    }
    #endregion

    #region function GenSquare
    void GenSquare( int x, int y, Vector2 texture )
    {
        newVertices.Add( new Vector3( x    , y    , 0 ) );                           // These lines defines the corners of a square
        newVertices.Add( new Vector3( x + 1, y    , 0 ) );
        newVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
        newVertices.Add( new Vector3( x    , y - 1, 0 ) );


        newTriangles.Add(   squareCount * 4 );                                       // We define the triangles. Because without them all we have is points in space, we have to show how those points are connected and we connect them in triangles.
        newTriangles.Add( ( squareCount * 4 ) + 1 );                                 // What we do is add (squareCount*4) to each number we .Add() to newTriangles. 
        newTriangles.Add( ( squareCount * 4 ) + 3 );
        newTriangles.Add( ( squareCount * 4 ) + 1 );
        newTriangles.Add( ( squareCount * 4 ) + 2 );
        newTriangles.Add( ( squareCount * 4 ) + 3 );

        newUV.Add( new Vector2( tUnit * texture.x        , tUnit * texture.y + tUnit ) );
        newUV.Add( new Vector2( tUnit * texture.x + tUnit, tUnit * texture.y + tUnit ) );
        newUV.Add( new Vector2( tUnit * texture.x + tUnit, tUnit * texture.y         ) );
        newUV.Add( new Vector2( tUnit * texture.x        , tUnit * texture.y         ) );

        squareCount++;
    }
    #endregion

    #region function GenCollider
    void GenCollider( int x, int y )
    {
        #region block_top
        if ( Block( x, y + 1 ) == 0 )
        {
            colVertices.Add( new Vector3( x    , y, 1 ) );
            colVertices.Add( new Vector3( x + 1, y, 1 ) );
            colVertices.Add( new Vector3( x + 1, y, 0 ) );
            colVertices.Add( new Vector3( x    , y, 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_bot
        if ( Block( x, y - 1 ) == 0 )
        {
            colVertices.Add( new Vector3( x    , y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 1 ) );
            colVertices.Add( new Vector3( x    , y - 1, 1 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_left
        if ( Block( x - 1, y ) == 0 )
        {
            colVertices.Add( new Vector3( x, y - 1, 1 ) );
            colVertices.Add( new Vector3( x, y    , 1 ) );
            colVertices.Add( new Vector3( x, y    , 0 ) );
            colVertices.Add( new Vector3( x, y - 1, 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_right
        if ( Block( x + 1, y ) == 0 )
        {
            colVertices.Add( new Vector3( x + 1, y    , 1 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 1 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y    , 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion
    }
    #endregion

    #region function ColliderTriangles
    void ColliderTriangles()
    {
        colTriangles.Add(   colCount * 4 );
        colTriangles.Add( ( colCount * 4 ) + 1 );
        colTriangles.Add( ( colCount * 4 ) + 3 );
        colTriangles.Add( ( colCount * 4 ) + 1 );
        colTriangles.Add( ( colCount * 4 ) + 2 );
        colTriangles.Add( ( colCount * 4 ) + 3 );
    }
    #endregion

    #region function Block
    byte Block( int x, int y )
    {
        #region Description
        /*
        If you were to extend the size of the array for more squares, you would run into an efficiency problem because every solid square is creating eight triangles.  
        That's a lot more than we need so we need a way to only make these colliders when they face an empty block.
        For that, this function checks the contents of a block.
        
        The function checks if the block you're checking is within the array's boundaries, if not it returns 1 (Solid rock) otherwise it returns the block's value.
        This means that we can use this places where we're not sure that the block we're checking is within the level size.
        Use this in the collider function.
        */
        #endregion

        if ( x == -1 || x == blocks.GetLength( 0 ) || y == -1 || y == blocks.GetLength( 1 ) )
        {
            return ( byte ) 1;
        }
        return blocks[ x, y ];
    }
    #endregion 

    #region Perlin Noise
    int Noise( int x, int y, float scale, float mag, float exp )
    {
        /*
        Perlin noise is an algorithm created by Ken Perlin to create gradient noise.
        - all you really need to know is that it returns values between 1 and 0 based on the values you enter.
         
        What the function above does is it takes coordinates for x and y to sample for noise, 
        then it calls the perlin noise function with those divided by scale. Because perlin 
        noise isn't random but bases itself on the coordinates supplied then the closer those 
        coordinates are to each other the more similar the values it returns. So when we divide 
        the coordinates by a number they end up as smaller numbers closer to each other. 
        (1,0) and (2,0) might return 0.5 and 0.3 respectively but if we divide them by two 
        calling perlin noise for (0.5,0) and (1,0) instead the numbers might be 0.4 and 0.5. 
        This will be more clear once we apply it to the terrain. 
         
        Then we take the value we get from perlin noise and multiply it by the magnitude "mag" 
        because perlin noise returns a value between 0 and 1 and we are going to want noise that 
        creates hills that vary in height by larger sizes like between 0 and 10. Then we take the 
        result and put it to the power of the exponent "exp". This is useful for mountains and things. 
        Lastly we convert the float returned into an int.
        
        We'll apply this to the GenTerrain function column by column. By getting a number for 
        perlin noise in the first loop (for each x) and then using that number in the y loop as the height of the terrain:
        */

        return (int) ( Mathf.Pow ( ( Mathf.PerlinNoise( x / scale, y / scale ) * mag ), ( exp ) ) );
    }
    #endregion

}