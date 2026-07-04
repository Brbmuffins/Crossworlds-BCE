import math
import os
import random

import bpy
from mathutils import Euler, Vector


OUTPUT_DIR = os.path.join(
    os.getcwd(), "Assets", "Game", "3d Assets", "Fences", "GenericWoodenFence"
)
OUTPUT_FBX = os.path.join(OUTPUT_DIR, "Generic_Wooden_Fence_5k.fbx")
OUTPUT_BLEND = os.path.join(OUTPUT_DIR, "Generic_Wooden_Fence_5k.blend")


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def make_material(name, color, roughness=0.75):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Roughness"].default_value = roughness
    return mat


def bevelled_cube(name, location, scale, rotation=(0, 0, 0), bevel=0.035, segments=3):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    bevel_mod = obj.modifiers.new("soft_chipped_edges", "BEVEL")
    bevel_mod.width = bevel
    bevel_mod.segments = segments
    bevel_mod.affect = "EDGES"

    weighted = obj.modifiers.new("weighted_wood_normals", "WEIGHTED_NORMAL")
    weighted.keep_sharp = True

    bpy.ops.object.shade_smooth()
    return obj


def add_cut_grooves(parent, x, z, y, width, height, count, mat):
    grooves = []
    for i in range(count):
        offset = (i - (count - 1) * 0.5) * (width / max(1, count - 1)) * 0.72
        groove = bevelled_cube(
            "dark_wood_crack",
            (x + offset + random.uniform(-0.02, 0.02), y, z + random.uniform(-0.03, 0.03)),
            (0.018, 0.018, height * random.uniform(0.35, 0.85)),
            rotation=(0, 0, random.uniform(-0.05, 0.05)),
            bevel=0.006,
            segments=1,
        )
        groove.data.materials.append(mat)
        groove.parent = parent
        grooves.append(groove)
    return grooves


def add_post(index, x, mat_wood, mat_dark):
    height = random.uniform(1.45, 1.85)
    width = random.uniform(0.22, 0.3)
    z = height * 0.5
    rot_z = random.uniform(-0.045, 0.045)

    post = bevelled_cube(
        f"uneven_post_{index:02d}",
        (x, 0, z),
        (width, width * random.uniform(0.85, 1.1), height),
        rotation=(random.uniform(-0.02, 0.02), random.uniform(-0.02, 0.02), rot_z),
        bevel=0.032,
        segments=4,
    )
    post.data.materials.append(mat_wood)

    cap = bevelled_cube(
        f"slanted_post_top_{index:02d}",
        (x + random.uniform(-0.015, 0.015), 0, height + 0.055),
        (width * 1.08, width * 1.02, 0.11),
        rotation=(0, random.uniform(-0.22, 0.22), rot_z),
        bevel=0.025,
        segments=3,
    )
    cap.data.materials.append(mat_wood)
    cap.parent = post

    add_cut_grooves(post, x, height * 0.48, -width * 0.52, width, height, 5, mat_dark)
    return post


def add_rail(name, z, y, mat_wood, mat_dark):
    rail = bevelled_cube(
        name,
        (0, y, z),
        (6.35, 0.18, 0.23),
        rotation=(0, 0, random.uniform(-0.025, 0.025)),
        bevel=0.035,
        segments=4,
    )
    rail.data.materials.append(mat_wood)

    for i in range(14):
        x = -2.9 + i * 0.45 + random.uniform(-0.05, 0.05)
        groove = bevelled_cube(
            f"{name}_grain_{i:02d}",
            (x, y - 0.1, z + random.uniform(-0.055, 0.055)),
            (random.uniform(0.11, 0.28), 0.018, 0.018),
            rotation=(0, 0, random.uniform(-0.12, 0.12)),
            bevel=0.005,
            segments=1,
        )
        groove.data.materials.append(mat_dark)
        groove.parent = rail

    return rail


def add_rope_wrap(name, x, z, mat):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=0.18,
        minor_radius=0.018,
        major_segments=24,
        minor_segments=6,
        location=(x, -0.005, z),
        rotation=(math.pi / 2, 0, 0),
    )
    rope = bpy.context.object
    rope.name = name
    rope.scale.x = 1.0
    rope.scale.y = 0.75
    rope.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return rope


def build_fence():
    random.seed(42)
    clear_scene()

    mat_wood = make_material("weathered_cyan_safe_brown_wood", (0.42, 0.24, 0.12, 1), 0.9)
    mat_dark = make_material("dark_recessed_wood_cracks", (0.09, 0.045, 0.025, 1), 0.95)
    mat_rope = make_material("dry_rope_bindings", (0.5, 0.42, 0.28, 1), 0.85)

    root = bpy.data.objects.new("Generic_Wooden_Fence_5k", None)
    bpy.context.collection.objects.link(root)

    post_positions = [-3.0, -2.0, -1.02, 0.0, 1.05, 2.02, 3.0]
    for idx, x in enumerate(post_positions):
        obj = add_post(idx, x + random.uniform(-0.04, 0.04), mat_wood, mat_dark)
        obj.parent = root

    for obj in (
        add_rail("upper_weathered_rail", 1.08, -0.13, mat_wood, mat_dark),
        add_rail("lower_weathered_rail", 0.58, -0.14, mat_wood, mat_dark),
    ):
        obj.parent = root

    for idx, x in enumerate(post_positions[1:-1]):
        for z in (0.58, 1.08):
            rope = add_rope_wrap(f"rope_lashing_{idx}_{int(z * 100)}", x, z, mat_rope)
            rope.parent = root

    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    anchor = bpy.context.object
    anchor.name = "Fence_Placement_Origin"
    anchor.parent = root

    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        if obj.type == "MESH":
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            for modifier in obj.modifiers:
                try:
                    bpy.ops.object.modifier_apply(modifier=modifier.name)
                except Exception:
                    pass

    mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    faces = sum(len(obj.data.polygons) for obj in mesh_objects)
    verts = sum(len(obj.data.vertices) for obj in mesh_objects)
    print(f"Generated fence mesh objects: {len(mesh_objects)}")
    print(f"Approx polygons/faces: {faces}")
    print(f"Vertices: {verts}")

    os.makedirs(OUTPUT_DIR, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=OUTPUT_BLEND)

    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in mesh_objects:
        obj.select_set(True)

    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=True,
        apply_unit_scale=True,
        bake_space_transform=False,
        object_types={"EMPTY", "MESH"},
        mesh_smooth_type="FACE",
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        path_mode="AUTO",
    )
    print(f"Saved FBX: {OUTPUT_FBX}")


if __name__ == "__main__":
    build_fence()
