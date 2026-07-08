# hierarchical-lcppn-folder-prediction (Issue #177)

- Date captured: 2026-06-08
- Author: Dan Moisan
- Status: Active feature folder created (Issue #177)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub feature issue template.

- Issue: #177
- Issue URL: (to be created at PR time)
- Last Updated: 2026-06-08
- Work Mode: full-feature

## Summary

TaskMaster predicts the destination Outlook folder for an email using a flat Bayesian
classifier: `BayesianClassifierGroup` holds one independent `BayesianClassifierShared`
per leaf-folder tag and scores every leaf independently (chi-squared probability) with no
awareness that sibling folders share a parent in the folder tree. This treats every leaf
as an unrelated class, which discards the hierarchical structure of the folder tree and
limits leaf-level prediction quality.

This feature introduces a hierarchy-aware predictor based on the **Local Classifier Per
Parent Node (LCPPN)** strategy: one multiclass decision per internal folder node selecting
among that node's children, with leaf probability expressed as the product of conditional
probabilities along the root-to-leaf path. The goal is higher leaf-level F1 while
preserving the existing incremental-update property (count-based, no full retrain) and the
existing "all-or-nothing" abstention behavior.

The two vision documents (`docs/LCPPN_doc1.md`, `docs/LCPPN_doc2.md`) survey the approach
in scikit-learn terms. TaskMaster is a C#/.NET VSTO add-in, so the implementation must be a
native C# design that reuses the existing token-count infrastructure rather than adopting
Python tooling. The documents' author could not access the repository; this issue grounds
the request in the actual codebase.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Language/runtime: C# / .NET (VSTO Outlook add-in), MSTest + Moq + FluentAssertions
- Relevant modules: `UtilitiesCS/EmailIntelligence/Bayesian/`,
  `UtilitiesCS/EmailIntelligence/ClassifierGroups/`
- Data source or fixture: Mined `MinedMailInfo` corpus with `FolderInfo.RelativePath` as the
  hierarchical folder label

## Current Behavior

- `BayesianClassifierGroup.Classify(...)` scores every leaf tag in `Classifiers` in parallel
  and returns predictions ordered by probability (flat, all-vs-all).
- `OlFolderClassifierGroup.BuildFolderClassifiersAsync` groups the corpus by
  `FolderInfo.RelativePath` (a delimited hierarchical path) and builds one classifier per
  full path, ignoring intermediate path segments.
- Incremental update (`Train`/`TrainMultiTag`) updates per-tag token counts and a shared
  token base. Updates touch the single flat tag, not a path of parent nodes.
- Sibling relationships are not modeled; distant leaves compete directly with near siblings.

## Desired Behavior

- Folder labels are parsed into a parent→children tree from `FolderInfo.RelativePath`.
- Each internal node owns a multiclass classifier over its direct children.
- Prediction descends from the root, combining per-step probabilities along the path
  (`P(leaf|x) = ∏ P(child|parent, x)`), using beam search over path log-probabilities rather
  than pure greedy descent so a single uncertain early decision does not lock out the correct
  branch.
- The existing "all-or-nothing" threshold is applied at the path level (and/or per-parent for
  early-stop abstention), preserving current abstention semantics.
- Incremental updates remain count-based and localized: a user correction updates only the
  classifiers along the corrected root-to-leaf path. Adding a new leaf updates only its
  parent's child set, not a global K-way model.
- Backward compatibility: the existing flat predictor and its serialized models remain usable;
  the hierarchical predictor is introduced behind a seam/config so standard behavior is not
  broken.

## Candidate Approaches (to be evaluated in research)

1. LCPPN with hierarchical-shrinkage Naive Bayes per parent (parent-informed smoothing) —
   maximum reuse of existing count-based machinery, preserves pure incremental updates.
2. LCPPN with NBSVM-style per-parent decision (NB log-count-ratio features feeding a linear
   model) — keeps incremental counts, stronger decision boundary.
3. LCPPN with an online linear/logistic per-parent learner — likely highest F1 where data per
   sibling set is moderate; requires an online-update mechanism in C#.

Research must recommend one mainline approach grounded in the existing C# types and the
incremental-update and abstention constraints, plus a cold-start fallback for sparse nodes.

## Impact / Severity

- [ ] Blocker
- [x] High (core prediction quality)
- [ ] Medium
- [ ] Low

## Acceptance Criteria (draft — finalized in user-story.md)

- [ ] Folder labels are parsed into a parent→children hierarchy from `FolderInfo.RelativePath`.
- [ ] An LCPPN predictor descends the tree and returns a leaf prediction with a path-product
      probability and ordered alternatives.
- [ ] Beam search over path probabilities is implemented and configurable.
- [ ] Abstention threshold semantics are preserved and documented for F1 accounting.
- [ ] Incremental update of a single corrected example touches only classifiers on the true path.
- [ ] Adding a new leaf folder updates only the affected parent classifier.
- [ ] Existing flat predictor and serialized models remain functional (no breaking change to the
      current path).
- [ ] Unit coverage meets repository policy (>= 90% for new modules/classes); deterministic,
      no temp files, MSTest + Moq + FluentAssertions.
- [ ] Full C# toolchain passes: CSharpier → .NET analyzers → nullable → MSTest.

## Next Step

- [ ] Deep research (task-researcher) grounding approach in the C# codebase
- [ ] Feature documents (spec.md, user-story.md) finalized
- [ ] Atomic implementation plan generated and preflight-cleared
