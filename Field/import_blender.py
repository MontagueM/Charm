import bpy
import os

TEXTURE_PATH = os.path.abspath(bpy.context.space_data.text.filepath / "Textures")


def load_textures():
    for files in os.walk(TEXTURE_PATH):
        for file in files:
            bpy.data.images.load(file.path, check_existing=True)


def main():
    load_textures()


if __name__ == "__main__":
    main()
