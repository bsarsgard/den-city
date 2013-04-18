using UnityEngine;
using System.Collections;

public class TerrainGeneratorBehaviorScript : MonoBehaviour {
	
	const int BOARD_HIGH = 100;
	const int BOARD_WIDE = 100;
	
	const float NOISE_SCALE = 4.0f;
	
	const int HEIGHT_LEVELS = 12;
	const int GROUND_HEIGHT = 5;
	const int WATER_HEIGHT = 1;
	
	private float[,] heightMap;
	
	// total height and width of terrain
	private float terrain_high;
	private float terrain_wide;
	
	// resolution of th efull heightmap
	private int heightmap_high;
	private int heightmap_wide;
	
	// tile height and width in resolution
	private float heightmap_tile_high;
	private float heightmap_tile_wide;
	
	// tile heiht and width in game scale
	private float tile_high;
	private float tile_wide;
	
	public float[,] HeightMap {
		get {
			return this.heightMap;
		}
	}
	
	public float TileHigh {
		get {
			return this.tile_high;
		}
	}
	
	public float TileWide {
		get {
			return this.tile_wide;
		}
	}
	
	// Generate terrain on awake
	public void Start () {
		Terrain terrain = Terrain.activeTerrain;
		
		terrain_high = terrain.terrainData.size.z;
		terrain_wide = terrain.terrainData.size.x;
		
		heightmap_high = terrain.terrainData.heightmapHeight;
		heightmap_wide = terrain.terrainData.heightmapWidth;
		
		heightmap_tile_high = (float)heightmap_high / (float)BOARD_HIGH;
		heightmap_tile_wide = (float)heightmap_wide / (float)BOARD_WIDE;
		
		tile_high = terrain_high / (float)BOARD_HIGH;
		tile_wide = terrain_wide / (float)BOARD_WIDE;
		
		// round down to get even tiles
		//heightmap_high = heightmap_tile_high * BOARD_HIGH;
		//heightmap_wide = heightmap_tile_wide * BOARD_WIDE;
		
		//waterPrefab.transform.localScale = new Vector3((float)heightmap_tile_wide / 10f, 1, (float)heightmap_tile_high / 10f);
		
		int[,] baseHeightMap = new int[BOARD_WIDE, BOARD_HIGH];
		heightMap = new float[terrain.terrainData.heightmapHeight, terrain.terrainData.heightmapWidth];
		
		// get a random seed for the perlin noise
		float rand_x = Random.Range(0, 100);
		float rand_y = Random.Range(0, 100);
		
		// first, we generate the game board
    	for (int yy = 0; yy < BOARD_HIGH; yy++) {
      		for (int xx = 0; xx < BOARD_WIDE; xx++) {
				float noise_x = (float)xx / (float)BOARD_WIDE * NOISE_SCALE;
				float noise_y = (float)yy / (float)BOARD_HIGH * NOISE_SCALE;
				noise_x = Mathf.Round(noise_x * 10f) / 10f;
				noise_y = Mathf.Round(noise_y * 10f) / 10f;
				noise_x += rand_x;
				noise_y += rand_y;
				float sample = Mathf.PerlinNoise(noise_x, noise_y);
				// neet stepped terrain
				int height = Mathf.FloorToInt(sample * (float)HEIGHT_LEVELS); // height is now a stepped int
				if (height <= WATER_HEIGHT) {
					height = 0;
				} else if (height <= GROUND_HEIGHT) {
					height = 1;
				} else {
					height -= GROUND_HEIGHT;
				}
				baseHeightMap[yy, xx] = height;
      		}
    	}
		
		for (int yy = 0; yy < heightmap_high; yy++) {
			for (int xx = 0; xx < heightmap_wide; xx++) {
				// get the base height
				int base_x = Mathf.FloorToInt(((float)xx / (float)heightmap_wide) * (float)BOARD_WIDE);
				int base_y = Mathf.FloorToInt(((float)yy / (float)heightmap_high) * (float)BOARD_HIGH);
				float height = (float)baseHeightMap[base_y, base_x];
				// slope it to create angled tiles
				float slope = 0;
				float percentTraveledX = (float)(xx % heightmap_tile_wide) / (float)heightmap_tile_wide;
				float percentTraveledY = (float)(yy % heightmap_tile_high) / (float)heightmap_tile_high;
				if (base_x > 0) {
					// check slope from the left
					slope = Mathf.Max(slope, height - baseHeightMap[base_y, base_x - 1] - percentTraveledX);
				}
				if (base_x < BOARD_WIDE - 1) {
					// check slope from the right
					slope = Mathf.Max(slope, height - baseHeightMap[base_y, base_x + 1] - (1f - percentTraveledX));
				}
				if (base_y > 0) {
					// check slope from the bottom
					slope = Mathf.Max(slope, height - baseHeightMap[base_y - 1, base_x] - percentTraveledY);
				}
				if (base_y < BOARD_HIGH - 1) {
					// check slope from the top
					slope = Mathf.Max(slope, height - baseHeightMap[base_y + 1, base_x] - (1f - percentTraveledY));
				}
				if (base_x > 0 && base_y > 0) {
					// check slope from the left bottom
					slope = Mathf.Max(slope, height - baseHeightMap[base_y - 1, base_x - 1] - Mathf.Max (percentTraveledX, percentTraveledY));
				}
				if (base_x < BOARD_WIDE - 1 && base_y < BOARD_HIGH - 1) {
					// check slope from the top right
					slope = Mathf.Max(slope, height - baseHeightMap[base_y + 1, base_x + 1] - Mathf.Max(1f - percentTraveledX, 1f - percentTraveledY));
				}
				if (base_x > 0 && base_y < BOARD_HIGH - 1) {
					slope = Mathf.Max(slope, height - baseHeightMap[base_y + 1, base_x - 1] - Mathf.Max (percentTraveledX, 1f - percentTraveledY));
				}
				if (base_x < BOARD_WIDE - 1 && base_y > 0) {
					slope = Mathf.Max(slope, height - baseHeightMap[base_y - 1, base_x + 1] - Mathf.Max (1f - percentTraveledX, percentTraveledY));
				}
				height -= slope;
				heightMap[yy, xx] = height / (BOARD_HIGH / 2);
			}
		}
		
		// add a test building
		//buildings.Add(Instantiate(residentialCubePrefab, new Vector3(heightmap_tile_wide * 10, 1, heightmap_tile_high * 10), Quaternion.identity));
		
		SplatPrototype[] terrainTexture = new SplatPrototype[1];
		terrainTexture[0] = new SplatPrototype();
		terrainTexture[0].texture = (Texture2D)Resources.Load("Standard Assets/Terrain Assets/Terrain Textures/Grass (Hill)");
		terrain.terrainData.splatPrototypes = terrainTexture;
		
		terrain.terrainData.SetHeights(0, 0, heightMap);
		
		terrain.Flush();
	}
}
