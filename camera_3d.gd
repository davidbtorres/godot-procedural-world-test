extends Camera3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	look_at(Vector3.ZERO, Vector3.UP)
