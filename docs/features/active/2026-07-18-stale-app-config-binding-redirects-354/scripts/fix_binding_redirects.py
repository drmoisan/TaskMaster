import re
import glob

EXCLUDE_PROJECTS = {"SVGControl", "SVGControl.Test"}


def project_list():
    projs = []
    for path in glob.glob("*/packages.config"):
        proj = path.split("/")[0].split("\\")[0]
        if proj not in EXCLUDE_PROJECTS:
            projs.append(proj)
    return projs


REF_RE = re.compile(
    r'<Reference Include="([\w.]+), Version=([0-9.]+), Culture=neutral, PublicKeyToken=([0-9a-fA-F]+)[^"]*">'
)

report = []
for proj in project_list():
    csproj_path = f"{proj}/{proj}.csproj"
    app_cfg_path = f"{proj}/app.config"
    try:
        with open(csproj_path, encoding="utf-8") as f:
            cs_text = f.read()
    except FileNotFoundError:
        continue
    try:
        with open(app_cfg_path, encoding="utf-8") as f:
            app_text = f.read()
    except FileNotFoundError:
        continue

    real_versions = {}
    for m in REF_RE.finditer(cs_text):
        pid, asm_ver, token = m.groups()
        real_versions[pid] = (asm_ver, token)

    orig_app_text = app_text
    n = 0
    for pid, (real_ver, token) in real_versions.items():
        pattern = re.compile(
            r'(name="'
            + re.escape(pid)
            + r'"\s*\n\s*publicKeyToken="'
            + re.escape(token)
            + r'"\s*\n\s*culture="neutral"\s*\n\s*/>\s*\n\s*<bindingRedirect oldVersion="0\.0\.0\.0-)'
            + r"([0-9.]+)"
            + r'(" newVersion=")'
            + r"([0-9.]+)"
            + r'(")'
        )

        def repl(m2, real_ver=real_ver):
            old_lo, cur_new = m2.group(2), m2.group(4)
            if cur_new == real_ver:
                return m2.group(0)
            new_hi = real_ver if _ver_tuple(real_ver) >= _ver_tuple(old_lo) else old_lo
            return m2.group(1) + new_hi + m2.group(3) + real_ver + m2.group(5)

        def _ver_tuple(v):
            return tuple(int(x) for x in v.split("."))

        new_text, c = pattern.subn(repl, app_text)
        if c and new_text != app_text:
            app_text = new_text
            n += c
            report.append(f"{proj}: app.config {pid} bindingRedirect -> {real_ver}")

    if app_text != orig_app_text:
        with open(app_cfg_path, "w", encoding="utf-8", newline="") as f:
            f.write(app_text)

for line in report:
    print(line)
print("TOTAL:", len(report))
