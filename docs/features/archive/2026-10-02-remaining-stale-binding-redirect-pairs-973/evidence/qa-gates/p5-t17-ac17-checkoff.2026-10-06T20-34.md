# AC17 check-off (remediation cycle 1, P1-T5; base plan P5-T17)

Timestamp: 2026-10-06T20-34
Command: Edit tool on spec.md (AC17 checkbox `- [ ]` to `- [x]`, nothing else on the line); Grep `^- \[x\] AC17 ` with -n over spec.md (the Grep tool printed `375:[Omitted long matching line]`, so the Read tool at line 375, limit 1, supplied the text); Grep counts `^- \[x\] AC17 `, `^- \[ \] AC18 `, `^- \[x\] AC[0-9]+ `, `^- \[ \] AC[0-9]+ ` and `\r$` over spec.md; git -C <execution-worktree-root> diff --numstat HEAD -- docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md
EXIT_CODE: 0

AC17: MET (as amended by Planner Amendment 6)

Before state: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T18-36.md` (NOT MET under the version 1.1 wording), retained unchanged.

System.Linq.AsyncEnumerable half:
- TaskMaster/app.config lines 218-219: System.Linq.AsyncEnumerable redirect oldVersion 0.0.0.0-10.0.0.12, newVersion 10.0.0.12 (the AC8 value) — `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t4-azure-core-redirect-census.2026-10-06T20-33.md`, item (d).
- AC12 artifact `^BIN TaskMaster System\.Linq\.AsyncEnumerable\.dll=True` count 1 — same artifact, item (e); P1-T3 also prints `PRESENT TaskMaster System.Linq.AsyncEnumerable.dll=True`.
- TaskMaster/app.config lines 90-91: Azure.Core redirect oldVersion 0.0.0.0-1.63.0.0, newVersion 1.63.0.0 — same artifact, item (d).

Azure.Core observations:
- (i) Sixteen-config redirect census: 16 AZURE-REDIRECT lines, every redirect `0.0.0.0-1.63.0.0` / `1.63.0.0`, literal count 1 in each of the 16 files and no other; 10 csproj Reference lines all `Version=1.63.0.0`, none in TaskMaster/TaskMaster.csproj — `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t4-azure-core-redirect-census.2026-10-06T20-33.md` (P1-T4).
- (ii) Reference scan over TaskMaster\bin\Debug: `FILES=48 MANAGED=48 SKIPPED=0 AZURECORE_REFERRERS=0`, no REF line, `ASM UtilitiesCS.dll REFS=35` and `ASM TaskMaster.dll REFS=30` present — `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t1-azure-core-refscan-taskmaster.2026-10-06T20-31.md` (P1-T1).
- (iii) Positive control over UtilitiesCS\bin\Debug: `FILES=106 MANAGED=106 SKIPPED=0 AZURECORE_REFERRERS=3`, `REF Microsoft.Kiota.Authentication.Azure.dll Azure.Core=1.50.0.0`; KIOTA-REQUESTS-AZURE-CORE: 1.50.0.0 (at or below 1.63.0.0) — `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t2-azure-core-refscan-utilitiescs-control.2026-10-06T20-31.md` (P1-T2).
- (iv) GetAssemblyName: Azure.Core.dll 1.63.0.0 in UtilitiesCS, UtilitiesCS.Test and TaskMaster.Test bin\Debug; Test-Path in TaskMaster\bin\Debug: Azure.Core.dll False, Microsoft.Kiota.Authentication.Azure.dll False — `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t3-azure-core-version-presence.2026-10-06T20-32.md` (P1-T3).

Acceptance counts over spec.md after the edit:
- `^- \[x\] AC17 ` count 1 (line 375)
- `^- \[ \] AC18 ` count 1
- `^- \[x\] AC[0-9]+ ` count 22
- `^- \[ \] AC[0-9]+ ` count 1
- CMD-CRCOUNT 0
- numstat against HEAD before the commit: `1	1	docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md`

SPEC-LINE: - [x] AC17 (invariant trace delivered; Azure.Core clause as amended by Planner Amendment 6 on 2026-10-06). Reading `TaskMaster/app.config` after the change shows the Azure.Core block at oldVersion 0.0.0.0-1.63.0.0 and newVersion 1.63.0.0 and the System.Linq.AsyncEnumerable block at the AC8 value, and the AC12 artifact shows System.Linq.AsyncEnumerable.dll in TaskMaster's bin\Debug, so the traced System.Linq.AsyncEnumerable 10.0.0.6 request from System.Linq.Async redirects to an assembly that exists in the add-in's output directory. For the traced Azure.Core 1.50.0.0 request from Microsoft.Kiota.Authentication.Azure the criterion is the following four observations, each recorded with its command and output in a check-off artifact under the feature's qa-gates evidence folder: (i) every app.config directly under a root-level directory that carries an Azure.Core dependentAssembly block, namely `Tags/app.config`, `TaskTree/app.config`, `TaskVisualization/app.config`, `QuickFiler/app.config`, `TaskMaster/app.config`, `ToDoModel/app.config`, `UtilitiesCS/app.config`, `VBFunctions.Test/app.config`, `Tags.Test/app.config`, `TaskTree.Test/app.config`, `QuickFiler.Test/app.config`, `TaskMaster.Test/app.config`, `TaskVisualization.Test/app.config`, `ToDoModel.Test/app.config`, `UtilitiesCS.Test/app.config` and SVGControl.Test/app.config (sixteen files, enumerated by a Grep of the assemblyIdentity name attribute; SVGControl.Test is unbackticked because it is not written), reads oldVersion 0.0.0.0-1.63.0.0 and newVersion 1.63.0.0, the one version every Azure.Core csproj Reference Include declares (`UtilitiesCS/UtilitiesCS.csproj` and nine test csproj); (ii) a System.Reflection.Metadata scan over every file with the .dll or .exe extension directly in TaskMaster\bin\Debug, printing one line per managed assembly with its AssemblyReferences count and one line per reference whose name is Azure.Core, reports zero assemblies referencing Azure.Core, with UtilitiesCS.dll and TaskMaster.dll among the scanned assemblies, so no assembly deployed beside TaskMaster.dll requests Azure.Core, the add-in process issues no such request, and the TaskMaster/app.config block is correct and inert; (iii) the identical scan over UtilitiesCS\bin\Debug, the positive control showing that the scan detects the reference, reports at least one referencing assembly and names Microsoft.Kiota.Authentication.Azure.dll with the Azure.Core version it references (expected 1.50.0.0; the printed value governs, is recorded, and must be at or below 1.63.0.0 so that it lies inside the corrected range); and (iv) System.Reflection.AssemblyName.GetAssemblyName on the deployed Azure.Core.dll in UtilitiesCS\bin\Debug, UtilitiesCS.Test\bin\Debug and TaskMaster.Test\bin\Debug prints Version 1.63.0.0 for each, so the corrected test-host redirect targets a deployed file, and a Test-Path probe for Azure.Core.dll and for Microsoft.Kiota.Authentication.Azure.dll in TaskMaster\bin\Debug prints False for both, consistent with (ii).

Output Summary:
- AC17 is MET under the Planner Amendment 6 wording: observations (i) to (iv) each hold as recorded in the P1-T4, P1-T1, P1-T2 and P1-T3 artifacts, and the System.Linq.AsyncEnumerable half holds.
- spec.md AC17 checkbox flipped; 22 criteria checked, 1 unchecked (AC18, out of scope by ruling, pending manual).
- spec.md diff is the one checkbox line (numstat 1/1) and the file stays LF.
- Additional observation recorded by P1-T2, not part of the criterion: Microsoft.Graph.dll and Microsoft.Graph.Core.dll in UtilitiesCS\bin\Debug also reference Azure.Core 1.50.0.0; neither is deployed beside TaskMaster.dll (P1-T1).
