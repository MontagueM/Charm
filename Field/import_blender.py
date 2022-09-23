import logging

import bpy
import os
from pathlib import Path

ASSET_HASH = "<<REPLACE_HASH>>"

ROOT_DIR = Path(bpy.context.space_data.text.filepath).parent
TEXTURE_DIR = ROOT_DIR / "Textures"
MATERIAL_DIR = ROOT_DIR / "Materials" / "Blender"

def load_textures():
    for file in TEXTURE_DIR.iterdir():
        if file.is_file():
            bpy.data.images.load(str(file), check_existing=True)


def load_fbx():
    bpy.ops.import_scene.fbx(filepath=str(ROOT_DIR / f"{ASSET_HASH}.fbx"))


def load_materials():
    for files in os.walk(MATERIAL_DIR):
        for file in files:
            if file.endswith(".py"):
                import file as f
                f.start_import()


def main():
    load_textures()
    load_fbx()


if __name__ == "__main__":
    main()
