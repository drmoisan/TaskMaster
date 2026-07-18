"""Corrects stale ``bindingRedirect`` entries in per-project ``app.config`` files.

Purpose:
    This script scans first-party project directories for a ``packages.config``
    marker, then compares each project's ``.csproj`` assembly ``<Reference>``
    entries against the ``bindingRedirect`` entries in that project's
    ``app.config``. Any redirect whose ``newVersion`` is stale relative to the
    version actually referenced by the ``.csproj`` is rewritten in place.

Responsibilities:
    - Discover first-party project directories (excluding vendored/exempt
      projects such as ``SVGControl``).
    - Parse referenced assembly versions and public key tokens out of each
      project's ``.csproj`` file.
    - Parse and correct ``bindingRedirect`` entries in each project's
      ``app.config`` file to match the referenced assembly version.
    - Report every change made, and leave already-correct redirects untouched
      (idempotent behavior).

Usage:
    Run as a script from the repository root:

        python fix_binding_redirects.py

    This invokes ``apply_fixes()`` and prints one line per corrected
    ``bindingRedirect``, followed by a ``TOTAL:`` summary line. The module can
    also be imported without triggering any file I/O or side effects; callers
    that want the behavior programmatically should call ``apply_fixes()``
    directly.

High-level flow:
    1. ``discover_projects`` walks the repository root for ``packages.config``
       files to find first-party project directories.
    2. For each project, ``load_project_config_texts`` reads the ``.csproj``
       and ``app.config`` file contents, skipping the project if either file
       is missing.
    3. ``find_referenced_versions`` extracts the real assembly
       version/public-key-token pairs from the ``.csproj`` text.
    4. ``correct_binding_redirects`` rewrites any stale ``bindingRedirect``
       entries in the ``app.config`` text to match the real versions.
    5. ``apply_fixes`` writes the corrected ``app.config`` back to disk only
       when its contents changed, and accumulates a human-readable report.

Key invariants:
    - An already-correct ``bindingRedirect`` (``newVersion`` already equal to
      the referenced version) is left byte-for-byte unchanged.
    - Only first-party project directories are considered; projects named in
      ``EXCLUDE_PROJECTS`` are always skipped.

Side effects:
    - Reads ``<project>/packages.config``, ``<project>/<project>.csproj``, and
      ``<project>/app.config`` files from disk.
    - Overwrites ``<project>/app.config`` in place when its bindingRedirect
      entries change.
"""

import glob
import os
import re

EXCLUDE_PROJECTS = {"SVGControl", "SVGControl.Test"}

REF_RE = re.compile(
    r'<Reference Include="([\w.]+), Version=([0-9.]+), '
    r'Culture=neutral, PublicKeyToken=([0-9a-fA-F]+)[^"]*">'
)


def parse_version(version: str) -> tuple[int, ...]:
    """Parse a dotted version string into a tuple of ints for ordered comparison.

    Args:
        version: A dotted numeric version string, for example ``"6.0.10.0"``.

    Returns:
        A tuple of ints, one per dot-separated segment, suitable for
        lexicographic tuple comparison that respects numeric ordering (for
        example ``parse_version("6.0.10.0") > parse_version("6.0.9.0")``,
        which a plain string comparison would get wrong).
    """
    return tuple(int(x) for x in version.split("."))


def discover_projects(repo_root: str = ".") -> list[str]:
    """Discover first-party project directory names under ``repo_root``.

    Args:
        repo_root: The directory to search for project directories. Defaults
            to the current working directory.

    Returns:
        A list of project directory names (not full paths) that contain a
        ``packages.config`` file, excluding any project named in
        ``EXCLUDE_PROJECTS``.
    """
    projs: list[str] = []
    # Walk every top-level directory containing a packages.config marker to
    # find first-party project directories, then drop vendored/exempt ones
    # (e.g. SVGControl) that are intentionally excluded from this audit.
    for path in glob.glob(os.path.join(repo_root, "*", "packages.config")):
        proj = os.path.basename(os.path.dirname(path))
        if proj not in EXCLUDE_PROJECTS:
            projs.append(proj)
    return projs


def load_project_config_texts(
    csproj_path: str, app_config_path: str
) -> tuple[str, str] | None:
    """Read a project's ``.csproj`` and ``app.config`` file contents.

    Args:
        csproj_path: Path to the project's ``.csproj`` file.
        app_config_path: Path to the project's ``app.config`` file.

    Returns:
        A ``(csproj_text, app_config_text)`` tuple when both files exist, or
        ``None`` when either file is missing.

    Side effects:
        Reads both files from disk using UTF-8 encoding.
    """
    try:
        with open(csproj_path, encoding="utf-8") as f:
            cs_text = f.read()
        with open(app_config_path, encoding="utf-8") as f:
            app_text = f.read()
    except FileNotFoundError:
        # A project missing either its .csproj or its app.config cannot be
        # audited for binding-redirect drift, so it is skipped rather than
        # treated as an error: not every first-party project directory is
        # guaranteed to carry both files (e.g. a project with no external
        # assembly references may have no app.config at all).
        return None
    return cs_text, app_text


def find_referenced_versions(csproj_text: str) -> dict[str, tuple[str, str]]:
    """Extract referenced assembly versions and tokens from ``.csproj`` text.

    Args:
        csproj_text: The full text contents of a ``.csproj`` file.

    Returns:
        A mapping of package id to ``(assembly_version, public_key_token)``
        for every ``<Reference Include="...">`` entry found.
    """
    real_versions: dict[str, tuple[str, str]] = {}
    # Scan every <Reference> entry in the project file to build the map of
    # what version/token each referenced assembly actually carries.
    for m in REF_RE.finditer(csproj_text):
        pid, asm_ver, token = m.groups()
        real_versions[pid] = (asm_ver, token)
    return real_versions


def correct_binding_redirects(
    app_config_text: str, real_versions: dict[str, tuple[str, str]]
) -> tuple[str, list[str]]:
    """Rewrite stale ``bindingRedirect`` entries to match real assembly versions.

    Args:
        app_config_text: The full text contents of an ``app.config`` file.
        real_versions: Mapping of package id to ``(assembly_version,
            public_key_token)``, as returned by ``find_referenced_versions``.

    Returns:
        A ``(corrected_text, change_descriptions)`` tuple. ``corrected_text``
        is the ``app.config`` text with any stale redirects rewritten (equal
        to the input text when nothing changed). ``change_descriptions`` is a
        list of human-readable strings, one per corrected redirect.
    """
    changes: list[str] = []
    # Walk every referenced package and attempt to correct its matching
    # bindingRedirect block in the app.config text, one package at a time.
    for pid, (real_ver, token) in real_versions.items():
        pattern = re.compile(
            r'(name="'
            + re.escape(pid)
            + r'"\s*\n\s*publicKeyToken="'
            + re.escape(token)
            + r'"\s*\n\s*culture="neutral"\s*\n\s*/>\s*\n\s*'
            + r'<bindingRedirect oldVersion="0\.0\.0\.0-)'
            + r"([0-9.]+)"
            + r'(" newVersion=")'
            + r"([0-9.]+)"
            + r'(")'
        )

        def repl(m2: re.Match[str], real_ver: str = real_ver) -> str:
            """Rewrite one matched bindingRedirect block, or leave it as-is."""
            old_lo, cur_new = m2.group(2), m2.group(4)
            # An already-correct redirect (newVersion already equal to the
            # real referenced version) is left byte-for-byte unchanged: this
            # is the idempotency contract that lets the script be re-run
            # safely without perturbing files that need no change.
            if cur_new == real_ver:
                return m2.group(0)
            new_hi = (
                real_ver if parse_version(real_ver) >= parse_version(old_lo) else old_lo
            )
            return m2.group(1) + new_hi + m2.group(3) + real_ver + m2.group(5)

        new_text, count = pattern.subn(repl, app_config_text)
        if count and new_text != app_config_text:
            app_config_text = new_text
            changes.append(f"app.config {pid} bindingRedirect -> {real_ver}")

    return app_config_text, changes


def apply_fixes(repo_root: str = ".") -> list[str]:
    """Discover projects and correct stale binding redirects across all of them.

    Args:
        repo_root: The directory to search for project directories. Defaults
            to the current working directory.

    Returns:
        A flat list of human-readable change descriptions, one per corrected
        ``bindingRedirect``, prefixed with the owning project name.

    Side effects:
        Overwrites each project's ``app.config`` file on disk when its
        binding redirects changed.
    """
    report: list[str] = []
    # Visit every discovered first-party project and correct its
    # app.config in place when a stale binding redirect is found.
    for proj in discover_projects(repo_root):
        csproj_path = f"{proj}/{proj}.csproj"
        app_cfg_path = f"{proj}/app.config"

        loaded = load_project_config_texts(csproj_path, app_cfg_path)
        if loaded is None:
            continue
        cs_text, app_text = loaded

        real_versions = find_referenced_versions(cs_text)
        corrected_text, changes = correct_binding_redirects(app_text, real_versions)
        report.extend(f"{proj}: {change}" for change in changes)

        if corrected_text != app_text:
            with open(app_cfg_path, "w", encoding="utf-8", newline="") as f:
                f.write(corrected_text)

    return report


if __name__ == "__main__":
    report = apply_fixes()
    for line in report:
        print(line)
    print("TOTAL:", len(report))
