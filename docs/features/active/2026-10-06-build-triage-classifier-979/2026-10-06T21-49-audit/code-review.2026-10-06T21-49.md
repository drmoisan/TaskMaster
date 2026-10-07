# Code Review — Issue #979

Timestamp: 2026-10-06T21-49
Base branch: `main`
Reviewed head: `ca8b98d6a69cfbb38439571c2105cda3994ea8f0`

## Executive Summary

The model preservation, classifier reconstruction, and XML/callback wiring are appropriately scoped and tested. One blocking defect prevents the requested user action from running when Triage is disabled: the action silently returns instead of creating or obtaining the existing Triage instance. No other blocking code-review finding was identified.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 141-153 | The command reads a Triage instance only from `InboxEngines` and then silently returns when none is present. | Use the existing lazy Triage lifecycle and test the absent-engine path. | Disabled engines are deliberately excluded from `InboxEngines`, while the menu action stays enabled. | `AppItemEngines.cs:63-64`; controller lines 141-153; XML lines 303-307. |

## Blocking Finding

### CR-979-1 — Build Triage Classifier silently does nothing when the Triage engine is disabled

**Severity:** blocking

`TaskMaster/Ribbon/RibbonController.Intelligence.cs:141` implements the menu action. After an injected test delegate is considered, it obtains `Triage` at line 149 and invokes the rebuild only when that property is non-null. `Triage` reads `Globals.Engines.InboxEngines["Triage"]`. `TaskMaster/AppGlobals/AppItemEngines.cs:63-64` builds `InboxEngines` from configurations where `config.Value.Engine` is true, so a disabled Triage engine is intentionally absent from that map. The XML button at `TaskMaster/Ribbon/RibbonExplorer.xml:303-307` is not readiness-gated and remains selectable. In that supported configuration the action completes without rebuilding and without feedback.

This violates the command acceptance criterion: selecting `TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier` must invoke the mined-mail rebuild flow. The current test injects `TriageClassifierRebuildAsync`, so it does not exercise the actual controller path when the engine is absent.

**Remediation:** obtain or initialize the existing Triage instance through the controller's lazy Triage creation path, then invoke `RebuildFromStagedMinedMailAsync`; preserve the existing injected delegate seam. Add a deterministic test that represents a disabled/absent `InboxEngines["Triage"]` entry and verifies the rebuild seam or the actual rebuild method is reached.

## Verified Behavior

- `MinedMailInfo` retains nullable `Triage` values in construction and deep copying.
- The rebuild filters training data to exact `A`, `B`, and `C` labels, builds aggregate state, persists through the existing configuration path, and replaces the manager entry.
- The ribbon XML and callback signatures are present and the focused unit tests pass.

## Non-blocking Observations

The review found no formatting, compiler, analyzer, nullable, or whitespace defect in the committed implementation.
