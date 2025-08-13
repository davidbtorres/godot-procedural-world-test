# world_renderer.gd
extends Node3D

const WORLD_FILE := "res://world/world.json"

# Block size in world units
const BLOCK_SIZE := 1.0

# Basic colors for block types (replace later with textures)
var BLOCK_COLORS := {
	"grass": Color(0.2, 0.8, 0.2),
	"dirt": Color(0.5, 0.25, 0.1),
	"stone": Color(0.6, 0.6, 0.6),
	"air": null # null = don't render
}

var world_data := {}

func _ready():
	load_world_data()
	render_visible_chunks()

func load_world_data():
	var file := FileAccess.open(WORLD_FILE, FileAccess.READ)
	if not file:
		push_error("Could not open world.json")
		return
	var json_string := file.get_as_text()
	var json := JSON.new()
	var parse_result := json.parse(json_string)
	if parse_result != OK:
		push_error("Failed to parse JSON: %s" % json.get_error_message())
		return
	world_data = json.get_data()

func render_visible_chunks():
	for chunk_key in world_data["chunks"].keys():
		var chunk = world_data["chunks"][chunk_key]
		var coords = chunk_key.split(",") # ["0", "0"]
		var chunk_x = int(coords[0])
		var chunk_z = int(coords[1])

		for block_entry in chunk["blocks"]:
			var pos_array = block_entry[0] # e.g. [0, 1, 0]
			var block_type = block_entry[1]

			if not BLOCK_COLORS.has(block_type) or BLOCK_COLORS[block_type] == null:
				continue # skip air or undefined

			var world_x = chunk_x * world_data["chunk_size"] + pos_array[0]
			var world_y = pos_array[1]
			var world_z = chunk_z * world_data["chunk_size"] + pos_array[2]

			spawn_block(Vector3(world_x, world_y, world_z), BLOCK_COLORS[block_type])

func spawn_block(world_pos: Vector3, color: Color):
	var mesh_instance := MeshInstance3D.new()
	mesh_instance.mesh = BoxMesh.new()
	mesh_instance.mesh.size = Vector3(BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mesh_instance.mesh.material = mat
	mesh_instance.position = world_pos
	add_child(mesh_instance)
