# [P5-T1] Scoped Formatting Pass (change set only)

Timestamp: 2026-08-26T10-52

Task: [P5-T1]
Feature: docs/features/active/quickfiler-bug-family-446

## Scope derivation

Path list produced by `git diff --name-only <mb>...HEAD -- "*.cs"` where `<mb>` is the
merge-base sha `61edc19befcf6c4e95b5acd32542f2dcdab41b78` recorded by `[P0-T3]`. Thirteen paths
were returned. Per D-Plan-4 the mutating pass is scoped to exactly this list; a repo-wide
mutating pass would rewrite non-owned production files and `packages.config` files (which
`.csharpierignore` does not exempt) and would break AC18, AC19 and AC22.

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcDatamodelTests.cs QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs QuickFiler/Controllers/QfcDatamodel.cs QuickFiler/Controllers/QfcFormController.Actions.cs QuickFiler/Controllers/QfcHomeController.Iteration.cs QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs QuickFiler/Interfaces/IQfcDatamodel.cs'`
EXIT_CODE: 0

CSharpier printed `Formatted 13 files in 6083ms.` That line reports files **processed**, not files
**changed**, so the rewritten-file count below is derived from the SHA-256 digest comparison and
never from that line.

## SHA-256 digests before the pass

| path | sha256 |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | `a63bd471cdd274069ab0ae7a5b96c917db2f44e997897c22721e90907366edeb` |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | `676a1b53dc160bbe5bd999b9a4288d7c9d672e9b01d3ec93560549e2fc7e8bbc` |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | `501f713625d656b60d00f4b9ba621da7c1c4d3890c877594b472f470100a4f78` |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | `9dc6847a3653cad5380d21adcdca1e8f3a7ce7724905cee71e5d21b3e3b4e49b` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | `bf55b5fd0ce7a6fd1805145298d3850df7e38669cafe591e8b238f10e819ec86` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | `2112277eaf4e69712bfaea7936142aa259c81d6f77270a77c0172a279b01d6e6` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | `b2a15f879f9ef7e47d6a3aef352f0f3d5a6f4f444babacada7d4afde6c3225f1` |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `4e0dcd2e991793dcba610c80855d80ed5bcbdf859409d346b39fd8245ea266a2` |
| `QuickFiler/Controllers/QfcDatamodel.cs` | `f912694600622b220991ae77ad398d61da810e15ddf456e13ec8373c278b6121` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | `83ac1911d99a028b87387c41b0d75ff41c7abc2b3189d22628051d8f6b7369de` |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | `61f39f1cdf922bc3ad240bb70f0d91ede037d36adb70e14bbb1c7cd303348e78` |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | `cc41852bb9cf0139ac64fdedb6eecb8029ef58a6826c7bb75ee5cb23222bd699` |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | `f2b4a4763082d21a0dd356ec0553fd5f48fbca9e8a0344e3e81c349b650bc1c1` |

## SHA-256 digests after the pass

| path | sha256 |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | `a63bd471cdd274069ab0ae7a5b96c917db2f44e997897c22721e90907366edeb` |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | `676a1b53dc160bbe5bd999b9a4288d7c9d672e9b01d3ec93560549e2fc7e8bbc` |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | `501f713625d656b60d00f4b9ba621da7c1c4d3890c877594b472f470100a4f78` |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | `9dc6847a3653cad5380d21adcdca1e8f3a7ce7724905cee71e5d21b3e3b4e49b` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | `bf55b5fd0ce7a6fd1805145298d3850df7e38669cafe591e8b238f10e819ec86` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | `2112277eaf4e69712bfaea7936142aa259c81d6f77270a77c0172a279b01d6e6` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | `b2a15f879f9ef7e47d6a3aef352f0f3d5a6f4f444babacada7d4afde6c3225f1` |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `4e0dcd2e991793dcba610c80855d80ed5bcbdf859409d346b39fd8245ea266a2` |
| `QuickFiler/Controllers/QfcDatamodel.cs` | `f912694600622b220991ae77ad398d61da810e15ddf456e13ec8373c278b6121` |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | `83ac1911d99a028b87387c41b0d75ff41c7abc2b3189d22628051d8f6b7369de` |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | `61f39f1cdf922bc3ad240bb70f0d91ede037d36adb70e14bbb1c7cd303348e78` |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | `cc41852bb9cf0139ac64fdedb6eecb8029ef58a6826c7bb75ee5cb23222bd699` |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | `f2b4a4763082d21a0dd356ec0553fd5f48fbca9e8a0344e3e81c349b650bc1c1` |

## Derived rewritten-file count

**0.** All thirteen digest pairs are byte-identical, so CSharpier rewrote no file. `git status
--porcelain` immediately after the pass reported only the untracked `.claude/state/` entry, which
is outside this change set and is never staged by any task in this plan.

Because no file was rewritten and the exit code is `0`, the restart rule of Phase 5 is not
triggered by this task and the loop proceeds to `[P5-T2]`.

## Output Summary

Scoped mutating CSharpier pass over the 13 change-set `.cs` paths exited `0` and rewrote `0`
files, established by comparing SHA-256 digests taken before and after the pass rather than by
reading CSharpier's processed-file line. This is the pass the loop accepts.
