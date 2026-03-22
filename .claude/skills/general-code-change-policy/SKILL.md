---
name: general-code-change-policy
description: 'Baseline rules that apply to any code change in this repo. Use before implementing any code, tests, tasks, or scripts.'
user-invocable: false
---

# Agent Code Change Policy

**CRITICAL**: When implementing any code, tests, tasks, or scripts, you **must** adhere to these repo policies **without exception**. This includes but is not limited to adding, removing, or changing any code, tasks, scripts, modules, packages, tests or their components.

Read each policy document **thoroughly** before starting work. Implement them **exactly as written**. Do not interpret, modify, or skip any requirements. If you encounter **any** conflicting instructions, halt and notify the user.

Language-specific standards (e.g. for C#) are defined in additional skill files and **layer on top of** this general policy.

**Reading order / authority:** Apply this general policy first, then any language-specific code-change skills, then any unit-test addenda. Operational guidance (e.g., developer tooling, CI docs) sits underneath these policies.

## Before Making Changes

- [ ] Clarify the objective. Begin reasoning from clearly stated assumptions or axioms.
- [ ] Read existing change plans (e.g., `change-plan.md`).
- [ ] Document the plan to make changes. If it is part of an existing change plan, make any relevant updates to the plan before executing.

---

## Bugfix Workflow (all languages, defects only)

Use this workflow only when addressing a bug or defect. Feature work, refactors, and
new capabilities should follow the general planning steps and design principles
rather than this bugfix sequence.

1. **Create a failing regression test first**
   - Add the smallest deterministic test that reproduces the bug using the project's standard test layout.
   - Ensure the test fails before the fix and will pass after; avoid external services or temporary files.

2. **Implement the minimal, targeted fix**
   - Change only what is needed to make the failing test pass; keep boundaries intact and avoid opportunistic refactors.
   - If you uncover deeper design problems, open a new issue instead of widening scope.

3. **Verify locally before review**
   - Re-run the original repro and the new regression test.
   - Run the full toolchain in order (format → lint → type-check → test) using the repo-standard commands or tasks; rerun from the start if any step changes files or fails.

---

## 1. Design Principles

High-level design priorities (applies to all languages):

1. **Simplicity first** — Prefer the simplest design that works and is easy to read. Avoid cleverness and deep indirection.
2. **Reusability** — Factor out logic that is clearly reusable into small methods or pure functions. Avoid copy-paste.
3. **Extensibility** — Design public APIs so they can be extended without breaking callers.
4. **Separation of concerns** — Keep **pure logic** (transforms, calculations, parsing) separate from I/O (disk, network, DB), UI/CLI, and framework-specific glue.

---

## 2. Classes, Functions, and APIs

**Overall rule:** Use **strongly-typed, well-structured classes** to model domain concepts and workflows. Use **functions** (or equivalent) for small, stateless helpers and glue code.

### 2.1 Prefer classes for domain concepts and workflows

Create a class when at least one is true:
- There is a **clear domain concept** with data + behavior.
- You have **state + invariants** that should travel together.
- You expect **multiple implementations** behind a common interface.
- You are modeling a **multi-step workflow** that shares context.

When you use classes: keep methods small and focused; avoid god objects.

### 2.2 Use functions for small, pure helpers

Create a standalone function when:
- The operation is **pure, stateless, and simple**.
- It is a **small helper** that doesn't naturally belong on a specific domain class.

### 2.3 Interfaces and contracts

- Use interfaces / abstract types / protocols when multiple implementations are likely.
- Public methods and functions must have clear, documented contracts (inputs, outputs, invariants).

---

## 3. Error Handling, Logging, and Contracts

1. **Error handling** — Fail **fast and explicitly**. Don't silently ignore errors or broad-catch unless you immediately re-raise or propagate with added context.
2. **Logging** — Use the project's logging pattern instead of ad-hoc print/console output. Log at appropriate levels and include enough context to debug issues.
3. **Contracts / invariants** — Enforce invariants at construction/initialization time. Use assertions only for **internal sanity checks**, not user-facing error handling.

---

## 4. Module & File Structure

1. Keep modules **cohesive** — A module/file should have a clear purpose. Avoid dumping unrelated classes/functions into the same file. Do not exceed 500 lines for any one file.
2. Make the public surface area **small and intentional**.
3. Prefer clear, explicit imports. Avoid circular dependencies.

---

## 5. Naming, Docs, and Comments

1. Names should be descriptive, not cryptic. Abbreviations are okay only when standard and widely understood.
2. Public classes and methods should have a short description covering what it does, important arguments, and what it returns or side effects.
3. Comment **why**, not what. If you use workarounds or non-obvious patterns, add a short comment explaining the reasoning.

---

## 6. Performance, I/O, and Dependencies

1. Prefer clarity first; optimize only where there is a demonstrated need.
2. Isolate I/O (disk, network, APIs) into specific classes or modules. Core domain logic should be testable **without** touching the network or filesystem. **Use of temporary files within tests is strictly prohibited.**
3. Use only the libraries already approved in the project unless specifically told to add more.

---

## 7. How to Interact with Existing Code

1. Where the repo already has a clear style, **match that style**.
2. Avoid breaking public APIs. If a breaking change is necessary, call it out clearly.
3. Treat existing unit tests as **part of the spec**.

---

## 8. After Making Changes

### 1. Run the full toolchain (no shortcuts)

Run the full toolchain in this exact order and repeat it until everything passes:

1. **Formatting**
2. **Linting**
3. **Type checking**
4. **Testing**

Treat these four steps as one **toolchain pass**. If any step fails or auto-fixes anything, restart from step 1. You **may not stop** this loop while any step is failing.

When you report back, explicitly state which commands you ran and that all four steps passed without errors in the final pass.

### 2. Summarize key changes and rationale

- Summarize the key changes made and how they relate to the original objective.
- Explain any important design choices and other options you considered but did not implement.

### 3. Update supporting documents

- Update any supporting documents (e.g., README, design docs, runbooks).
- Update any workplan, change plan, or instructions document to show progress and reflect the new behavior.

### 4. Provide clear next steps

- Provide clear development next steps (what should happen next, and by whom).
- If development is complete, provide detailed instructions on usage and any operational caveats.
