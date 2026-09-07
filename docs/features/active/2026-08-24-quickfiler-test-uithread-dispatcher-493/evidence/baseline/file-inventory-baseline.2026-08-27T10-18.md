# File Identity and Size Baseline (P0-T11)

Timestamp: 2026-08-27T10-18
Task: [P0-T11]
Command: `wc -l <paths>` and `sha256sum <paths>` for the five paths below, run from `<repo-root>`
EXIT_CODE: 0
Output Summary: Five rows recorded, each with an integer line count and a 64-character SHA-256.
`QfcItemController.FocusAndThemeTests.cs` measures 497 lines, matching the figure AC-6 states.
`QfcItemController.TestSupport.cs` measures 489 lines, which is 124 lines larger than the 365-line
figure in research §8 and spec § File layout; the divergence and its consequences are recorded
below.

BASE_SHA: `125c36b0669d9dd6095f156901bba138e2272f56`

(as recorded by `P0-T2` in `toolchain-resolution.2026-08-27T09-53.md`)

## Inventory

| Repo-relative path | Line count | SHA-256 |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 489 | `6293904bd2dfacc7c2678481409d576ff651a400ae550cc3a628f89ec6958cdf` |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 418 | `5002ca0e5bedf06708f020f16e654ab4490576be025f1deb83048ba9cc14a31a` |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 490 | `a5133fbbc3b8a2d7ec29d52fd15d8e43d1ec5ee717e1d5f2a4830758ad551302` |
| `UtilitiesCS/Threading/UiThread.cs` | 163 | `87b4fde609398c59346557fb688ba192639ebc888104d74fea35d24dd18bdeaa` |

Each SHA-256 value is 64 hexadecimal characters. `P4-T1` recomputes the third and fifth rows and
compares against the values above.

## Divergence from the research projection — `QfcItemController.TestSupport.cs`

Research §8 and spec § File layout and size projections both record
`QfcItemController.TestSupport.cs` at 365 lines. The measured value at `BASE_SHA` is **489**. This is
not a measurement error and is not a defect: this feature branches from
`epic/quickfiler-bug-family-integration`, whereas research measured `main` at `988e819b`. Sibling
epic features have since landed shared arrange helpers into the tail of this file — the helpers carry
explicit `Issue #480`, `Issue #485`, and `Issue #483` markers in their XML doc comments — which
accounts for the growth.

Consequences, recorded so later tasks are read against a disclosed baseline:

- **Headroom is much tighter than projected.** The file starts at 489 of the 500-line ceiling, i.e.
  11 lines of headroom, not the 135 research recorded. `P2-T1` is a net deletion of roughly 40 lines
  (a 12-line method collapsed to one line, plus a 2-line field pair, a 34-line factory method, and
  an 8-line orphaned doc block removed, offset by a retained doc comment), so the post-edit count is
  expected to fall well below the ceiling. `P4-T3` measures it rather than assuming it. Per Decisions
  Record D2 this plan treats AC-8 as a fresh measurement and never restates a projection, so the
  divergence changes no gate.
- **Every line citation into this file in the plan is shifted by exactly +3.** Verified with
  line-numbered searches against the file at `BASE_SHA`:

  | Plan citation | Actual span at `BASE_SHA` | Content |
  | --- | --- | --- |
  | `213-220` (orphaned XML doc block) | `216-223` | the dispatcher-pumping `<summary>` block that documents neither field below it |
  | `221-222` (field declarations) | `224-225` | `_dedicatedDispatcher` and `_dedicatedDispatcherLock` |
  | `238-249` (`EnsureUiThreadDispatcher`) | `241-252` | the 12-line method, signature at 241 |
  | `251-285` / `257-285` (`GetDedicatedDispatcher`) | `254-288` / `260-288` | doc block from 254, method signature at 260 |
  | `297-317` (`StartRunningDispatcher`) | `300-320` | must stay in `QfcItemControllerTestSupport` |
  | `323-326` (`ShutdownDispatcher`) | `326-329` | must stay in `QfcItemControllerTestSupport` |

  The offset is uniform, and the members the plan names are unambiguous by identity. `P0-T14` and
  `P2-T1` therefore act on the members named by the plan, located at the actual spans above, and
  each records the substitution in its own artifact. No member is guessed at from a line number.

## Divergence check on the other four paths

| Path | Research / spec figure | Measured | Divergent |
| --- | --- | --- | --- |
| `QfcItemController.InitializationTests.Part2.cs` | 418 | 418 | no |
| `QfcItemController.FocusAndThemeTests.cs` | 497 | 497 | no |
| `UtilitiesCS/Threading/UiThread.cs` | 163 | 163 | no |
| `QuickFiler.Test/QuickFiler.Test.csproj` | (no figure stated) | 490 | n/a |

`Part2.cs` matches its research figure exactly, so the line citations in the plan's § Part2 Migration
section are expected to be valid as written. `P2-T2` verifies them before editing.
