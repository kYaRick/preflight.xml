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
# GitHub Pages subpath - used at publish time to rewrite <base href>
GH_PATH   := "preflight.xml"

# ─── default ─────────────────────────────────────────────────

# 📋 show all available recipes
default:
    @just --list --unsorted

# ─── onboarding ──────────────────────────────────────────────

# 🧩 one-time setup after clone (git hooks + package restore)
init:
    @echo "🔧 configuring git commit template…"
    git config --local commit.template .gitmessage
    @echo "📦 restoring .NET packages (skipped if no project yet)…"
    -dotnet restore
    @echo "✅ ready to contribute - run 'just' to see available commands"

# 🔄 restore NuGet packages
restore:
    dotnet restore

# ─── develop ─────────────────────────────────────────────────

# 🏗️ build all projects in Debug
build:
    dotnet build --configuration Debug

# 🚀 run the Blazor app locally (hot-reload)
run:
    dotnet watch --project {{PROJECT}}

# 🧪 run all tests
test:
    dotnet test --configuration Debug --verbosity normal

# 🎨 auto-format source
format:
    dotnet format

# ✅ verify formatting without modifying files (used by CI)
lint:
    dotnet format --verify-no-changes --verbosity diagnostic

# ─── release ─────────────────────────────────────────────────

# 📦 publish release build for GitHub Pages (rewrites <base href> to subpath)
publish:
    dotnet publish {{PROJECT}} --configuration {{CONFIG}} --output {{OUT_DIR}}
    @echo "🔧 rewriting <base href> for GitHub Pages subpath /{{GH_PATH}}/..."
    sed -i 's|<base href="/" />|<base href="/{{GH_PATH}}/" />|' {{OUT_DIR}}/wwwroot/index.html
    @echo "✅ published to {{OUT_DIR}}/wwwroot"

# ─── maintenance ─────────────────────────────────────────────

# 🧹 remove all build artifacts
clean:
    -rm -rf artifacts
    @echo "🧹 done"
