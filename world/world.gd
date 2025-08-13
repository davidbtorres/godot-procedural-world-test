extends Node3D

const CHUNK_SIZE = 16
const CHUNK_HEIGHT = 32
const LOAD_RADIUS = 2

@export var chunk_scene: PackedScene

var noise := FastNoiseLite.new()

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	noise.seed = randi()
	noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	noise.frequency = 0.02
	noise.fractal_octaves = 4
	
	for x in range(-LOAD_RADIUS, LOAD_RADIUS + 1):
		for z in range(-LOAD_RADIUS, LOAD_RADIUS + 1):
			load_chunk(Vector2i(x, z))

func load_chunk(chunk_coords: Vector2i):
	var chunk = chunk_scene.instantiate()
	chunk.position = Vector3(chunk_coords.x * CHUNK_SIZE, 0, chunk_coords.y * CHUNK_SIZE)
	chunk.call("generate", chunk_coords, noise)
	add_child(chunk)
