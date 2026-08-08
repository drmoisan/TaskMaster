---
name: iqfcdatamodel-contract-436
description: "#436 F5 IQfcDatamodel.cs: Cobertura emits NO <class> element for any interface or any enum, so declaration-only files are absent (not 0%) from coverage; issue.md's F11-consumer claim is false, real hidden consumers are F2/F6"
metadata:
  type: project
---

Two non-obvious findings from researching `QuickFiler/Interfaces/IQfcDatamodel.cs` (child F5 of epic #136),
verified 2026-08-08 against the committed report
`docs/features/active/2026-08-06-...-424/evidence/qa-gates/coverage-final.cobertura.xml`.

**1. The coverage tool emits no `<class>` element for interfaces or enums — measured, not inferred.**
Searches of that report returned: zero `<class>` elements for any QuickFiler interface type, zero for any
enum type anywhere in 110,849 instrumented lines, and exactly one `filename="QuickFiler\Interfaces\..."`
entry (`MailItemActionsAdapter`, a concrete class). `SortOptionsEnum` appears only as parameter-type text
inside `EmailSorter` method signatures. So a declaration-only C# file is **absent from the per-file report,
not 0%** — it can neither pass nor fail an 80% line gate.

**Why:** epic #136 requires per-file >= 80% or a ratified exemption, and ~24 of the 121 compiled QuickFiler
files are declaration-only. Reporting them as 0% would create ~24 permanently unfixable gate failures.

**How to apply:** when a coverage question involves an interface-only or enum-only file, read the committed
Cobertura artifact rather than speculating, and recommend a third ledger category
`not-measurable (declaration-only)` distinct from both `testable` and `ratified-exempt`. Do **not** route it
through the CLAUDE.md § UT2 COM/VSTO exemption even when the file's signatures mention `MailItem` — that
clause covers classes with behavior, and it would pull in a needless maintainer-ratification requirement plus
an `[ExcludeFromCodeCoverage]` that excludes nothing. See [[committed-cobertura-baselines]].

**2. Indirect consumers hide from a type-name grep.** `issue.md` claimed `IQfcDatamodel` is consumed by F7
and F11. F11 is **false** — `QfcCollectionController.cs` (2,349 lines) has zero matches for
`DataModel|Datamodel|_datamodel`. The two real unanticipated consumers are `QfcQueue.cs:476` (**F2**) and
`QfcFormController.EventHandlers.cs:196` (**F6**), both reaching the contract through
`IQfcHomeController.DataModel` — so a grep for `IQfcDatamodel` never surfaces them.

**How to apply:** for any cross-child interface contract question, grep for the *member names* through the
re-exposing property as well as the interface type name. Also: `IQfcDatamodel` has exactly **one** compiled
implementer (`QfcDatamodel`), and `Mock<T>` proxies are generated at runtime — so adding an interface member
compiles fine but silently makes every sibling's `Mock<IQfcDatamodel>` return `default` for it. That, not
compile breakage, is the reason to prohibit widening. `EfcDataModel` does not implement it. Related:
[[qfc-datamodel-coverage-436]], [[efcdatamodel-coverage-436]], [[qfc-queueprocessing-436]].
