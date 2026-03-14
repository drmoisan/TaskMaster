#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Codex Web copies this script to /tmp before running it, so the script-relative
# REPO_ROOT resolves to / rather than the actual checkout.  Fall back to the
# working directory, which Codex sets to the repo root.
if [ ! -f "${REPO_ROOT}/TaskMaster.sln" ]; then
  REPO_ROOT="$(pwd)"
fi

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
    lsb-release \
    mono-complete \
    ripgrep \
    unzip \
    zip
}

install_powershell() {
  if command -v pwsh >/dev/null 2>&1; then
    log "PowerShell is already available; skipping."
    return
  fi

  if ! command -v apt-get >/dev/null 2>&1; then
    warn "apt-get is unavailable; skipping PowerShell installation."
    return
  fi

  local runner=""
  if [ "$(id -u)" -ne 0 ]; then
    if command -v sudo >/dev/null 2>&1; then
      runner="sudo"
    else
      warn "PowerShell installation requires root; skipping."
      return
    fi
  fi

  local ubuntu_version
  ubuntu_version="$(lsb_release -rs 2>/dev/null || echo '24.04')"

  log "Registering Microsoft package repository for PowerShell (Ubuntu ${ubuntu_version})..."
  local ms_pkg
  ms_pkg="$(mktemp --suffix=.deb)"
  if curl -fsSL "https://packages.microsoft.com/config/ubuntu/${ubuntu_version}/packages-microsoft-prod.deb" -o "${ms_pkg}"; then
    ${runner} dpkg -i "${ms_pkg}" || true
    rm -f "${ms_pkg}"
    ${runner} apt-get update
    ${runner} apt-get install -y powershell
  else
    rm -f "${ms_pkg}"
    warn "Could not download Microsoft package repo; skipping PowerShell installation."
  fi
}

install_nuget() {
  if command -v nuget >/dev/null 2>&1; then
    log "nuget is already available; skipping."
    return
  fi

  if ! command -v mono >/dev/null 2>&1; then
    warn "mono is unavailable; cannot install nuget wrapper."
    return
  fi

  log "Downloading nuget.exe and creating mono wrapper..."
  local nuget_exe="/usr/local/bin/nuget.exe"
  local nuget_wrapper="/usr/local/bin/nuget"

  local runner=""
  if [ "$(id -u)" -ne 0 ]; then
    runner="sudo"
  fi

  if curl -fsSL "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -o "${nuget_exe}"; then
    printf '#!/usr/bin/env bash\nexec mono %s "$@"\n' "${nuget_exe}" | ${runner} tee "${nuget_wrapper}" >/dev/null
    ${runner} chmod +x "${nuget_wrapper}"
    log "nuget wrapper installed at ${nuget_wrapper}."
  else
    warn "Could not download nuget.exe; skipping."
  fi
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
  install_powershell
  install_nuget
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
