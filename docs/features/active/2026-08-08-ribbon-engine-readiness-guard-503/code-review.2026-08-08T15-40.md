# Code Review — ribbon-engine-readiness-guard (Issue #503)

- Review timestamp: 2026-08-08T15-40
- Cycle: **re-audit following remediation cycle 1**
- Base branch: `main` @ `003c5715055d7d1933db68a742531332756e30b2`
- Feature branch: `bug/ribbon-engine-readiness-guard-503` @ `85ff0ee4f0579a3622f2da3a21a6e942b3e4cd12`
- Scope: full branch diff versus base — 13 `.cs`, 1 `.xml`, 2 `.csproj`, 107 documentation/evidence files

## Executive Summary

**No blockers.** The implementation is well-designed, and the design choice at its centre is the right one on the merits rather than the convenient one.

The core insight is that a coarse `IsInitialized` flag would have been *incorrect*, not merely inelegant: `InitAsync()` filters engines on configuration and drops null factory results, so a global "initialized" signal would report ready for a command that will never work — converting a timing bug into a permanent one. The per-key probe avoids that, handles `RestartEngineAsync` for free because nothing is cached, and requires a zero-line diff to the two files the requirements fence off. The refusal to add an `IAppItemEngines` member is equally well-reasoned: on .NET Framework 4.8.1 there are no default interface members, so the member could only be bodied inside a `[ExcludeFromCodeCoverage]` class and the new decision logic would have been entirely uncoverable. Reading an existing interface member instead keeps every decision testable. That is the opposite of the anti-pattern where a coverage attribute is substituted for a real seam.

The guard mechanism is also the right shape. Deferring the engine dereference into a `Func<Task>` lambda means the null-or-missing engine is never touched when the gate is closed, which converts both reported exception types into a no-op without scattering null-conditional operators through `RibbonViewer`. `EngineGatedCommandRunner` contains no `catch` clause of any kind, so it structurally cannot degenerate into a swallow-all — an exception from a ready action propagates unchanged, and there is a named test pinning that.

Both findings from the first review were re-examined against the tree rather than accepted on report. **F1 is genuinely remediated**, with a mutate/fail/restore proof that is stronger than the usual claim: the mutation was verified present inside the built assembly before the test ran, a green control run on the unmutated resource is recorded, and the failure stack frame names the exact new assertion line. **F2 was correctly escalated rather than forced** — the formatter conflict was reproduced independently in this session by running CSharpier against a probe document, and the collapsed single-line form is genuinely rejected at 116 characters against a 100-column print width.

Six findings are recorded below. All are Low or Informational; none blocks the PR. Three of them concern process artifacts (a commit message, the PR-context generator) rather than shipped code.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | `TaskMaster/Ribbon/RibbonExplorer.xml` | whole file | File is 539 lines, above the 500-line guidance; grew from a pre-existing 519. The +20 decomposes into 8 functionally required `getEnabled` attributes and 12 formatter-mandated expansion lines. | Accept for this fix. Track a resource split (for example separating the Triage and SpamBayes groups into their own embedded parts) as its own issue rather than inside a bug fix. | The 500-line rule targets "production code, test code, or reusable script file"; this is a declarative embedded UI resource. The overage is pre-existing and none of the growth is discretionary — verified independently, not accepted on report. | `csharpier check` on a probe reproduction rejects the 116-char collapsed form; `.csharpierignore` has no `*.xml` exclusion; no `.csharpierrc` so print width is 100 |
| Low | git history | commit `00bc47bb` | Commit subject reads "make the AC5 ribbon-XML assertion non-vacuous **and restore RibbonExplorer.xml line count**", but the commit contains no `RibbonExplorer.xml` change. F2 was escalated, not fixed. | Amend the subject if the branch is rebased before merge; otherwise ensure the PR body states plainly that F2 was closed as not remediable. | The surrounding documentation is accurate and discloses the escalation fully; only the commit subject overstates. A reader relying on `git log` alone would be misled about what shipped. | `git show --numstat 00bc47bb` lists no `.xml` path; `git diff --numstat <base>..<head> -- TaskMaster/Ribbon/RibbonExplorer.xml` shows the XML unchanged since the implementation commit |
| Low | `artifacts/pr_context.summary.txt` | "Changed files overview" | Generator reported `Core logic changes: 0 files` and bucketed all 16 code paths as documentation. This is not cosmetic: the coverage hook derives its changed-language set from these bullets, so C# coverage enforcement would have been silently skipped. | Corrected in place during this review under a labelled `[REVIEWER CORRECTION]` block preserving the original output. Fix the generator's classifier as separate work. | Recurring generator defect on C#-touching branches; a reviewer who trusted the overview would produce an audit that omitted the only changed language. | Simulation of `Get-ChangedLanguageSet` returned an empty set before correction and `[CSharp]` after |
| Low | `artifacts/pr_context.summary.txt` | "Close candidates" | Author-asserted autoclose list contains `#ISO-8601` — not an issue number — plus `#227` and `#424`, which appear in the feature documents only as precedent citations, not as issues this branch closes. | Before `gh pr create`, restrict closing keywords to `#503`. Do not emit closing keywords for `#227`, `#424`, or `#ISO-8601`. | An unfiltered autoclose list would close unrelated issues on merge. `#ISO-8601` is a regex false positive from the timestamp-format convention. | `artifacts/pr_context.summary.txt` "Auto-close issues (author asserted)" |
| Informational | feature evidence folder | `evidence/**` filenames | Remediation-cycle evidence carries `14-52` timestamps that sort *before* implementation-cycle evidence at `14-56`, `14-58`, `15-00`, `15-05`, `15-10`, `15-12`, so filename ordering does not reflect execution order. | Prefer a monotonic clock reading per artifact write. No action needed for this PR. | Reduces auditability for a later reader reconstructing the sequence. The content is internally consistent and cross-referenced, so no conclusion is affected. | Directory listing of `evidence/`; the executor already recorded a related lesson about timestamp collisions |
| Informational | untouched packages | `UtilitiesCS`, `QuickFiler` | Coverage measurement is nondeterministic: two runs over identical production code move `UtilitiesCS` by ±12 lines and `QuickFiler` by ±1, on an unchanged valid-line denominator. | None for this branch. Already promoted as its own potential entry. | Explains the apparent 17-line drift between the merge-base baseline and the final artifact without invoking a regression; no file in either package is in the diff. | Comparison of `coverage-remediation-baseline.jacoco.xml` against `coverage-remediation-final.jacoco.xml` |

## Design and Implementation Notes

### What is done well

**The readiness contract is precise and its rationale is recorded in code, not just in the spec.** `EngineReadinessGate.TryGetEngine` checks all four conditions — non-null accessor result, non-null `InboxEngines`, key present, value non-null — and `IsEngineReady` delegates to it rather than duplicating the predicate. The XML doc comment explains *why* the probe is per-key rather than global, so a future maintainer who is tempted to "simplify" it to a boolean flag encounters the counter-argument at the point of change.

**Ordinal case sensitivity is treated as a contract, not an accident.** The catalog builds its map with `StringComparer.Ordinal` to match the `ConcurrentDictionary` default, and there is an explicit test asserting `"spam"` is not `"Spam"`. That is the kind of detail that silently breaks later.

**The single-source-of-truth catalog is what makes the XML/code agreement testable.** Because the eight control ids live in exactly one place, `RibbonExplorerXmlTests` can iterate `EngineCommandCatalog.ControlIds` and assert both directions: every catalog id declares the callback, and no element outside the catalog declares it. The negative assertion is the one that prevents over-disabling the ribbon, and it is genuinely load-bearing — it was the criterion that kept AC5 enforced while the AC5 test itself was vacuous.

**The Office callback signature is pinned by reflection.** VSTO silently ignores a `getEnabled` callback whose signature does not match: the code compiles and nothing happens. The test asserts the method is public, instance, returns `bool`, and takes exactly one `Microsoft.Office.Core.IRibbonControl`. The comment explaining why the parameter is compared by `Type.FullName` rather than `typeof` — the test project carries no reference to the Office PIA, and a legacy non-SDK `ProjectReference` does not flow it — is exactly the kind of non-obvious constraint that deserves a comment.

**The STA marshalling is explicit rather than ambient.** `InvalidateEngineCommands` checks `UiThread.Dispatcher.CheckAccess()` and marshals when needed, instead of assuming a captured synchronization context. The doc comment records why: `InitAsync()` is launched via `Task.Run` and only resumes on the STA when a context happened to be captured, which is not true on every load path. This is a real correctness concern, correctly handled.

**Ordering is deliberately not asserted where the platform does not guarantee it.** `EngineCommandRefreshPlannerTests` asserts set equality against the catalog, with a comment stating that Office documents callback ordering as unspecified. Writing a sequence assertion here would have produced a test that passes today and fails unpredictably later.

**Test doubles avoid a known side-effecting seam.** The tests deliberately do not reach readiness through `RibbonController.SB`/`Triage`, whose getters install a real `WindowsFormsSynchronizationContext` on the calling thread. The gate is exercised through its injected accessor instead. This trap is documented in the test class remarks.

**The `RibbonViewer.cs` split is minimal.** The only source change in that file is `public class` → `public partial class`; the rest is region relocation. The split was necessary — the file was at 487/500 and could not have absorbed the new callbacks.

### Points examined and found acceptable

**The three null-forgiving operators.** `engine = null!`, `() => Globals?.Engines!`, and `control?.Id!` each carry an in-code comment recording that null is a *supported value* the consumer treats as "not ready", rather than an assertion that null cannot occur. Each is justified by a pinned non-nullable signature on the consuming side. These are honest annotations, not warning suppression, and each has a corresponding test proving the null path returns `false` instead of throwing.

**`MessageBox.Show` in the notification sink.** A modal dialog is a coarse presentation for a "still loading" notice. The deviation is disclosed in `spec.md` Deviation 1 with the reasoning that the repository has no non-modal notice surface and introducing one is scope creep for a bug fix, and the mechanism matches six existing call sites in the same ribbon layer. Critically, the *decision* to notify and the message *content* are host-neutral and unit-tested through the injected sink; only presentation is exempt. No test constructs a `MessageBox`. This is the correct seam placement, and a nicer surface can be substituted later without touching the tested logic.

**`BuildNotReadyMessage` looking up the rendered id.** The method looks up `renderedControlId` (which is `"(null)"` when the input was null) rather than the raw `controlId`. The comment explains the outcome is identical because `"(null)"` is not a catalog key so it still resolves to `"(unmapped)"`. Verified: the behaviour is the same and the test asserts the `"(unmapped)"` token appears.

**Two new files inside `[ExcludeFromCodeCoverage]` types.** `RibbonController.EngineCommands.cs` and `RibbonViewer.EngineCommands.cs` are partials of types already exempt at type level, so they inherit the exemption without adding a new attribute. Their content is null checks plus single delegating calls; every decision they touch is made elsewhere and covered at 100%. This is the ratified COM/VSTO exemption applied to thin wiring only, which is the distinction the policy requires.

**Ready-path preservation.** Compared the eight handlers against the merge-base line by line. Each relocated expression is character-for-character identical inside the lambda — for example `Controller.SB.TrainAsync(Controller.OlSelection, true)` and `_controller.Triage.OlLogic.TrainSelectionAsync("A")`, and the `TestSpam_Click` cast-and-index expression. Only the `await` moved outward to the runner. Behaviour once engines are loaded is unchanged.

### Observations that are not findings

`TestSpam_Click` remains the one handler using a dictionary indexer rather than `TryGetValue`. Inside the gated lambda this is now safe, because the lambda body is unreachable while the key is absent. Changing it to `TryGetValue` would alter the ready-path expression and violate the preservation requirement, so leaving it is correct for this fix.

The commented-out legacy `TriageSet*_Click` lines were carried across in the region relocation. They were already present at the merge-base; removing them is unrelated cleanup that a bug fix should not absorb.

## Verdict

**APPROVE.** No blocking or high-severity findings. The change is minimal and targeted, the fix mechanism is sound, the test suite is deterministic and genuinely non-vacuous after F1, and the two prior-cycle findings were resolved correctly — one by remediation and one by a properly evidenced escalation.

The remaining pre-merge obligation is not a code defect: AC19, AC20, and AC21 require a live Outlook profile and must be executed by the maintainer against the checklist already prepared in the feature folder.
