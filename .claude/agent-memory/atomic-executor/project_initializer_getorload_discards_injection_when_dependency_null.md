---
name: initializer-getorload-discards-injection-when-dependency-null
description: UtilitiesCS Initializer.GetOrLoad's dependency overload returns default(T) — not the injected value — when any dependency is null, so a test that injects through a public setter silently reads back nulls
metadata:
  type: project
---

When a property getter routes through
`Initializer.GetOrLoad(ref field, loader, callbackOnSet, strict, dependencies)`, injecting a value
through the property's public setter is **discarded** unless every `dependencies` argument is
non-null.

**Why:** That overload is `if (DependenciesNotNull(strict, dependencies)) { ... } else { return
default(T); }`. It returns `default(T)` rather than the stored field, so with a null dependency the
getter reports a fresh empty value and the injection is invisible. Encountered on 2026-08-26 while
writing COM-free tests for `ConversationResolver.ConversationItems`, whose getter passes the
resolver's own `_mailItem` as the dependency. Constructing
`new ConversationResolver(null, null)` and then assigning `ConversationItems` reads back a
`Pair<IList<MailItem>>` of two nulls, and the test fails for a reason that has nothing to do with
the code under test.

**How to apply:**
- Before injecting through a setter on a `UtilitiesCS` lazy property, read the getter and check
  which overload of `GetOrLoad` it uses. Any overload taking `params object[] dependencies` has
  this behaviour.
- Supply a non-null stand-in for every dependency. For `ConversationResolver` that means
  `new ConversationResolver(null, someMockedMailItem)` — the globals argument is not a dependency
  of that particular getter, the mail item is.
- With a non-null dependency the call falls through to
  `GetOrLoad(ref variable, loader, callbackOnSet)`, which compares the field against `default(T)`
  and returns the injected value without invoking the loader. `Pair<T>` is a struct with no
  `Equals` override, so the comparison is `ValueType.Equals` field-by-field; a pair holding real
  lists is correctly treated as initialized.

Related: [[project_qfcitemcontroller_pump_harness_needs_saveparameters]] — the same class of
failure, where a reflection-injected field leaves a lazily-defaulted collaborator null.
