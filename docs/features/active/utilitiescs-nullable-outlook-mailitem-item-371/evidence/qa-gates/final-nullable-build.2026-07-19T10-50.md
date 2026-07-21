# Final QC — Pragma-Only Nullable / TreatWarningsAsErrors Type-Check Gate (P10-T3)

- Timestamp: 2026-07-19T10-50
- Task: [P10-T3]
- Plan-literal Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable` — confirmed)
  - EXIT_CODE (solution TWAE): 1 — halts at the vendored `SVGControl` project on 2 PRE-EXISTING out-of-scope CS0649 errors (`SvgImageSelector._relativeImagePath`/`_absoluteImagePath` never assigned), before reaching `UtilitiesCS`. This is the same documented epic-integration-branch condition from P0-T4 (present for the already-merged #363/#364 children); it is not introduced by #371 and is not fixable in this cluster-scoped child.
- Authoritative in-scope CS86xx Command (SVGControl.dll first restored WITHOUT TWAE): `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO TWAE, NO `/p:Nullable=enable`; `warning CS86xx` count == TWAE `error CS86xx` count because `UtilitiesCS.csproj` is nullable-oblivious by default)
  - EXIT_CODE (isolated authoritative build): 0

## Output Summary

- CS86xx across all 30 opted-in `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` files: **0**.
- CS86xx total across the ENTIRE UtilitiesCS assembly (including out-of-scope nullable-enabled files such as `Extensions/DfDeedle.cs`, `DfDeedle.FrameUtilities.cs`, and all #363/#364-remediated files): **0** — no regression introduced in any out-of-scope nullable-enabled consumer of the remediated public surface.
- Errors: 0.
- Confirmed: `/p:Nullable=enable` was NOT passed; no `<Nullable>` element exists (see P10-T5).
- All 30 in-scope files carry a single whole-file `#nullable enable` pragma on line 1 (verified 30/30).
