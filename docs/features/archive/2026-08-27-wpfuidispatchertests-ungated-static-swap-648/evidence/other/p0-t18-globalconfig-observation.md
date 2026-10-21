# P0-T18 — `.globalconfig` Documentation Observation (Recorded, Not Acted On)

Timestamp: 2026-09-01T13-58

## Citation 1 — `CLAUDE.md:197`

```
   - C# code must pass Roslyn/.NET analyzer diagnostics configured by `.editorconfig`, `.globalconfig`, and project properties.
```

## Citation 2 — `CLAUDE.md:273`

```
- Prefer built-in .NET SDK analyzers and configuration through `.editorconfig` / `.globalconfig`.
```

## Citation 3 — glob result

A repository-wide glob for `**/.globalconfig`, rooted at the checkout root, returned no files. No
`.globalconfig` exists in this checkout.

## Citation 4 — `.editorconfig:27`

```
dotnet_analyzer_diagnostic.severity = suggestion
```

`.editorconfig` is the only analyzer severity configuration present, and this is the line that sets
the default analyzer diagnostic severity.

## Disposition

The two `CLAUDE.md` lines name an analyzer-configuration input that does not exist in the repository.
This is a documentation discrepancy in a file outside the scope of issue #648. It is recorded here as
an observation only. **No change is made under #648.** Acting on it would require modifying
`CLAUDE.md`, which lies outside the single changed `.cs` path AC-6 permits at
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/issue.md:141-144`, and
`CLAUDE.md` is not a file this plan's scope boundary admits.

No file outside
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/` was modified
by this task.
