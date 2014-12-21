using UnityEngine;
using System.Collections;

public class script_ModifyTerrain : MonoBehaviour 
{
    script_World    world;
    GameObject      cameraGO;

    void Start()
    {
        world       = gameObject.GetComponent<script_World>() as script_World;
        cameraGO    = GameObject.FindGameObjectWithTag( "MainCamera" );
    }

    void Update()
    {
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            ReplaceBlockCursor( 0 );
        }

        if ( Input.GetMouseButtonDown( 1 ) )
        {
            AddBlockCursor( 1 );
        }
    }

    public void ReplaceBlockCenter( float range, byte block )                                                   //Replaces the block directly in front of the player
    {
        Ray ray = new Ray( cameraGO.transform.position, cameraGO.transform.forward );
        RaycastHit hit;

        if ( Physics.Raycast( ray, out hit ) )
        {
            if ( hit.distance < range )
            {
                ReplaceBlockAt( hit, block );
            }
        }
    }

    public void AddBlockCenter( float range, byte block )                                                       //Adds the block specified directly in front of the player
    {
        Ray ray = new Ray( cameraGO.transform.position, cameraGO.transform.forward );
        RaycastHit hit;

        if ( Physics.Raycast( ray, out hit ) )
        {
            if ( hit.distance < range )
            {
                AddBlockAt( hit, block );
            }
            Debug.DrawLine( ray.origin, ray.origin + ( ray.direction * hit.distance ), Color.green, 2 );
        }
    }

    public void ReplaceBlockCursor( byte block )                                                                //Replaces the block specified where the mouse cursor is pointing
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;

        if ( Physics.Raycast( ray, out hit ) )
        {
            ReplaceBlockAt( hit, block );
            Debug.DrawLine( ray.origin, ray.origin + ( ray.direction * hit.distance ), Color.green, 2 );
        }
    }

    public void AddBlockCursor( byte block )                                                                    //Adds the block specified where the mouse cursor is pointing
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        
        if ( Physics.Raycast( ray, out hit ) )
        {
            Debug.Log( "HELLO!" );
            AddBlockAt( hit, block );
            Debug.DrawLine( ray.origin, ray.origin + ( ray.direction * hit.distance ), Color.green, 2 );
        }
    }

    public void ReplaceBlockAt( RaycastHit hit, byte block )                                                    //removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
    {
        Vector3 position = hit.point;
        position += ( hit.normal * -0.5f );

        SetBlockAt( position, block );
    }

    public void AddBlockAt( RaycastHit hit, byte block )                                                        //adds the specified block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
    {
        Vector3 position = hit.point;
        position += ( hit.normal * 0.5f );

        SetBlockAt( position, block );
    }

    public void SetBlockAt( Vector3 position, byte block )                                                      //sets the specified block at these coordinates
    {    
        int x= Mathf.RoundToInt( position.x );
        int y= Mathf.RoundToInt( position.y );
        int z= Mathf.RoundToInt( position.z );

        SetBlockAt( x, y, block );
    }

    public void SetBlockAt( int x, int y, byte block )                                                          //adds the specified block at these coordinates
    {    
        print( "Adding: " + x + ", " + y );

        world.data[ x, y ] = block;
        UpdateChunkAt( x, y );
    }

    public void UpdateChunkAt( int x, int y )                                                                   //Updates the chunk containing this block
    {     
        int updateX= Mathf.FloorToInt( x / world.chunkSize);
        int updateY= Mathf.FloorToInt( y / world.chunkSize);

        print( "Updating: \" + updateX + \", \" + updateY" );

        if ( x - ( world.chunkSize * updateX ) == 0 && updateX != 0 )
        {
            world.chunks[ updateX - 1, updateY ].update = true;
        }

        if ( x - ( world.chunkSize * updateX ) == 15 && updateX != world.chunks.GetLength( 0 ) - 1 )
        {
            world.chunks[ updateX + 1, updateY ].update = true;
        }

        if ( y - ( world.chunkSize * updateY ) == 0 && updateY != 0 )
        {
            world.chunks[ updateX, updateY - 1 ].update = true;
        }

        if ( y - ( world.chunkSize * updateY ) == 15 && updateY != world.chunks.GetLength( 1 ) - 1 )
        {
            world.chunks[ updateX, updateY + 1 ].update = true;
        }
    }
}
