extends CharacterBody3D

@export var mouse_sensitivity := 0.002
@export var speed := 5.0
@export var gravity := 9.8
@export var jump_velocity := 5.0

var head
var cam_pitch := 0.0

var flying = true

func _ready():
	head = $Head
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _unhandled_input(event):
	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * mouse_sensitivity)
		cam_pitch = clamp(cam_pitch - event.relative.y * mouse_sensitivity, deg_to_rad(-90), deg_to_rad(90))
		head.rotation.x = cam_pitch
	elif event is InputEventKey and event.pressed:
		if event.keycode == KEY_ESCAPE:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _physics_process(delta):
	var dir = Vector3.ZERO
	var movementBasis = global_transform.basis

	if Input.is_action_pressed("move_forward"):
		dir -= movementBasis.z
	if Input.is_action_pressed("move_back"):
		dir += movementBasis.z
	if Input.is_action_pressed("move_left"):
		dir -= movementBasis.x
	if Input.is_action_pressed("move_right"):
		dir += movementBasis.x
	
	if Input.is_action_just_pressed("fly"):
		flying = !flying
		print(flying)

	dir.y = 0
	dir = dir.normalized()

	velocity.x = dir.x * speed
	velocity.z = dir.z * speed
	
	# Gravity
	if not flying:
		if not is_on_floor():
			velocity.y -= gravity * delta
		else:
			velocity.y = 0
			if Input.is_action_just_pressed("jump"):
				velocity.y = jump_velocity
	else:
		velocity.y = 0
		if Input.is_action_pressed("jump"):
			velocity.y = jump_velocity
		else:
			if Input.is_action_pressed("crouch"):
				velocity.y = -jump_velocity

	move_and_slide()
