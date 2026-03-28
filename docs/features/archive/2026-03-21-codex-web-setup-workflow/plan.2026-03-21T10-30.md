# 2026-03-21-codex-web-setup-workflow (Plan)

- **Issue:** none
- **Branch:** current working branch
- **Owner:** drmoisan
- **Last Updated:** 2026-03-21T20-55
- **Status:** Complete
- **Version:** 1.0
- **Work Mode:** minor-change
- **Requirements Source:** user request in Codex session on 2026-03-21

## Overview

Add a dedicated GitHub Actions workflow under `.github/workflows` that can be started manually and validates `.codex/codex-web-setup.sh`. The workflow should check shell syntax, run shell linting, and execute the script under a lightweight harness that confirms the expected Linux failure mode without performing the full environment bootstrap.

## Tasks

- [x] Read the current workflow structure and the target shell script.
- [x] Review the active repository plan document and repository workflow policy.
- [x] Add a manually triggered workflow for `.codex/codex-web-setup.sh`.
- [x] Validate the new workflow with `actionlint`.
- [x] Update this plan with completion status.
