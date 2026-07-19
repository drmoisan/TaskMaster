# Baseline File Inventory — SVGControl/

Timestamp: 2026-07-19T00-05

Command used to enumerate: `find SVGControl -name "*.cs" | sort`, with per-file line count
(`wc -l`) and `#nullable enable` presence confirmed via a `^#nullable enable` regex search
(the `Grep` tool, anchored, case-sensitive).

Total files: 20

## Group 1 — Already `#nullable enable` (verify-only, 3 files)

| File | Lines | `#nullable enable` present |
|---|---|---|
| SVGControl/PathInternal.cs | 251 | yes |
| SVGControl/RelativePath.cs | 1678 | yes |
| SVGControl/ValueStringBuilder.cs | 341 | yes |

## Group 2 — Hand-authored remediation targets (12 files, pragma to be added)

| File | Lines | `#nullable enable` present |
|---|---|---|
| SVGControl/ButtonSVG.cs | 83 | no |
| SVGControl/PictureBoxSVG.cs | 63 | no |
| SVGControl/ToggleSwitch.cs | 52 | no |
| SVGControl/SVGParser.cs | 111 | no |
| SVGControl/SvgRenderer.cs | 344 | no |
| SVGControl/SvgImageSelector.cs | 335 | no |
| SVGControl/ISvgResource.cs | 30 | no |
| SVGControl/SvgOptionsConverter.cs | 59 | no |
| SVGControl/SvgOptionsConverter2.cs | 73 | no |
| SVGControl/SvgResourceConverter.cs | 41 | no |
| SVGControl/DropDownEditor.cs | 123 | no |
| SVGControl/SVGFileNameEditor.cs | 80 | no |

## Group 3 — Designer/generated files, not opted in (5 files)

| File | Lines | `#nullable enable` present |
|---|---|---|
| SVGControl/ButtonSVG.Designer.cs | 43 | no |
| SVGControl/PictureBoxSVG.Designer.cs | 45 | no |
| SVGControl/ToggleSwitch.Designer.cs | 36 | no |
| SVGControl/Properties/Resources.Designer.cs | 83 | no |
| SVGControl/Properties/AssemblyInfo.cs | 36 | no |

## Totals confirmation

- 3 (Group 1) + 12 (Group 2) + 5 (Group 3) = 20 files. Matches the total `.cs` file count
  found under `SVGControl/`.
