using Godot;
using System;
using System.Collections.Generic;

public partial class WorldRenderer : Node3D
{
	private const string WorldFile = "res://world/world.json";
	private const float BlockSize = 1.0f;

	// Block colors (replace later with textures)
	private readonly Dictionary<string, Color?> BlockColors = new()
	{
		{ "grass", new Color(0.2f, 0.8f, 0.2f) },
		{ "dirt", new Color(0.5f, 0.25f, 0.1f) },
		{ "stone", new Color(0.6f, 0.6f, 0.6f) },
		{ "air", null }
	};

	private Godot.Collections.Dictionary<string, Variant> worldData;

	public override void _Ready()
	{
		LoadWorldData();
		RenderVisibleChunks();
	}

	private void LoadWorldData()
	{
		var file = FileAccess.Open(WorldFile, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushError("Could not open world.json");
			return;
		}

		var jsonString = file.GetAsText();
		var json = new Json();
		var parseResult = json.Parse(jsonString);

		if (parseResult != Error.Ok)
		{
			GD.PushError($"Failed to parse JSON: {json.GetErrorMessage()}");
			return;
		}

		worldData = (Godot.Collections.Dictionary<string, Variant>)json.Data;
	}

	private void RenderVisibleChunks()
	{
		if (!worldData.ContainsKey("chunks"))
			return;

		var chunks = (Godot.Collections.Dictionary<string, Variant>)worldData["chunks"];

		foreach (var chunkKey in chunks.Keys)
		{
			var chunk = (Godot.Collections.Dictionary<string, Variant>)chunks[chunkKey];
			var coords = chunkKey.Split(",");
			int chunkX = int.Parse(coords[0]);
			int chunkZ = int.Parse(coords[1]);

			var blocks = (Godot.Collections.Array)chunk["blocks"];
			foreach (Godot.Collections.Array blockEntry in blocks)
			{
				var posArray = (Godot.Collections.Array)blockEntry[0];
				string blockType = (string)blockEntry[1];

				if (!BlockColors.ContainsKey(blockType) || BlockColors[blockType] == null)
					continue;

				float worldX = chunkX * (int)worldData["chunk_size"] + (int)posArray[0];
				float worldY = (int)posArray[1];
				float worldZ = chunkZ * (int)worldData["chunk_size"] + (int)posArray[2];

				SpawnBlock(new Vector3(worldX, worldY, worldZ), BlockColors[blockType] ?? Colors.White);
			}
		}
	}

	private void SpawnBlock(Vector3 worldPos, Color color)
	{
		var meshInstance = new MeshInstance3D();
		var boxMesh = new BoxMesh();
		boxMesh.Size = new Vector3(BlockSize, BlockSize, BlockSize);

		var mat = new StandardMaterial3D();
		mat.AlbedoColor = color;
		boxMesh.Material = mat;

		meshInstance.Mesh = boxMesh;
		meshInstance.Position = worldPos;

		AddChild(meshInstance);
	}
}
