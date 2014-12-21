using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class script_World : MonoBehaviour
{
    #region Two ways of doing this
    /*
    This is an interesting point because there are two ways to do this, 
    you can store the information in each chunk so that every chunk has 
    the data for the blocks it contains or you can use a big array for 
    all the level data that each chunk refers to. I think we'll use the 
    big array because it's easier later on. This will mean that our world 
    has a fixed size though but making an infinitely generating world will 
    have to remain outside of the scope of this tutorial.
    */
    #endregion

    public GameObject       chunk;
    public script_Chunk[,]  chunks;

    public int              chunkSize       =   16;

    public byte[,]          data;
    public int              worldX          =   16;
    public int              worldY          =   16;

    void Start () 
    {
        GenWorld();

        StartCoroutine( GenChunks() );
    }

    #region variables
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
    #endregion

    void GenWorld()                                                                                               // This makes blocks a 10x10 array then goes through each block making any block with a y less that 5 into rock and the row at 5 into grass.
    {
        data = new byte[ worldX, worldY ];

        for ( int px = 0; px < worldX; px++ )
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

            int noise_dirt   = Noise( px, 0, dirt_01_scale, dirt_01_mag, dirt_01_exp );                                   //  [Layer 1] 
            noise_dirt += Noise( px, 0, dirt_02_scale, dirt_02_mag, dirt_02_exp );                                   //  [Layer 2]
            noise_dirt += dirt_height;                                                                               // Lastly we add 75 to the stone to raise it up.

            int noise_grass  = Noise( px, 0, dirt_01_scale, dirt_01_mag, dirt_01_exp );
            noise_grass += Noise( px, 0, dirt_02_scale + grass_scale, dirt_02_mag + grass_mag, dirt_02_exp );
            noise_grass += dirt_height;

            for ( int py = 0; py < worldY; py++ )                                                    // Loop will make stone a certain height
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

                if ( py < noise_dirt )
                {
                    data[ px, py ] = 2;       // block = Dirt

                    if ( Noise( px, py, spot_stoneScale, spot_stoneMag, spot_stoneExp ) > spotSize )
                    {
                        data[ px, py ] = 1;   // block = Stone
                    }
                    if ( Noise( px, py, spot_sandScale, spot_sandMag, spot_sandExp ) > spotSize )
                    {
                        data[ px, py ] = 4;   // block = sand
                    }
                    if ( Noise( px, py * spotWidth, spot_caveScale, spot_caveMag, spot_caveExp ) > spotSize )       // The next three lines remove dirt and rock to make caves in certain places
                    {
                        data[ px, py ] = 0;   // block = air
                    }
                }
                else if ( py < noise_grass )
                {
                    data[ px, py ] = 3;       // block = Grass
                }
            }
        }
    }


    IEnumerator GenChunks()
    {
        chunks = new script_Chunk[ Mathf.FloorToInt( worldX / chunkSize ), Mathf.FloorToInt( worldY / chunkSize ) ];

        for ( int x = 0; x < chunks.GetLength( 0 ); x++ )
        {
            for ( int y = 0; y < chunks.GetLength( 1 ); y++ )
            {
                yield return new WaitForSeconds( 0.0001f );

                GameObject newChunk         = Instantiate( chunk, new Vector3( x * chunkSize - 0.5f, y * chunkSize + 0.5f ), new Quaternion( 0, 0, 0, 0 ) ) as GameObject;

                newChunk.transform.parent   = transform;

                chunks[ x, y ]              = newChunk.GetComponent<script_Chunk>() as script_Chunk;
                chunks[ x, y ].worldGO      = gameObject;
                chunks[ x, y ].chunkSize    = chunkSize;
                chunks[ x, y ].chunkX       = x * chunkSize;
                chunks[ x, y ].chunkY       = y * chunkSize;
            }
        }
    }


    int Noise( int x, int y, float scale, float mag, float exp )
    {
        return (int)( Mathf.Pow( ( Mathf.PerlinNoise( x / scale, y / scale ) * mag ), ( exp ) ) );
    }

    
    public byte Block( int x, int y )
    {
        if ( x >= worldX || x < 0 || y >= worldY || y < 0 )
        {
            return (byte) 2;
        }

        return data[ x, y ];
    }
    

}
