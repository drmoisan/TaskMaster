# Scope Lock After the Phase 1 Comment Corrections

Timestamp: 2026-08-23T19-03

Command:
```bash
MB=f85a36faebaaec29fe5233c9d9f69d223d80e4c5   # $MergeBase, recorded by P0-T6
git diff --name-only $MB
git diff --name-only $MB | grep -E '\.(cs|csproj|props|targets|config)$'
git diff --name-only $MB | grep -c '^QuickFiler/'
git diff --name-only $MB | grep -c '\.csproj$'
git diff --name-only $MB | grep '^\.claude/' | grep -vc '^\.claude/agent-memory/'
```

EXIT_CODE: 0

Output Summary:

Filtered set (paths ending `.cs`, `.csproj`, `.props`, `.targets`, or `.config`) — exactly 3 members,
matching the three paths the scope lock admits:

| # | Path |
| --- | --- |
| 1 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` |
| 2 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` |
| 3 | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` |

Prohibited counts, measured against the unfiltered `git diff --name-only $MergeBase` list:

| Prohibited class | Count | Required |
| --- | --- | --- |
| paths beginning `QuickFiler/` (production) | 0 | 0 |
| paths ending `.csproj` | 0 | 0 |
| paths beginning `.claude/` other than `.claude/agent-memory/` | 0 | 0 |

The unfiltered list additionally carries 22 `.claude/agent-memory/` Markdown files and 46 paths under
`docs/features/active/winformspumphost-suite-determinism-511/` (the feature's own documentation and
evidence tree, including the `evidence/.gitignore` appended by P0-T9). Both classes are permitted by
plan prohibition 6 and are excluded from the scope-lock filter, which admits only `.cs`, `.csproj`,
`.props`, `.targets`, and `.config` paths.

The scope lock holds: the executable-code diff against the merge base is exactly the three
`QuickFiler.Test/` files, unchanged in membership from before Phase 1.
