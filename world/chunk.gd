extends Node3D


const CHUNK_SIZE = 16
const CHUNK_HEIGHT = 32
const BLOCK_SIZE = 1.0

const FACE_DIRECTIONS = {
	Vector3.UP: {"normal": Vector3.UP, "verts": [
		Vector3(0, 1, 0), Vector3(1, 1, 0), Vector3(1, 1, 1), Vector3(0, 1, 1)
	]},
	Vector3.DOWN: {"normal": Vector3.DOWN, "verts": [
		Vector3(0, 0, 1), Vector3(1, 0, 1), Vector3(1, 0, 0), Vector3(0, 0, 0)
	]},
	Vector3.LEFT: {"normal": Vector3.LEFT, "verts": [
		Vector3(0, 0, 0), Vector3(0, 0, 1), Vector3(0, 1, 1), Vector3(0, 1, 0)
	]},
	Vector3.RIGHT: {"normal": Vector3.RIGHT, "verts": [
		Vector3(1, 0, 1), Vector3(1, 0, 0), Vector3(1, 1, 0), Vector3(1, 1, 1)
	]},
	Vector3.FORWARD: {"normal": Vector3.FORWARD, "verts": [
		Vector3(1, 0, 0), Vector3(0, 0, 0), Vector3(0, 1, 0), Vector3(1, 1, 0)
	]},
	Vector3.BACK: {"normal": Vector3.BACK, "verts": [
		Vector3(0, 0, 1), Vector3(1, 0, 1), Vector3(1, 1, 1), Vector3(0, 1, 1)
	]}
}

func generate(chunk_coords: Vector2i, noise: FastNoiseLite):
	const CHUNK_HEIGHT = 32
	const BLOCK_SIZE = 1.0
	
	var mesh := ArrayMesh.new()
	var arrays := []
	var verts := PackedVector3Array()
	var normals := PackedVector3Array()
	var indices := PackedInt32Array()

	var block_data := {}
	var max_y = 0

	for x in CHUNK_SIZE:
		for z in CHUNK_SIZE:
			var world_x = chunk_coords.x * CHUNK_SIZE + x
			var world_z = chunk_coords.y * CHUNK_SIZE + z
			var height = int(noise.get_noise_2d(world_x, world_z) * 8.0) + 8
			max_y = max(max_y, height)

			for y in height:
				block_data[Vector3i(x, y, z)] = true

	var index = 0

	for pos_i in block_data.keys():
		var pos = Vector3(pos_i)
		
		for dir in FACE_DIRECTIONS.keys():
			var neighbor = pos_i + Vector3i(dir)
			if not block_data.has(neighbor):  # face is exposed to air/sky
				var face = FACE_DIRECTIONS[dir]
				for v in face["verts"]:
					verts.append(Vector3((pos + v) * BLOCK_SIZE))
					normals.append(face["normal"])

				indices.append_array([
					index, index + 1, index + 2,
					index, index + 2, index + 3
				])
				index += 4

	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = verts
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_INDEX] = indices

	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)

	var mesh_instance = MeshInstance3D.new()
	mesh_instance.mesh = mesh

	# material
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color("green")
	mat.flags_wireframe = true
	mesh_instance.material_override = mat
	
	add_child(mesh_instance)
	
	# Add collision
	var static_body := StaticBody3D.new()
	var collision_shape := CollisionShape3D.new()
	var shape := mesh.create_trimesh_shape()  # Generate collision from mesh geometry
	collision_shape.shape = shape
	static_body.add_child(collision_shape)
	add_child(static_body)
