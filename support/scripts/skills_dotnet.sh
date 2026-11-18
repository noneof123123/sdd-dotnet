#!/usr/bin/env bash
set -euo pipefail

CLEAN_EXISTING=0

for arg in "$@"; do
  case "$arg" in
    --clean-existing|-CleanExisting)
      CLEAN_EXISTING=1
      ;;
    *)
      printf 'Unknown option: %s\n' "$arg" >&2
      printf 'Usage: %s [--clean-existing]\n' "${0##*/}" >&2
      exit 1
      ;;
  esac
done

if ! command -v git >/dev/null 2>&1; then
  printf 'Git is not installed or is not available in PATH.\n' >&2
  printf 'Install Git first: https://git-scm.com/downloads\n' >&2
  exit 1
fi

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
SKILLS_DESTINATION_ROOT="$PROJECT_ROOT/.github/skills"

printf '\n'
printf 'Installing Microsoft .NET Skills...\n'
printf '\n'
printf 'Project root: %s\n' "$PROJECT_ROOT"
printf 'Skills destination: %s\n' "$SKILLS_DESTINATION_ROOT"
printf '\n'

mkdir -p -- "$SKILLS_DESTINATION_ROOT"

SKILLS_REPO="$(mktemp -d "${TMPDIR:-/tmp}/dotnet-skills.XXXXXX")"
trap 'rm -rf -- "$SKILLS_REPO"' EXIT

printf 'Cloning official Microsoft .NET skills repository...\n'
if ! git clone --depth 1 https://github.com/dotnet/skills "$SKILLS_REPO"; then
  printf 'Failed to clone dotnet/skills repository.\n' >&2
  exit 1
fi

if [[ ! -d "$SKILLS_REPO/.git" ]]; then
  printf 'Failed to clone dotnet/skills repository.\n' >&2
  exit 1
fi

if [[ "$CLEAN_EXISTING" -eq 1 ]]; then
  printf '\n'
  printf 'Cleaning existing .github/skills content...\n'
  find "$SKILLS_DESTINATION_ROOT" -mindepth 1 -maxdepth 1 -exec rm -rf -- {} +
fi

SKILL_NAMES=(
  "dotnet-webapi"
  "create-blazor-project"
  "author-component"
  "fetch-and-send-data"
  "optimizing-ef-core-queries"
  "run-tests"
  "dotnet-test-frameworks"
)

SKILL_SOURCES=(
  "plugins/dotnet-aspnetcore/skills/dotnet-webapi"
  "plugins/dotnet-blazor/skills/create-blazor-project"
  "plugins/dotnet-blazor/skills/author-component"
  "plugins/dotnet-blazor/skills/fetch-and-send-data"
  "plugins/dotnet-data/skills/optimizing-ef-core-queries"
  "plugins/dotnet-test/skills/run-tests"
  "plugins/dotnet-test/skills/dotnet-test-frameworks"
)

printf '\n'
printf 'Copying selected .NET skills...\n'
printf '\n'

COPIED=0
SKIPPED=0

for i in "${!SKILL_NAMES[@]}"; do
  name="${SKILL_NAMES[$i]}"
  source_path="$SKILLS_REPO/${SKILL_SOURCES[$i]}"
  destination_path="$SKILLS_DESTINATION_ROOT/$name"

  if [[ ! -d "$source_path" ]]; then
    printf 'Skipped: %s - source folder not found: %s\n' "$name" "$source_path"
    SKIPPED=$((SKIPPED + 1))
    continue
  fi

  if [[ ! -f "$source_path/SKILL.md" ]]; then
    printf 'Skipped: %s - SKILL.md not found.\n' "$name"
    SKIPPED=$((SKIPPED + 1))
    continue
  fi

  if [[ -e "$destination_path" ]]; then
    rm -rf -- "$destination_path"
  fi

  cp -a -- "$source_path" "$destination_path"
  printf 'Copied: %s\n' "$name"
  COPIED=$((COPIED + 1))
done

printf '\n'
printf 'Installation completed.\n'
printf 'Copied skills: %s\n' "$COPIED"
printf 'Skipped skills: %s\n' "$SKIPPED"

printf '\n'
printf 'Installed skills location:\n'
printf '%s\n' "$SKILLS_DESTINATION_ROOT"

printf '\n'
printf 'Next steps:\n'
printf '1. Commit the .github/skills folder to your repository.\n'
printf '2. Restart VS Code or run: Developer: Reload Window.\n'
printf '3. In Copilot Chat Agent mode, ask Copilot to use the relevant .NET skill.\n'

printf '\n'
printf 'Example prompts:\n'
printf '%s\n' 'Use the dotnet-webapi skill to create a Minimal API endpoint.'
printf '%s\n' 'Use the author-component skill to create a Blazor component.'
printf '%s\n' 'Use the run-tests skill to execute and fix the test suite.'

printf '\n'
