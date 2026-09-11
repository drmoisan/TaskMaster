---
name: backticked-paths-are-the-change-footprint
description: A downstream tool harvests backtick-delimited repository paths from spec.md/plan to derive the change footprint — backtick every in-scope file, leave out-of-scope files unbackticked
metadata:
  type: feedback
---

In feature documents, every repository file the fix will create or modify must appear at least once
as a Markdown inline code span with its full repository-relative path. Conversely, do not backtick a
path the fix will not touch — write out-of-scope paths as bare prose under a clearly labelled
non-goals section. Never use placeholder forms such as `<FEATURE>/...` or `${VAR}/...`.

**Why:** A downstream tool derives the change footprint by harvesting backticked path tokens from
`spec.md` and the plan. A file named only in bare prose is invisible to it, so an in-scope file that
is not backticked silently drops out of the footprint. The same mechanism makes a backticked
out-of-scope path a false positive that widens the apparent blast radius.

**Strictest form (required when the caller says a parallel run's schedulability depends on it, seen
on #736, 2026-09-02):** put a section titled exactly `## Write Set` listing every created/modified/
deleted file as backticked repo-relative paths, one per line, and make that section the *only* place
in the document with a backticked path. Everywhere else — including bare filenames and
`File.cs:123` line citations — write paths as plain prose. Add a "do not fix this formatting"
blockquote near the top so a later editor does not re-add backticks. Exclude the planning document
itself and the timestamp-named research/evidence artifacts from the Write Set. Audit mechanically
before reporting: `Grep` for `` `[^`\n]*(\.cs|\.csproj|\.md|/|\\)[^`\n]*` `` and confirm the only
hits outside the Write Set are code identifiers and the mandated CLAUDE.md msbuild command strings
(those contain spaces, so they are not harvestable tokens). Also grep for `[<>$%]` and confirm every
hit is a C# generic or XML element, never a path placeholder.

**How to apply:** After drafting, sweep the document twice — once to confirm every in-scope file has
at least one backticked occurrence, once to confirm no out-of-scope file is backticked. This bites
hardest in the Root Cause Analysis section, where it is natural to cite comparison files that will
not be modified: keep those citations unbackticked. Add a one-line note in the non-goals section
explaining that the paths there are deliberately unbackticked, so a later editor does not "fix" the
formatting and corrupt the footprint. Related: [[full-bug-spec-only]],
[[ac-gates-verify-satisfiability]].

**The verbatim-AC block is an unavoidable exception (seen on #797, 2026-09-06).** The strictest form
collides with the "copy acceptance criteria verbatim" rule whenever the maintainer wrote inline code
spans into the AC text. On #797 the criteria carried `StoresWrapper.json`,
`%LocalAppData%\TaskMaster\StoresWrapper.json`, `SmartSerializable<T>.Serialize()` and `\\` — tokens
that trip both the backtick sweep and the "no percent sign / no angle brackets" placeholder rule.
Verbatim reproduction wins: keep the spans, and add a sentence to the top blockquote naming the AC
block as the sole exception and classifying each span as a runtime value or a C# member name rather
than a repository path. Do not paraphrase the criteria to dodge the sweep, and do not strip their
backticks. Expect the audit grep to return exactly (Write Set paths + AC lines containing spans).

**The seeded spec template is itself a source of false write claims (seen on #798, 2026-09-07).**
The promotion scaffold copies `issue.md` prose into Context / Repro & Evidence, and that prose
arrives with backticks already around paths that are *not* write claims: the debug-log path under
the add-in bin directory, and any `issue.md` / skill-path reference the header adds. Strip those on
the first pass — they are inside the template, so they are easy to read past. Fenced code blocks
(stack traces, log excerpts) are safe and can keep their paths: an inline-code-span regex does not
match inside a triple-backtick fence. Bare tokens such as `` `.cs` ``, `` `/t:Rebuild` `` and
`` `File.cs:123` `` example strings also trip a path harvester; write them as prose. The mandated
CLAUDE.md msbuild/vstest command strings are safe because they contain spaces.
