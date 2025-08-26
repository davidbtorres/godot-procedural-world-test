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
		GD.Print("Generating new world blueprint...");
		var data = new Dictionary<string, object>
		{
			{ "seed", 123456 },
			{ "chunk_size", 16 },
			{ "chunks", new Dictionary<string, object>() }
		};


		int chunkSize = (int)data["chunk_size"];
		int worldChunksX = 4;
		int worldChunksZ = 4;

		var chunksDict = (Dictionary<string, object>)data["chunks"];

		for (int cx = 0; cx < worldChunksX; cx++)
		{
			for (int cz = 0; cz < worldChunksZ; cz++)
			{
				var blocks = new List<object>();

				// Fill this chunk with grass blocks (flat layer at y = 0)
				for (int x = 0; x < chunkSize; x++)
				{
					for (int z = 0; z < chunkSize; z++)
					{
						blocks.Add(new object[] { new int[] { x, 0, z }, "grass" });
					}
				}

				// Save chunk
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
	}
}
