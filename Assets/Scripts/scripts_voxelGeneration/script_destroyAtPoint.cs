using UnityEngine;
using System.Collections;

public class script_destroyAtPoint : MonoBehaviour 
{
    public      GameObject                  terrain;
    //private     script_PolygonGenerator     tScript;
    private     script_World                world; 
    public      int                         size        =   4;
    public      bool                        circular    =   false;


	
	void Update () 
    {
        //tScript = GameObject.FindGameObjectWithTag( "PolygonGenerator" ).gameObject.GetComponent( "script_PolygonGenerator" ) as script_PolygonGenerator;

        world = GameObject.FindGameObjectWithTag( "World" ).gameObject.GetComponent<script_World>() as script_World;

        bool collision = false;

        for ( int x=0; x < size; x++ )
        {
            for ( int y=0; y < size; y++ )
            {
                if ( circular )
                {
                    if ( Vector2.Distance( new Vector2( x - ( size / 2 ), y - ( size / 2 ) ), Vector2.zero ) <= ( size / 3 ) )
                    {
                        if ( RemoveBlock( x - ( size / 2 ), y - ( size / 2 ) ) )
                        {
                            collision = true;
                        }

                    }
                }
                else
                {
                    if ( RemoveBlock( x - ( size / 2 ), y - ( size / 2 ) ) )
                    {
                        collision = true;
                    }
                }

            }
        }
        if ( collision )
        {
            //tScript.update = true;
        }
	}

    bool RemoveBlock( float offsetX, float offsetY )
    {
        int x = Mathf.RoundToInt( transform.position.x + offsetX        );
        int y = Mathf.RoundToInt( transform.position.y + 1.0f + offsetY );

        //if ( x < tScript.blocks.GetLength( 0 ) && y < tScript.blocks.GetLength( 1 ) && x >= 0 && y >= 0 )
        if ( x < world.chunks.GetLength( 0 ) && y < world.chunks.GetLength( 1 ) && x >= 0 && y >= 0 )
        {
            //if ( tScript.blocks[ x, y ] != 0 )
            if ( world.Block( x, y ) != 0 )
            {
                //tScript.blocks [ x, y ] = 0;
                return true;
            }
        }
        return false;
    }

}
