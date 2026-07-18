"""Unit tests for the stale ``app.config`` binding-redirect fix script (issue #354).

Purpose:
    Exercises `fix_binding_redirects.py` at the unit level: stale redirect
    correction, idempotency, missing-file skip behavior, project discovery
    filtering, reference-version extraction, version-tuple ordering, and the
    end-to-end `apply_fixes` composition.

Usage:
    Run with pytest from the repository root, e.g.:

        pytest docs/features/active/2026-07-18-stale-app-config-binding-redirects-354/tests/scripts/test_fix_binding_redirects.py -v

Key invariants:
    - No test creates or reads real temporary files; all file I/O is
      simulated via `monkeypatch` on `builtins.open` and `glob.glob`, backed
      by in-memory `io.StringIO` buffers.
    - The production script is loaded via `importlib.util.spec_from_file_location`
      against its repo-relative path, without mutating `sys.path`.
"""

from __future__ import annotations

import glob
import importlib.util
import io
import pathlib
import types

import pytest


def _load_fix_binding_redirects_module() -> types.ModuleType:
    """Load the durable `fix_binding_redirects.py` script as an importable module.

    Returns:
        The executed module object, loaded directly from its file location so
        the test suite does not need to mutate `sys.path` or depend on the
        feature folder being an importable package.
    """
    module_path = (
        pathlib.Path(__file__).resolve().parents[2]
        / "scripts"
        / "fix_binding_redirects.py"
    )
    spec = importlib.util.spec_from_file_location("fix_binding_redirects", module_path)
    assert spec is not None and spec.loader is not None
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


fix_binding_redirects = _load_fix_binding_redirects_module()


class _CapturingStringIO(io.StringIO):
    """An in-memory text buffer that records its final contents into a dict on close.

    Used to simulate a real file write target for `open(path, "w", ...)`
    calls under test without creating any file on the local filesystem.
    """

    def __init__(self, sink: dict[str, str], key: str) -> None:
        """Initialize the buffer.

        Args:
            sink: The dict that the buffer's final contents are recorded into.
            key: The key under which to record the buffer's contents in `sink`.
        """
        super().__init__()
        self._sink = sink
        self._key = key

    def close(self) -> None:
        """Record the buffer's contents into the sink dict, then close normally."""
        self._sink[self._key] = self.getvalue()
        super().close()


def test_correct_binding_redirects_corrects_stale_entry() -> None:
    """A stale bindingRedirect (newVersion below the referenced assembly version)
    is rewritten to match the real referenced version.
    """
    app_config_text = (
        "<dependentAssembly>\n"
        '  <assemblyIdentity name="Newtonsoft.Json"\n'
        '                     publicKeyToken="30ad4fe6b2a6aeed"\n'
        '                     culture="neutral"\n'
        "  />\n"
        '  <bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="12.0.0.0" />\n'
        "</dependentAssembly>\n"
    )
    real_versions = {"Newtonsoft.Json": ("13.0.0.0", "30ad4fe6b2a6aeed")}

    corrected_text, changes = fix_binding_redirects.correct_binding_redirects(
        app_config_text, real_versions
    )

    assert 'oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0"' in corrected_text
    assert changes == ["app.config Newtonsoft.Json bindingRedirect -> 13.0.0.0"]


def test_correct_binding_redirects_leaves_already_correct_entry_unchanged() -> None:
    """An already-correct bindingRedirect (newVersion already equal to the real
    version) is left byte-for-byte unchanged and produces no change entries
    (the idempotency contract).
    """
    app_config_text = (
        "<dependentAssembly>\n"
        '  <assemblyIdentity name="Newtonsoft.Json"\n'
        '                     publicKeyToken="30ad4fe6b2a6aeed"\n'
        '                     culture="neutral"\n'
        "  />\n"
        '  <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />\n'
        "</dependentAssembly>\n"
    )
    real_versions = {"Newtonsoft.Json": ("13.0.0.0", "30ad4fe6b2a6aeed")}

    corrected_text, changes = fix_binding_redirects.correct_binding_redirects(
        app_config_text, real_versions
    )

    assert corrected_text == app_config_text
    assert changes == []


def test_load_project_config_texts_returns_none_when_app_config_missing(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """`load_project_config_texts` returns None (rather than raising) when the
    app.config file is missing, even though the .csproj file exists.
    """

    def fake_open(
        path: str,
        mode: str = "r",
        encoding: str | None = None,
        newline: str | None = None,
    ):
        if path == "Proj.csproj":
            return io.StringIO("<Project></Project>\n")
        if path == "app.config":
            raise FileNotFoundError(path)
        raise AssertionError(f"unexpected open() call: {path!r}")

    monkeypatch.setattr("builtins.open", fake_open)

    result = fix_binding_redirects.load_project_config_texts(
        "Proj.csproj", "app.config"
    )

    assert result is None


def test_load_project_config_texts_returns_none_when_csproj_missing(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """`load_project_config_texts` returns None (rather than raising) when the
    .csproj file is missing, even though app.config exists.
    """

    def fake_open(
        path: str,
        mode: str = "r",
        encoding: str | None = None,
        newline: str | None = None,
    ):
        if path == "Proj.csproj":
            raise FileNotFoundError(path)
        if path == "app.config":
            return io.StringIO("<configuration></configuration>\n")
        raise AssertionError(f"unexpected open() call: {path!r}")

    monkeypatch.setattr("builtins.open", fake_open)

    result = fix_binding_redirects.load_project_config_texts(
        "Proj.csproj", "app.config"
    )

    assert result is None


def test_discover_projects_filters_excluded_projects(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """`discover_projects` returns first-party project directory names while
    filtering out any project listed in `EXCLUDE_PROJECTS`.
    """

    def fake_glob(pattern: str) -> list[str]:
        return [
            "QuickFiler/packages.config",
            "SVGControl/packages.config",
            "SVGControl.Test/packages.config",
            "TaskMaster/packages.config",
        ]

    monkeypatch.setattr(glob, "glob", fake_glob)

    projects = fix_binding_redirects.discover_projects()

    assert sorted(projects) == ["QuickFiler", "TaskMaster"]


def test_find_referenced_versions_parses_csproj_reference_entries() -> None:
    """`find_referenced_versions` extracts the expected {package_id: (version,
    token)} mapping from a representative in-memory .csproj XML snippet.
    """
    csproj_text = (
        "<ItemGroup>\n"
        '  <Reference Include="Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL">\n'
        "    <HintPath>..\\packages\\Newtonsoft.Json.13.0.3\\lib\\net45\\Newtonsoft.Json.dll</HintPath>\n"
        "  </Reference>\n"
        "</ItemGroup>\n"
    )

    result = fix_binding_redirects.find_referenced_versions(csproj_text)

    assert result == {"Newtonsoft.Json": ("13.0.0.0", "30ad4fe6b2a6aeed")}


def test_parse_version_orders_dotted_segments_as_ints() -> None:
    """`parse_version` compares dotted version segments numerically, guarding
    against lexicographic string-comparison bugs (e.g. "10" < "9" as strings).
    """
    assert fix_binding_redirects.parse_version(
        "6.0.10.0"
    ) > fix_binding_redirects.parse_version("6.0.9.0")


def test_apply_fixes_corrects_one_project_and_skips_project_missing_app_config(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """`apply_fixes` corrects a stale redirect in one project, writes the
    corrected app.config back, and skips a second project that is missing
    its app.config file without raising or reporting a change for it.
    """

    def fake_glob(pattern: str) -> list[str]:
        return ["ProjA/packages.config", "ProjB/packages.config"]

    monkeypatch.setattr(glob, "glob", fake_glob)

    proja_csproj_text = (
        '<Reference Include="Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed">\n'
        "</Reference>\n"
    )
    proja_app_config_text = (
        "<dependentAssembly>\n"
        '  <assemblyIdentity name="Newtonsoft.Json"\n'
        '                     publicKeyToken="30ad4fe6b2a6aeed"\n'
        '                     culture="neutral"\n'
        "  />\n"
        '  <bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="12.0.0.0" />\n'
        "</dependentAssembly>\n"
    )
    projb_csproj_text = "<Project></Project>\n"

    written: dict[str, str] = {}

    def fake_open(
        path: str,
        mode: str = "r",
        encoding: str | None = None,
        newline: str | None = None,
    ):
        if path == "ProjA/ProjA.csproj":
            return io.StringIO(proja_csproj_text)
        if path == "ProjA/app.config" and mode == "r":
            return io.StringIO(proja_app_config_text)
        if path == "ProjA/app.config" and mode == "w":
            return _CapturingStringIO(written, "ProjA/app.config")
        if path == "ProjB/ProjB.csproj":
            return io.StringIO(projb_csproj_text)
        if path == "ProjB/app.config":
            raise FileNotFoundError(path)
        raise AssertionError(f"unexpected open() call: path={path!r} mode={mode!r}")

    monkeypatch.setattr("builtins.open", fake_open)

    report = fix_binding_redirects.apply_fixes()

    assert report == ["ProjA: app.config Newtonsoft.Json bindingRedirect -> 13.0.0.0"]
    assert 'newVersion="13.0.0.0"' in written["ProjA/app.config"]
