# Pre-Change State Baseline

- Timestamp: 2026-07-16T15-56
- Issue: #340

## `.github/dependabot.yml` existence check

- Command: `Test-Path .github/dependabot.yml`
- EXIT_CODE: 0
- Output Summary: `False` (file does not exist)

## README.md baseline

- Command: read `README.md` `## Contents` list and the section ordering around `## Configuration & storage` / `## Common issues`.
- EXIT_CODE: 0
- Output Summary:

Current `## Contents` list (verbatim, lines 11-19 of `README.md`):

```
* [Features](#features)
* [Solution layout](#solution-layout)
* [Getting started](#getting-started)
* [Build & debug (VSTO add-in)](#build--debug-vsto-add-in)
* [Running the tests](#running-the-tests)
* [Configuration & storage](#configuration--storage)
* [Common issues](#common-issues)
* [Contributing & branches](#contributing--branches)
* [License](#license)
```

There is no `Dependency updates (Dependabot)` entry in the `## Contents` list.

Section adjacency confirmed: `## Configuration & storage` (README.md line 142) is immediately followed (after its closing `---` divider at line 154) by `## Common issues` (README.md line 156), with no section in between.
