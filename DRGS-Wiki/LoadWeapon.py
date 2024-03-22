import os
import sys
import json

base_path = "E:/Programme/Steam/steamapps/common/Deep Rock Survivor/BepInEx/plugins/DRGS-Wiki/data"
io_path = base_path + "/io.txt"

title = sys.argv[1].removesuffix("/Data")


def find_all(name, path):
    result = []
    for root, dirs, files in os.walk(path):
        for file_name in files:
            if file_name == "LMG Gun Platform_Sub_ProjectileWeaponSkill_LMG_Turret.wiki":
                continue
            if file_name.lower().startswith(name.lower()):
                result.append(os.path.join(root, file_name))
    return result


files = find_all(title, base_path + "/weapons")

result = ""

if len(files) == 1:
    with open(files[0], "r") as file:
        result = file.read()
else:
    result = "Error: " + str(len(files)) + " files found: " + str(files)

with open(io_path, "w") as file:
    file.write(result)
