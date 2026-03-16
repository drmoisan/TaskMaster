#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

log() {
  printf '[codex-web-setup] %s\n' "$*"
}

warn() {
  printf '[codex-web-setup] warning: %s\n' "$*" >&2
}

append_if_missing() {
  local file="$1"
  local line="$2"

  touch "$file"
  if ! grep -Fqx "$line" "$file"; then
    printf '%s\n' "$line" >>"$file"
  fi
}

install_apt_packages() {
  if ! command -v apt-get >/dev/null 2>&1; then
    warn "apt-get is unavailable; skipping OS package installation."
    return
  fi

  local runner=""
  if [ "$(id -u)" -eq 0 ]; then
    runner=""
  elif command -v sudo >/dev/null 2>&1; then
    runner="sudo"
  else
    warn "apt-get requires root and sudo is unavailable; skipping OS package installation."
    return
  fi

  log "Installing Codex Web dependencies with apt-get..."
  ${runner} apt-get update
  ${runner} apt-get install -y \
    ca-certificates \
    curl \
    git \
    jq \
    mono-complete \
    nuget \
    powershell \
    ripgrep \
    unzip \
    zip
}

install_dotnet_sdk() {
  local dotnet_root="${HOME}/.dotnet"

  if ! command -v dotnet >/dev/null 2>&1; then
    log "Installing .NET SDK into ${dotnet_root}..."
    local install_script
    install_script="$(mktemp)"
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o "${install_script}"
    bash "${install_script}" --channel 8.0 --install-dir "${dotnet_root}"
    rm -f "${install_script}"
  fi

  export DOTNET_ROOT="${dotnet_root}"
  export PATH="${dotnet_root}:${dotnet_root}/tools:${PATH}"

  append_if_missing "${HOME}/.bashrc" 'export DOTNET_ROOT="$HOME/.dotnet"'
  append_if_missing "${HOME}/.bashrc" 'export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"'
}

install_dotnet_tools() {
  if ! command -v dotnet >/dev/null 2>&1; then
    warn "dotnet is unavailable; skipping global tool installation."
    return
  fi

  log "Installing dotnet global tools used by the repo..."
  dotnet tool update --global dotnet-coverage >/dev/null 2>&1 || dotnet tool install --global dotnet-coverage
}

restore_packages_if_needed() {
  cd "${REPO_ROOT}"

  if [ -d "${REPO_ROOT}/packages" ] && [ -n "$(find "${REPO_ROOT}/packages" -mindepth 1 -maxdepth 1 -print -quit)" ]; then
    log "packages/ is already populated; skipping restore."
    return
  fi

  if ! command -v nuget >/dev/null 2>&1; then
    warn "nuget is unavailable; cannot restore packages.config dependencies."
    return
  fi

  log "Restoring solution packages into packages/..."
  nuget restore "${REPO_ROOT}/TaskMaster.sln" -PackagesDirectory "${REPO_ROOT}/packages"
}

write_repo_notes() {
  cat <<'EOF'

Workspace profile detected:
- Legacy Visual Studio solution targeting .NET Framework 4.8.1
- 23 classic .csproj/.vbproj projects and 16 packages.config files
- Windows-first build/test scripts that rely on VS tools such as vswhere, MSBuild.exe, and vstest.console.exe
- Outlook interop and VSTO references across the main add-in and supporting libraries

Codex Web caveat:
- This script reproduces the useful Linux-side editing and restore environment.
- It does not make Outlook, Office PIAs, VSTO runtime, or full Visual Studio/MSBuild parity available on Codex Web.
- Full add-in build/debug parity still requires Windows with Visual Studio 2022, Office/VSTO tooling, and Outlook desktop.

Useful follow-up commands in Codex Web:
- source ~/.bashrc
- dotnet --info
- nuget restore TaskMaster.sln -PackagesDirectory packages
- rg "TODO|FIXME" .

Best-effort build experiments:
- export CI=true
- xbuild /p:Configuration=Debug /p:Platform="Any CPU" TaskMaster.sln

Note:
- The repo's existing PowerShell helper scripts under scripts/vscode/ are Windows/Visual Studio oriented and are not expected to work unchanged in Codex Web.
EOF
}

main() {
  log "Bootstrapping a Codex Web environment for ${REPO_ROOT}"

  export CI=true
  export DOTNET_CLI_TELEMETRY_OPTOUT=1
  export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
  export NUGET_XMLDOC_MODE=skip

  append_if_missing "${HOME}/.bashrc" 'export CI=true'
  append_if_missing "${HOME}/.bashrc" 'export DOTNET_CLI_TELEMETRY_OPTOUT=1'
  append_if_missing "${HOME}/.bashrc" 'export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1'
  append_if_missing "${HOME}/.bashrc" 'export NUGET_XMLDOC_MODE=skip'

  install_apt_packages
  install_dotnet_sdk
  install_dotnet_tools
  restore_packages_if_needed

  if command -v git >/dev/null 2>&1; then
    git config --global core.autocrlf input || true
  fi

  write_repo_notes
  log "Setup complete."
}

main "$@"
