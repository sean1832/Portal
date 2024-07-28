import mmap
import threading
import time

import bpy


# Main logic
def handle_data(data):
    print(data)


class PeriodicFileReader(threading.Thread):
    def __init__(self, mmf_name, interval, buffer_size=1024):
        super().__init__()
        self.mmf_name = mmf_name
        self.interval = interval
        self.running = False
        self.mmf_size = buffer_size * 1024  # Buffer size

    def run(self):
        print(f"Reading from {self.mmf_name} every {self.interval} seconds.")
        with mmap.mmap(-1, self.mmf_size, tagname=self.mmf_name) as mm:
            while self.running:
                mm.seek(0)
                if mm.size() >= 4:
                    length_prefix = mm.read(4)
                    data_length = int.from_bytes(length_prefix, "little")

                    # Verify that the entire data can be read
                    if data_length > 0 and mm.size() >= 4 + data_length:
                        data = mm.read(data_length).decode("utf-8")
                        handle_data(data)
                    else:
                        print("Data length exceeds the current readable size. Please increase buffer size.")
                        print(f"Data length: {data_length / 1024} KB, buffer size: {mm.size() / 1024} KB")
                else:
                    print("Not enough data to read length prefix.")
                time.sleep(self.interval)

    def start_reading(self):
        if not self.running:
            self.running = True
            self.start()

    def stop_reading(self):
        self.running = False


# Blender UI
class MMF_PT_ControlPanel(bpy.types.Panel):
    bl_label = "MMF Control Panel"
    bl_idname = "MMF_PT_ControlPanel"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "MMF Reader"

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        # File name input
        layout.prop(scene, "mmf_filename")

        # Interval input
        layout.prop(scene, "mmf_interval")

        # buffer size input
        layout.prop(scene, "mmf_buffer_size")

        # Start and stop buttons
        layout.operator("mmf.start_reading", text="Start Reading")
        layout.operator("mmf.stop_reading", text="Stop Reading")


class MMF_OT_StartReading(bpy.types.Operator):
    bl_idname = "mmf.start_reading"
    bl_label = "Start Reading MMF"

    def execute(self, context):
        scene = context.scene
        global file_reader
        file_reader = PeriodicFileReader(scene.mmf_filename, scene.mmf_interval, scene.mmf_buffer_size)
        file_reader.start_reading()
        return {"FINISHED"}


class MMF_OT_StopReading(bpy.types.Operator):
    bl_idname = "mmf.stop_reading"
    bl_label = "Stop Reading MMF"

    def execute(self, context):
        file_reader.stop_reading()
        return {"FINISHED"}


def register():
    bpy.utils.register_class(MMF_PT_ControlPanel)
    bpy.utils.register_class(MMF_OT_StartReading)
    bpy.utils.register_class(MMF_OT_StopReading)
    bpy.types.Scene.mmf_filename = bpy.props.StringProperty(name="MMF Filename", default="test-mmf")
    bpy.types.Scene.mmf_interval = bpy.props.FloatProperty(
        name="Interval (seconds)", default=0.1, min=0.01, max=2.0
    )
    bpy.types.Scene.mmf_buffer_size = bpy.props.IntProperty(name="Buffer Size (KB)", default=1024, min=512, max=8192) # 512KB to 8MB


def unregister():
    bpy.utils.unregister_class(MMF_PT_ControlPanel)
    bpy.utils.unregister_class(MMF_OT_StartReading)
    bpy.utils.unregister_class(MMF_OT_StopReading)
    del bpy.types.Scene.mmf_filename
    del bpy.types.Scene.mmf_interval
    del bpy.types.Scene.mmf_buffer_size


if __name__ == "__main__":
    register()
