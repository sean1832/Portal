import argparse
import os
import subprocess

import yaml
from vspacker import package_builder as builder


def parse_args():
    parser = argparse.ArgumentParser(description="Package files to zip")
    parser.add_argument("project_file", help="project file")
    parser.add_argument("--runyak", action="store_true", help="runyak")
    return parser.parse_args()


def update_manifest_version(manifest_file, version):
    with open(manifest_file, "r") as f:
        manifest = yaml.safe_load(f)
    manifest["version"] = version
    with open(manifest_file, "w") as f:
        yaml.dump(manifest, f)


def main():
    args = parse_args()
    project_file = args.project_file
    solution_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    example_dir = os.path.join(solution_root, "Example/grasshopper")

    proj = builder.AssemblyBuilder(project_file)
    proj.build_zip(
        input_dir=os.path.join(proj.project_dir, "bin", "release", "net48"),
        zip_filename=f"{proj.project_name}-{proj.version}.zip",
        exclude_patterns=["*.xml", "*.pdb"],
        include_files=[
            os.path.join(solution_root, "LICENSE"),
            os.path.join(solution_root, "README.md"),
        ],
        internal_folder=proj.project_name,
    )
    proj.build_folder(
        dir_paths=os.path.join(proj.project_dir, "bin", "release", "net48"),
        folder_name="yak",
        exclude_patterns=["*.xml", "*.pdb"],
        include_files=[
            os.path.join(solution_root, "LICENSE"),
            os.path.join(solution_root, "README.md"),
            os.path.join(solution_root, "manifest.yml"),
            (os.path.join(solution_root, "Design", "Logo@512x-80.jpg"), "icon.jpg"),
        ],
    )
    update_manifest_version(
        os.path.join(proj.output_folder, "yak", "manifest.yml"),
        proj.version,
    )
    proj.build_zip(
        input_dir=example_dir,
        zip_filename=f"{proj.project_name}-examples-{proj.version}.zip",
        exclude_patterns=[],
        include_files=[],
    )

    if args.runyak:
        # go to the output folder
        os.chdir(os.path.join(proj.output_folder, "yak"))
        # call yak command
        subprocess.run(["yak", "build", "--platform", "win"])


if __name__ == "__main__":
    main()
