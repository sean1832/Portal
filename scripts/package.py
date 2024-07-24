import argparse
import os

from vspacker import package_builder as builder


def parse_args():
    parser = argparse.ArgumentParser(description="Package files to zip")
    parser.add_argument("project_file", help="project file")
    return parser.parse_args()


def main():
    args = parse_args()
    project_file = args.project_file
    solution_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    example_file = os.path.join(solution_root, "Example/grasshopper", "portal-example.gh")

    bd = builder.AssemblyBuilder(project_file)
    bd.build_zip(
        zip_filename=f"{bd.project_name}_v{bd.version}.zip",
        exclude_patterns=["RhinoCommon.dll", "Grasshopper.dll", "*.xml", "*.pdb"],
        include_files=["LICENSE", "README.md"],
    )
    bd.package_example(example_file)


if __name__ == "__main__":
    main()
