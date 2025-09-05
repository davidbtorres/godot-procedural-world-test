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

	private readonly Dictionary<string, Vector2I> BlockUVs = new()
{
	{ "grass_top", new Vector2I(0, 0) },
	{ "grass_side", new Vector2I(1, 0) },
	{ "dirt", new Vector2I(2, 0) },
	{ "stone", new Vector2I(3, 0) }
};

	private Godot.Collections.Dictionary<string, Variant> worldData;

	public override void _Ready()
	{
		GD.Print("Calling GenerateWorld script.");
		WorldGenerator.GenerateWorld();
		GD.Print("Loading world data...");
		LoadWorldData();
		GD.Print("World Data successfully loaded.");
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

			// Build one mesh and one collider for this chunk
			BuildChunkMesh(chunkX, chunkZ, blocks);
		}
	}


	private void BuildChunkMesh(int chunkX, int chunkZ, Godot.Collections.Array blocks)
	{
		// Prepare a SurfaceTool for triangle geometry
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		// We'll reuse the cube vertex/index data for every block
		var cube = new BoxMesh { Size = new Vector3(BlockSize, BlockSize, BlockSize) };
		var cubeArraysMesh = new ArrayMesh();
		cubeArraysMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, cube.GetMeshArrays());
		var arrays = cubeArraysMesh.SurfaceGetArrays(0);

		var verts = (Vector3[])arrays[(int)Mesh.ArrayType.Vertex];
		var norms = (Vector3[])arrays[(int)Mesh.ArrayType.Normal];
		var indices = (int[])arrays[(int)Mesh.ArrayType.Index];

		int chunkSize = (int)worldData["chunk_size"];

		foreach (Godot.Collections.Array blockEntry in blocks)
		{
			var posArray = (Godot.Collections.Array)blockEntry[0]; // [x,y,z] local to chunk
			string btype = (string)blockEntry[1];

			if (!BlockColors.ContainsKey(btype) || BlockColors[btype] == null)
				continue; // skip air/undefined

			Color color = BlockColors[btype] ?? Colors.White;

			// Convert chunk-local coords to world coords
			Vector3 worldPos = new Vector3(
				chunkX * chunkSize + (int)posArray[0],
				(int)posArray[1],
				chunkZ * chunkSize + (int)posArray[2]
			);

			// Add cube triangles translated to worldPos
			for (int i = 0; i < indices.Length; i++)
			{
				int vi = indices[i];
				st.SetNormal(norms[vi]);
				st.SetColor(color); // per-vertex color (material must use vertex color)
				st.SetSmoothGroup(UInt32.MaxValue);
				st.AddVertex(verts[vi] + worldPos);
			}

			// Add cube triangles translated to worldPos
			for (int i = 0; i < indices.Length; i++)
			{
				int vi = indices[i];

				st.SetNormal(norms[vi]);

				// Pick UVs from atlas
				Vector2 uvBase = BlockUVs.ContainsKey(btype)
					? BlockUVs[btype]
					: new Vector2I(3, 0); // default stone if missing

				// Scale tile coords into [0,1] UV space
				Vector2 uv = (new Vector2(worldPos.X, worldPos.Z) * 0.1f) + new Vector2(0.5f, 0.5f);

				// ⚠️ This is a stub – next step is face-specific UV mapping

				st.SetUV(uv);
				st.SetSmoothGroup(UInt32.MaxValue);
				st.AddVertex(verts[vi] + worldPos);
			}

		}


		// Finish and get the mesh
		st.GenerateNormals();
		var chunkMesh = st.Commit();

		// Create a container node for this chunk (keeps mesh & collider aligned)
		var chunkNode = new Node3D();
		AddChild(chunkNode);

		// Visual
		var mi = new MeshInstance3D { Mesh = chunkMesh };
		// Use vertex colors as albedo so SetColor() shows up
		var mat = new StandardMaterial3D { VertexColorUseAsAlbedo = true };
		mi.MaterialOverride = mat;
		chunkNode.AddChild(mi);

		// Collider (concave, matches triangle mesh)
		var body = new StaticBody3D();
		var shape = new CollisionShape3D();
		var concave = new ConcavePolygonShape3D { Data = chunkMesh.GetFaces() };
		shape.Shape = concave;
		body.AddChild(shape);
		chunkNode.AddChild(body);

		var meshInstance = new MeshInstance3D();
		//meshInstance.Mesh = arrayMesh;

		// Load the material once (static/shared material is more efficient)
		var material = ResourceLoader.Load<StandardMaterial3D>("res://materials/blockatlas.tres");
		meshInstance.MaterialOverride = material;

		AddChild(meshInstance);
	}
}
