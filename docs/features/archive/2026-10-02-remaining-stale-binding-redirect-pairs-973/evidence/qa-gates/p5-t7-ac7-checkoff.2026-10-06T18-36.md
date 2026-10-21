# P5-T7 AC7 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC7 ` and `^- \[[ x]\] AC[0-9]+ ` (count); live Greps over glob */packages.config and */*.csproj; git -C <execution-worktree-root> diff --name-only a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- '*.csproj' '*packages.config'
EXIT_CODE: 0
Output Summary: AC7 met and checked off. Each of the five manifests carries exactly one System.Linq.AsyncEnumerable 10.0.12 net481 package element. Each of the five csproj carries one aliased Reference at Version=10.0.0.12 with the net462 HintPath, the Aliases child, and the preceding comment citing CS0121 and CS0433. No other csproj or packages.config is in the diff.

Artifacts read:
- P3-T1 to P3-T5 (p3-t1-UtilitiesCS-packages, p3-t2-QuickFiler-packages, p3-t3-ToDoModel-packages, p3-t4-TaskMaster-packages, p3-t5-UtilitiesCS.Test-packages; each EXIT_CODE 0).
- P3-T8 to P3-T12 (p3-t8-UtilitiesCS-csproj, p3-t9-QuickFiler-csproj, p3-t10-ToDoModel-csproj, p3-t11-TaskMaster-csproj, p3-t12-UtilitiesCS.Test-csproj; each EXIT_CODE 0).
- evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md (EXIT_CODE 0).

Live observations (2026-10-06):
- Grep `id="System\.Linq\.AsyncEnumerable" version="10\.0\.12" targetFramework="net481"` over */packages.config: 5 files, 1 each (UtilitiesCS, QuickFiler, ToDoModel, TaskMaster, UtilitiesCS.Test).
- Grep, five alternatives over */*.csproj (Include `System.Linq.AsyncEnumerable, Version=10.0.0.12,`; `<Aliases>SystemLinqAsyncEnumerable</Aliases>`; the `lib\net462\System.Linq.AsyncEnumerable.dll</HintPath>` line; `CS0121`; `CS0433`): 5 matching lines in each of the same five projects.
- Grep `Aliased on purpose \(issue #973\)` over */*.csproj: 1 in each of the five projects.
- diff --name-only over '*.csproj' and '*packages.config': exactly the five csproj and the five packages.config.

SPEC-LINE: `- [x] AC7 (package installed in the 5 projects).` (criterion text unchanged)
