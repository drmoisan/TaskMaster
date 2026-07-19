---
name: 371-outlookobjects-nullable-lessons
description: Non-obvious lessons from #371 (utilitiescs-nullable-outlook-mailitem-item, 30-file OutlookObjects cluster, Wave-1 child of the nullable epic) — public-signature cascade to out-of-scope nullable-enabled files, lazy-field CS8618, ToLazy class constraint, GetOrLoad ref-match, ForEach grep gate.
metadata:
  type: project
---

Executing the OutlookObjects nullable-remediation child (#371, 30 files across MailItem/Item/Conversation/Attachment/Table) on a branch off the epic integration tip.

**Why:** these patterns cost real iteration; capturing them makes future epic children (and re-runs) faster.

**How to apply:**

1. **Per-batch, check TOTAL UtilitiesCS CS86xx, not just in-scope.** Making a public member's signature nullable (e.g. `OlTableExtensions.ETL` tuple → `(object[,]?, Dict?)`, `GetTableInViewAsync` → `Task<Outlook.Table?>`, `EtlPrepAsync` tuple slots) REGRESSES OTHER already-nullable-enabled files in the SAME assembly that consume it — here `Extensions/DfDeedle.cs` + `DfDeedle.FrameUtilities.cs` (7 CS86xx). The isolated `UtilitiesCS.csproj /p:BuildProjectReferences=false` build DOES surface these (DfDeedle is in UtilitiesCS), but only if you grep TOTAL `warning CS86`, not a path-filtered in-scope count. Fix per spec's "public signatures stay behavior-compatible": keep the public tuple/return NON-null and put a justified `!` at the internal null-producing sites (`return (null!, null!)`, `return (data!, columnDictionary)`, `return table!`). Do NOT edit the out-of-scope consumer.

2. **Lazy-field partial-class CS8618 (125 at once in MailItemHelper).** Backing `Lazy<T>`/`LazyTry<T>` fields set in a separate `InitLazyFields`/`InitializeSafeDefaults` method → the compiler can't see through it and net481 has no `[MemberNotNull]`. Fix = make the fields `Lazy<T>?` (the getters already use `_x?.Value ?? default`). The 4 lazy props with NO `?? fallback` (Sender/FolderInfo/AttachmentsInfo/Globals) become the nullable public contract with `value?.ToLazy()` setters.

3. **`ToLazy<T>`/`ToLazyValue<T>` have `where T : class`** — you cannot do `value?.ToLazy<byte[]?>()` / `ToLazy` on a nullable-value-typed target (CS8634). Rebuild the setter as an explicit `new Lazy<byte[]?>(() => value)` (behavior-identical null→null).

4. **`_item = null!` deferred-init** so `ref _item` matches `Initializer.GetOrLoad<MailItem>` (its strict overload infers `T=MailItem` from the `Func<MailItem>` loader; a `MailItem?` field would break the `ref` nullability). `ResolveMail` then returns `MailItem?` (GetOrLoad's `T?` contract).

5. **`IEnumerableExtensions.ForEach<T>` is COMMENTED OUT** but `string[].ForEach(...)` in ConversationHelper.Formatting.cs still compiles (resolves via a referenced assembly, oblivious). The P8-T1 grep gate is a FLAG not a block — the build is the authority; Batch H built CS86xx-clean.

6. **Solution TWAE gate still halts on vendored SVGControl CS0649** (unchanged from #363/#364); authoritative in-scope CS86xx = isolated `UtilitiesCS.csproj /t:Rebuild /p:BuildProjectReferences=false` (no TWAE) grep `warning CS86`. Restore SVGControl.dll (build it without TWAE) first if a prior solution `/t:Rebuild` wiped it. See [[project_nullable_pragma_gate_mechanics]] and [[project_364_nullable_gate_preexisting_blockers]].

7. Coverage baseline/final: scope `Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test` (4511 tests, ~25s; coverage.config handles Deedle instrumentation). In-scope OutlookObjects production line coverage held flat at 87.07% (annotation-only). csharpier is global v1.3.0 (`csharpier check .`, no repo-local sdk). Perl multi-line `\Q\E` patterns fail on the repo's CRLF files — use single-line patterns or `\r?\n`.
