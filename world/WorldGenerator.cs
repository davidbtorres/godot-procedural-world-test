using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class WorldGenerator : Object
{
	private const string WorldFile = "res:///world/world.json";

	// public override void _Ready()
	// {
	// 	GenerateWorld();
	// }

	public static void GenerateWorld()
	{
		var stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		GD.Print("Generating new world blueprint...");
		var data = new Dictionary<string, object>
		{
			{ "seed", 123456 },
			{ "chunk_size", 16 },
			{ "chunks", new Dictionary<string, object>() }
		};

		int seed = (int)data["seed"];
		int chunkSize = (int)data["chunk_size"];
		int worldChunksX = 4;
		int worldChunksZ = 4;

		// Setup noise
		var noise = new FastNoiseLite();
		noise.Seed = seed;
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin; // or OpenSimplex2
		noise.Frequency = 0.05f; // controls how "stretched" terrain is

		var chunksDict = (Dictionary<string, object>)data["chunks"];

		for (int cx = 0; cx < worldChunksX; cx++)
		{
			for (int cz = 0; cz < worldChunksZ; cz++)
			{
				var blocks = new List<object>();

				for (int x = 0; x < chunkSize; x++)
				{
					for (int z = 0; z < chunkSize; z++)
					{
						// World-space coords for this block (global x/z)
						int worldX = cx * chunkSize + x;
						int worldZ = cz * chunkSize + z;

						// Sample noise (-1..1) → scale to desired height
						float noiseValue = noise.GetNoise2D(worldX, worldZ);
						int height = (int)(Mathf.Remap(noiseValue, -1, 1, 2, 12));
						// terrain from y=2 to y=12

						// Add column of blocks up to that height
						for (int y = 0; y <= height; y++)
						{
							string blockType = (y == height) ? "grass" : "dirt";
							blocks.Add(new object[] { new int[] { x, y, z }, blockType });
						}
					}
				}

				var chunk = new Dictionary<string, object>
				{
					{ "blocks", blocks }
				};

				string chunkKey = $"{cx},{cz}";
				chunksDict[chunkKey] = chunk;
			}
		}

		// Convert to JSON 
		string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

		// Save to file
		using var file = FileAccess.Open(WorldFile, FileAccess.ModeFlags.Write);
		file.StoreString(json);

		GD.Print("World blueprint saved to: ", WorldFile);
		stopwatch.Stop();
		GD.Print("World generation took: " + stopwatch.ElapsedMilliseconds + " ms");
		GD.Print(OS.GetStaticMemoryUsage());
		GD.Print(OS.GetStaticMemoryPeakUsage());
	}
}
