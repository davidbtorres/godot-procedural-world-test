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

		// Create one chunk
		var blocks = new List<object>();
		for (int i = 0; i < 20; i++)
		{
			blocks.Add(new object[] { new int[] { i, 0, 0 }, "grass" });
		}

		var chunk = new Dictionary<string, object>
		{
			{ "blocks", blocks }
		};

		((Dictionary<string, object>)data["chunks"])["0,0"] = chunk;

		// Convert to JSON 
		string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

		// Save to file
		using var file = FileAccess.Open(WorldFile, FileAccess.ModeFlags.Write);
		file.StoreString(json);

		GD.Print("World blueprint saved to: ", WorldFile);
	}
}
