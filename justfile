# ─────────────────────────────────────────────────────────────
#  ✈️  preflight.xml - task runner
#
#  Requires `just`:
#    winget install Casey.Just             # Windows
#    brew install just                     # macOS
#    cargo install just                    # any platform (Rust)
#    scoop install just                    # Windows (Scoop)
#
#  Run `just` with no args to list every recipe.
# ─────────────────────────────────────────────────────────────

# force bash everywhere - requires git-bash on Windows (bundled with Git for Windows)
set shell := ["bash", "-ceuo", "pipefail"]

# ─── config ──────────────────────────────────────────────────
SOLUTION  := "Preflight.slnx"
PROJECT   := "srcs/Preflight.App/Preflight.App.csproj"
CONFIG    := "Release"
OUT_DIR   := "artifacts/app/Preflight.App/publish"
SITE_DIR  := OUT_DIR + "/wwwroot"
README    := "README.md"
BUILD_PROPS := "Directory.Build.props"

# ─── default ─────────────────────────────────────────────────

# 📋 show all available recipes
default:
    @just --list --unsorted

# ─── onboarding ──────────────────────────────────────────────

# 🧩 one-time setup after clone (git hooks + package restore)
init:
    @echo "🔧 configuring git commit template…"
    git config --local commit.template .gitmessage
    @echo "🪝 enabling repo git hooks…"
    git config --local core.hooksPath .githooks
    @echo "📦 restoring .NET packages (skipped if no project yet)…"
    -dotnet restore
    @echo "✅ ready to contribute - run 'just' to see available commands"

# 🔄 restore NuGet packages
restore:
    dotnet restore

# ─── develop ─────────────────────────────────────────────────

# 🏗️ build all projects in Debug
build:
    just sync-version-badges
    dotnet build --configuration Debug

# 🚀 run the Blazor app locally (hot-reload)
run:
    dotnet watch --project {{PROJECT}}

# 🧪 run all tests
test:
    just sync-version-badges
    dotnet test --configuration Debug --verbosity normal

# 🏷️ sync README version badge with Directory.Build.props version
sync-version-badges:
    @version_prefix="$(grep -oPm1 '(?<=<VersionPrefix>)[^<]+' {{BUILD_PROPS}} || true)"; \
    version_suffix="$(grep -oPm1 '(?<=<VersionSuffix>)[^<]+' {{BUILD_PROPS}} || true)"; \
    if [[ -z "$version_prefix" ]]; then \
      echo "❌ VersionPrefix not found in {{BUILD_PROPS}}"; \
      exit 1; \
    fi; \
    version="$version_prefix"; \
    if [[ -n "$version_suffix" ]]; then version="$version-$version_suffix"; fi; \
    perl -pi -e 's|(img\.shields\.io/static/v1\?label=version&message=)[^"&]+|$1v'"$version"'|g' {{README}}; \
    echo "🏷️ synced README version badge → v$version"

# 🔒 fail when README version badge is out of sync
check-version-badges:
    just sync-version-badges
    @git diff --exit-code -- {{README}} >/dev/null || (echo "❌ {{README}} has outdated version badge. Run: just sync-version-badges" && git --no-pager diff -- {{README}} && exit 1)

# 🎨 auto-format source
format:
    dotnet format

# ✅ verify formatting without modifying files (used by CI)
lint:
    dotnet format --verify-no-changes --verbosity diagnostic

# ─── release ─────────────────────────────────────────────────

# 📦 publish release build with base-path rewrite. Produces a fully
#    self-contained static site under {{SITE_DIR}} that can be served
#    from any static host (GitHub Pages, nginx, IIS, Caddy, S3, ...).
#
#    The `base` parameter MUST start and end with `/`:
#      just publish                    → /preflight.xml/  (GitHub Pages)
#      just publish /                  → /                (custom domain / localhost)
#      just publish /apps/preflight/   → /apps/preflight/ (reverse proxy)
#
#    The rewrite touches three things that are otherwise hardcoded to `/`:
#      1. <base href> in index.html  → Blazor's WASM loader resolves
#         _framework/* against this. Wrong value = 404 storm on boot.
#      2. `const base` in service-worker.js → offline cache keys are
#         built from this. Wrong value = cache miss on every request
#         and the PWA never goes offline.
#      3. 404.html is a copy of index.html → GitHub Pages serves it for
#         unknown paths, which turns into SPA-style deep-link support
#         (Blazor's Router picks up the URL once index.html boots).
publish base="/preflight.xml/":
    just sync-version-badges
    @echo "📦 publishing {{PROJECT}} ({{CONFIG}}) → {{OUT_DIR}}"
    dotnet publish {{PROJECT}} --configuration {{CONFIG}} --nologo --output {{OUT_DIR}}
    @echo "🔧 rewriting base path → {{base}}"
    # perl -i works identically on GNU, BSD (macOS) and git-bash.
    perl -pi -e 's|<base href="/" />|<base href="{{base}}" />|' {{SITE_DIR}}/index.html
    perl -pi -e 's|const base = "/";|const base = "{{base}}";|' {{SITE_DIR}}/service-worker.js
    cp {{SITE_DIR}}/index.html {{SITE_DIR}}/404.html
    # .nojekyll guards _framework/* against Jekyll stripping on the
    # legacy gh-pages branch publishing path. Harmless no-op for Actions.
    touch {{SITE_DIR}}/.nojekyll
    @echo "✅ static site ready → {{SITE_DIR}} (base {{base}})"

# 🌐 serve the published site locally (requires python3) - sanity check
#    before tagging a release or deploying to Pages.
serve port="8080":
    @echo "🌐 serving {{SITE_DIR}} at http://localhost:{{port}}/"
    python3 -m http.server {{port}} --directory {{SITE_DIR}}

# ─── maintenance ─────────────────────────────────────────────

# 🧹 remove all build artifacts
clean:
    -rm -rf artifacts
    @echo "🧹 done"
