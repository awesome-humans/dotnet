# Every local and CI entry point (AGENTS.md: Scripts). CI calls these recipes, never npm or
# dotnet directly, so a check runs the same way on every machine. Run `just` to list them.

set shell := ["bash", "-euo", "pipefail", "-c"]

# List the recipes
default:
    @just --list

# Install the prose tools at the versions package-lock.json pins
install:
    npm ci

# Format Markdown, JSON, JSONC, and YAML
format:
    npm run format

# Check formatting, Markdown lint, and spelling together
check: format-check lint spell

# Check that Prettier leaves Markdown, JSON, JSONC, and YAML unchanged
format-check:
    npm run format:check

# Lint Markdown with markdownlint
lint:
    npm run lint

# Check spelling with cspell
spell:
    npm run spell

# Check that every script with a shebang is executable, and every helper isn't
check-script-permissions:
    #!/usr/bin/env bash
    # An entry-point script is runnable both ways: `dotnet run scripts/x.cs` and
    # `./scripts/x.cs` through its shebang. The recipes only use the first, so a lost
    # executable bit stays invisible until someone tries the second, which is how #33
    # shipped a broken one. A helper has no shebang, because it's #:include'd.
    set -euo pipefail
    status=0
    for file in scripts/*.cs; do
        if head -n 1 "$file" | grep -q '^#!'; then
            if [ ! -x "$file" ]; then
                echo "$file has a shebang but isn't executable: chmod +x it" >&2
                status=1
            fi
        elif [ -x "$file" ]; then
            echo "$file is executable but has no shebang: chmod -x it" >&2
            status=1
        fi
    done
    exit "$status"

# Validate every skill against the Agent Skills specification (needs skills-ref from .github/requirements.txt)
validate-skills:
    #!/usr/bin/env bash
    set -euo pipefail
    # An unmatched glob would otherwise loop once over the literal pattern, so the
    # empty-directory check below could never fire.
    shopt -s nullglob
    if ! command -v agentskills > /dev/null; then
        echo "agentskills isn't installed: run uv tool install skills-ref --with-requirements .github/requirements.txt" >&2
        exit 1
    fi
    status=0
    found=0
    for dir in skills/*/; do
        found=1
        if ! agentskills validate "$dir"; then
            status=1
        fi
    done
    if [ "$found" -eq 0 ]; then
        echo "No skill directories found under skills/" >&2
        status=1
    fi
    exit "$status"

# Check the metadata on every opinion, research topic, and template
validate-metadata:
    dotnet run scripts/validate-metadata.cs

# Check the roster, and every source that opinions and templates cite against it
validate-sources:
    dotnet run scripts/validate-sources.cs

# Check that README.md indexes every opinion and skill, in order
validate-readme-index:
    dotnet run scripts/validate-readme-index.cs

# Check that each template's package entries are sorted within their ItemGroup
validate-template-order:
    dotnet run scripts/validate-template-order.cs

# Draw the SVG diagrams in assets/diagrams/
export-diagrams:
    dotnet run scripts/export-diagrams.cs

# Fail if the committed diagrams differ from what export-diagrams draws
check-diagrams: export-diagrams
    #!/usr/bin/env bash
    # The SVGs are committed so the README renders them, which leaves them free to drift
    # from the script that draws them. This also fails on a diagram never committed.
    set -euo pipefail
    if [ -n "$(git status --porcelain -- assets/diagrams)" ]; then
        git status --short -- assets/diagrams >&2
        echo "assets/diagrams/ is out of date: run just export-diagrams and commit the result" >&2
        exit 1
    fi

# Stage the /dotnet/ pages of awesome-humans.net in <out>/dotnet/
build-site out="artifacts/site":
    dotnet run scripts/build-site.cs -- --out "{{ out }}"

# Run every offline check CI runs. It redraws the diagrams, and fails until you commit any SVG it changes
verify: check check-script-permissions validate-skills validate-metadata validate-sources validate-readme-index validate-template-order check-diagrams build-site
