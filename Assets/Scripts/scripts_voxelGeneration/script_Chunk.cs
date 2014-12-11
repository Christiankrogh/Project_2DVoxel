using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 

public class script_Chunk : MonoBehaviour 
{
    public GameObject       worldGO;
    private script_World    world;

    public  int             chunkSize;
    private List<Vector3>   newVertices     = new List<Vector3>();
    private List<int>       newTriangles    = new List<int>();
    private List<Vector2>   newUV           = new List<Vector2>();
    private Mesh            mesh;
    public  List<Vector3>   colVertices     = new List<Vector3>();
    public  List<int>       colTriangles    = new List<int>();
    private int             colCount;
    private MeshCollider    col;
    private int             squareCount;  
    public int              chunkX;
    public int              chunkY;
    public int              chunkZ;

    private float           tUnit           = 0.015625f;                          // tUnit is the fraction of space 1 tile takes up out of the width of the texture. ## In this case it's 1/4 or 0.25 ## 
                                                                                  // Texture 2048x2048, tUnit = 0.125f;, Texture 4096x4096, tUnity = 0.015625f;
    public const byte       air             = 0;
    public const byte       stone           = 1;
    public const byte       dirt            = 2;
    public const byte       grass           = 3;
    public const byte       sand            = 4; 

	
    void Start () 
    {
        world       = worldGO.GetComponent<script_World>();
        mesh        = GetComponent<MeshFilter>().mesh;
        col         = GetComponent<MeshCollider>();
        chunkSize   = world.chunkSize;

        BuildMesh();   
	}


    void BuildMesh()    
    {
        for ( int x = 0; x < chunkSize; x++ )
        {
            for ( int y = 0; y < chunkSize; y++ )
            {
                if ( Block( x, y ) != air )                                        // If the block is not air
                {
                    GenCollider( x, y );

                    GenerateBlocks( x, y );
                }
            }
        }
        UpdateMesh();
    }

    public byte Block( int x, int y )
    {
        return world.Block( x + chunkX, y + chunkY );
    }
    
    void UpdateMesh()
    {
        mesh.Clear();                                                               // Then last of all we clear anything in the mesh to begin with, we set the mesh's vertices to ours
        mesh.vertices       = newVertices.ToArray();                                // ... (But we have to convert the list to an array with the .ToArray() command) and set the mesh's triangles to ours.
        mesh.triangles      = newTriangles.ToArray();
        mesh.uv             = newUV.ToArray();
        mesh.Optimize();                                                            // Unity does some work for us when we call the optimize command (This usually does nothing but it doesn't use any extra time so don't worry)
        mesh.RecalculateNormals();                                                  // The recalculate normals command so that the normals are generated automatically.

        //col.sharedMesh      = null;
        //col.sharedMesh      = mesh;

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

    void GenCollider( int x, int y )
    {
        #region block_top
        if ( Block( x, y + 1 ) == 0 )
        {
            colVertices.Add( new Vector3( x, y, 1 ) );
            colVertices.Add( new Vector3( x + 1, y, 1 ) );
            colVertices.Add( new Vector3( x + 1, y, 0 ) );
            colVertices.Add( new Vector3( x, y, 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_bot
        if ( Block( x, y - 1 ) == 0 )
        {
            colVertices.Add( new Vector3( x, y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 1 ) );
            colVertices.Add( new Vector3( x, y - 1, 1 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_left
        if ( Block( x - 1, y ) == 0 )
        {
            colVertices.Add( new Vector3( x, y - 1, 1 ) );
            colVertices.Add( new Vector3( x, y, 1 ) );
            colVertices.Add( new Vector3( x, y, 0 ) );
            colVertices.Add( new Vector3( x, y - 1, 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion

        #region block_right
        if ( Block( x + 1, y ) == 0 )
        {
            colVertices.Add( new Vector3( x + 1, y, 1 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 1 ) );
            colVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
            colVertices.Add( new Vector3( x + 1, y, 0 ) );

            ColliderTriangles();
            colCount++;
        }
        #endregion
    }

    void ColliderTriangles()
    {
        colTriangles.Add(   colCount * 4 );
        colTriangles.Add( ( colCount * 4 ) + 1 );
        colTriangles.Add( ( colCount * 4 ) + 3 );
        colTriangles.Add( ( colCount * 4 ) + 1 );
        colTriangles.Add( ( colCount * 4 ) + 2 );
        colTriangles.Add( ( colCount * 4 ) + 3 );
    }
    
    void GenerateBlocks( int x, int y )
    {
        switch ( Block( x, y ) )
        {
            case stone:
                GenBlockBorder( x, y, air, 1, 0 );
                break;

            case dirt:
                GenBlockBorder( x, y, air, 0, 0 );
                break;

            case grass:
                GenBlockBorder( x, y, air, 0, 0 );
                break;

            case sand:
                GenBlockBorder( x, y, air, 3, 0 );
                break;

            default:
                //Debug.Log("No block of that type...");
                break;
        }
    }
    
    void GenBlockBorder( int x, int y, byte blockType, float tOffsetX, float tOffsetY )
    {
        #region Texture Coordinates  
        Vector2       tMid_fill_01          = new Vector2( 3, 4 );
        Vector2       tMid_fill_02          = new Vector2( 4, 4 );
        Vector2       tMid_fill_03          = new Vector2( 3, 3 );
        Vector2       tMid_fill_04          = new Vector2( 4, 3 );

        Vector2       tSide_Top             = new Vector2( 3, 5 );
        Vector2       tSide_Bot             = new Vector2( 3, 2 );
        Vector2       tSide_left            = new Vector2( 2, 3 );
        Vector2       tSide_right           = new Vector2( 5, 3 );

        Vector2       tBigCorner_01         = new Vector2( 2, 2 );
        Vector2       tBigCorner_02         = new Vector2( 5, 2 );
        Vector2       tBigCorner_03         = new Vector2( 2, 5 );
        Vector2       tBigCorner_04         = new Vector2( 5, 5 );

        Vector2       tSmallCorner_01       = new Vector2( 1, 1 );
        Vector2       tSmallCorner_02       = new Vector2( 6, 1 );
        Vector2       tSmallCorner_03       = new Vector2( 1, 6 );
        Vector2       tSmallCorner_04       = new Vector2( 6, 6 );

        Vector2       tHorizontalTube       = new Vector2( 0, 0 );
        Vector2       tVerticalTube         = new Vector2( 0, 3 );
        Vector2       tVerticalTubeTop      = new Vector2( 0, 4 );
        Vector2       tDiagonal_01          = new Vector2( 0, 1 );
        Vector2       tDiagonal_02          = new Vector2( 0, 2 );
        Vector2       tFloatingBlock        = new Vector2( 0, 5 );

        Vector2       tSingleTop_part_01    = new Vector2( 3, 6 );
        Vector2       tSingleTop_part_02    = new Vector2( 4, 6 );

        Vector2       tSingleBOT_part_01    = new Vector2( 3, 1 );
        Vector2       tSingleBOT_part_02    = new Vector2( 4, 1 );

        Vector2       tSingleLeft_part_01   = new Vector2( 1, 3 );
        Vector2       tSingleLeft_part_02   = new Vector2( 1, 4 );

        Vector2       tSingleRight_part_01  = new Vector2( 6, 3 );
        Vector2       tSingleRight_part_02  = new Vector2( 6, 4 );
        #endregion

        #region Positions around the Square
        byte          top                   = Block( x, y + 1 );
        byte          bot                   = Block( x, y - 1 );
        byte          left                  = Block( x - 1, y );
        byte          right                 = Block( x + 1, y );
        byte          top_left              = Block( x - 1, y + 1 );
        byte          top_right             = Block( x + 1, y + 1 );
        byte          bot_left              = Block( x - 1, y - 1 );
        byte          bot_right             = Block( x + 1, y - 1 );
        #endregion

        #region [FILL]  Conditions
        if (      top         != blockType &&
                  bot         != blockType &&
                  left        != blockType &&
                  right       != blockType &&
                  top_left    != blockType &&
                  top_right   != blockType &&
                  bot_left    != blockType &&
                  bot_right   != blockType )
                  {
                      GenSquare( x, y, tMid_fill_01, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [TOP]   Conditions
        else if ( top       == blockType &&
                  bot       != blockType &&
                  left      != blockType &&
                  right     != blockType )
                  {
                      GenSquare( x, y, tSide_Top, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [BOT]   Conditions
        else if ( bot       == blockType &&
                  left      != blockType &&
                  right     != blockType &&
                  top       != blockType )
                  {
                      GenSquare( x, y, tSide_Bot, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [LEFT]  Conditions
        else if ( left      == blockType &&
                  top       != blockType &&
                  bot       != blockType &&
                  right     != blockType &&
                  top_right != blockType &&
                  bot_right != blockType )
                  {
                      GenSquare( x, y, tSide_left, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [RIGHT] Conditions
        else if ( right     == blockType &&
                  top       != blockType &&
                  bot       != blockType &&
                  left      != blockType &&
                  top_left  != blockType )
                  {
                      GenSquare( x, y, tSide_right, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [TOP_left] Conditions
        else if ( left      == blockType && 
                  top       == blockType && 
                  right     != blockType && 
                  bot       != blockType )
                  {
                      GenSquare( x, y, tBigCorner_03, tOffsetX, tOffsetY );
                  }
        else if ( top_left  == blockType && 
                  top_right != blockType && 
                  left      != blockType && 
                  top       != blockType && 
                  bot_left  != blockType && 
                  bot_right != blockType )
                  {
                      GenSquare( x, y, tSmallCorner_03, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [TOP_right] Conditions
        else if ( right     == blockType && 
                  top       == blockType && 
                  left      != blockType && 
                  bot       != blockType )
                  {
                      GenSquare( x, y, tBigCorner_04, tOffsetX, tOffsetY );
                  }
        else if ( top_right == blockType && 
                  top_left  != blockType && 
                  right     != blockType && 
                  top       != blockType && 
                  bot_right != blockType && 
                  bot_left  != blockType )
                  {
                      GenSquare( x, y, tSmallCorner_04, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [BOT_left] Conditions
        else if ( left      == blockType && 
                  bot       == blockType && 
                  top       != blockType && 
                  right     != blockType )
                  {
                      GenSquare( x, y, tBigCorner_01, tOffsetX, tOffsetY );
                  }

        else if ( bot_left  == blockType && 
                  bot_right != blockType && 
                  left      != blockType && 
                  bot       != blockType && 
                  top_left  != blockType && 
                  top_right != blockType )
                  {
                      GenSquare( x, y, tSmallCorner_01, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [BOT_right] Conditions
        else if ( right     == air && 
                  bot       == air && 
                  top       != air && 
                  left      != air )
                  {
                      GenSquare( x, y, tBigCorner_02, tOffsetX, tOffsetY );
                  }
        else if ( bot_right == air && 
                  bot_left  != air && 
                  right     != air && 
                  bot       != air && 
                  top_right != air && 
                  top_left  != air )
                  {
                      GenSquare( x, y, tSmallCorner_02, tOffsetX, tOffsetY );
                  }
        #endregion
        #region [Special] Conditions
        else if ( top_left  == air && 
                  top_right == air && 
                  top       != air && 
                  left      != air && 
                  right     != air )   
                  {
                      GenSquare( x, y, tSingleTop_part_01, tOffsetX, tOffsetY ); 
                  }
        
        else if ( top       == air && 
                  left      == air && 
                  right     == air )
                  {
                      GenSquare( x, y, tSingleTop_part_02, tOffsetX, tOffsetY ); 
                  }

        else if ( bot_left  == air && 
                  bot_right == air && 
                  bot       != air && 
                  left      == air && 
                  right     == air )
                  {
                      GenSquare( x, y, tSingleBOT_part_01, tOffsetX, tOffsetY ); 
                  }

        else if ( bot       == air && 
                  bot_left  == air && 
                  bot_right == air && 
                  left      == air && 
                  right     == air )
                  {
                      GenSquare( x, y, tSingleBOT_part_02, tOffsetX, tOffsetY );
                  }

        else if ( left      == air && 
                  bot       == air && 
                  top       == air )
                  {
                      GenSquare( x, y, tSingleLeft_part_01, tOffsetX, tOffsetY );
                  }

        else if ( left      != air && 
                  bot       != air && 
                  top       != air && 
                  top_left  == air && 
                  bot_left  == air )
                  {
                      GenSquare( x, y, tSingleLeft_part_02, tOffsetX, tOffsetY );
                  }

        else if ( right     == air && 
                  bot       == air && 
                  top       == air )
                  {
                      GenSquare( x, y, tSingleRight_part_01, tOffsetX, tOffsetY );
                  } 

        else if ( right     != air && 
                  bot       != air && 
                  top       != air && 
                  top_right == air && 
                  bot_right == air )
                  {
                      GenSquare( x, y, tSingleRight_part_02, tOffsetX, tOffsetY );
                  }

        else if ( bot_left  == air && 
                  bot_right == air && 
                  left      != air && 
                  right     != air && 
                  bot       != air && 
                  top       != air && 
                  top_left  != air && 
                  top_right != air )
                  {
                      GenSquare( x, y, tVerticalTubeTop, tOffsetX, tOffsetY );
                  }

        else if ( top       == air && 
                  bot       == air && 
                  left      != air && 
                  right     != air )
                  {
                      GenSquare( x, y, tHorizontalTube, tOffsetX, tOffsetY );
                  }

        else if ( right     == air && 
                  top       != air && 
                  bot       != air && 
                  left      == air )
                  {
                      GenSquare( x, y, tVerticalTube, tOffsetX, tOffsetY );
                  }

        else if ( top_left  == air && 
                  top_right != air && 
                  left      != air && 
                  top       != air &&
                  bot_left  != air && 
                  bot_right == air )
                  {
                      GenSquare( x, y, tDiagonal_01, tOffsetX, tOffsetY );
                  }

        else if ( bot_left  == air && 
                  left      != air && 
                  bot       != air && 
                  top_left  != air && 
                  top_right == air && 
                  bot_right != air )
                  {
                      GenSquare( x, y, tDiagonal_02, tOffsetX, tOffsetY );
                  }

        else if ( top       == air && 
                  bot       == air && 
                  left      == air && 
                  right     == air && 
                  top_left  == air && 
                  top_right == air && 
                  bot_left  == air && 
                  bot_right == air )
                  {
                      GenSquare( x, y, tFloatingBlock, tOffsetX, tOffsetY );
                  }
        #endregion 
    }

    void GenSquare( int x, int y, Vector2 texture, float tOffsetX, float tOffsetY ) // Vector2 texture,
    {
        newVertices.Add( new Vector3( x    , y    , 0 ) );                              // These lines defines the corners of a square
        newVertices.Add( new Vector3( x + 1, y    , 0 ) );
        newVertices.Add( new Vector3( x + 1, y - 1, 0 ) );
        newVertices.Add( new Vector3( x    , y - 1, 0 ) );

        newTriangles.Add( squareCount * 4 );                                          // We define the triangles. Because without them all we have is points in space, we have to show how those points are connected and we connect them in triangles.
        newTriangles.Add( ( squareCount * 4 ) + 1 );                                    // What we do is add (squareCount*4) to each number we .Add() to newTriangles. 
        newTriangles.Add( ( squareCount * 4 ) + 3 );
        newTriangles.Add( ( squareCount * 4 ) + 1 );
        newTriangles.Add( ( squareCount * 4 ) + 2 );
        newTriangles.Add( ( squareCount * 4 ) + 3 );

        newUV.Add( new Vector2( ( tOffsetX * 0.125f ) + ( tUnit * texture.x ), ( tOffsetY * 0.125f ) + ( tUnit * texture.y + tUnit ) ) );
        newUV.Add( new Vector2( ( tOffsetX * 0.125f ) + ( tUnit * texture.x + tUnit ), ( tOffsetY * 0.125f ) + ( tUnit * texture.y + tUnit ) ) );
        newUV.Add( new Vector2( ( tOffsetX * 0.125f ) + ( tUnit * texture.x + tUnit ), ( tOffsetY * 0.125f ) + ( tUnit * texture.y ) ) );
        newUV.Add( new Vector2( ( tOffsetX * 0.125f ) + ( tUnit * texture.x ), ( tOffsetY * 0.125f ) + ( tUnit * texture.y ) ) );

        squareCount++; 
    }

       
}
