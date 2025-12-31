# PowerShell script to build self-contained application and create installer
# Usage: .\build-installer.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "YouTube Video Downloader - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
cd YoutubeVideoDownloader
dotnet clean -c Release
if (Test-Path "src\YoutubeVideoDownloader.Console\bin\Release") {
    Remove-Item -Recurse -Force "src\YoutubeVideoDownloader.Console\bin\Release" -ErrorAction SilentlyContinue
}
Write-Host "✓ Cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Restore NuGet packages
Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Packages restored" -ForegroundColor Green
Write-Host ""

# Step 3: Build self-contained application
Write-Host "[3/4] Building self-contained application..." -ForegroundColor Yellow
Write-Host "  This may take a few minutes..." -ForegroundColor Gray

$publishPath = "src\YoutubeVideoDownloader.Console\bin\Release\net10.0\win-x64\publish"

dotnet publish src\YoutubeVideoDownloader.Console\YoutubeVideoDownloader.Console.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

if (Test-Path $publishPath) {
    $exeSize = (Get-Item "$publishPath\YoutubeVideoDownloader.Console.exe").Length / 1MB
    Write-Host "✓ Build successful (Size: $([math]::Round($exeSize, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "✗ Build output not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 4: Create installer with Inno Setup
Write-Host "[4/4] Creating installer..." -ForegroundColor Yellow
cd ..

$innoSetupPaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)

$innoSetupFound = $false
foreach ($path in $innoSetupPaths) {
    if (Test-Path $path) {
        $innoSetupExe = $path
        $innoSetupFound = $true
        break
    }
}

if ($innoSetupFound) {
    Write-Host "  Found Inno Setup at: $innoSetupExe" -ForegroundColor Gray
    
    # Create installer directory if it doesn't exist
    if (-not (Test-Path "installer")) {
        New-Item -ItemType Directory -Path "installer" | Out-Null
    }
    
    # Compile installer
    & $innoSetupExe "installer.iss"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Installer created successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Installer location: installer\YouTubeVideoDownloader-Setup-v1.2.3.exe" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Installer compilation failed" -ForegroundColor Red
        Write-Host "  Check installer.iss for errors" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ Inno Setup not found" -ForegroundColor Yellow
    Write-Host "  Installer script created but not compiled" -ForegroundColor Yellow
    Write-Host "  Please install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Alternative: Creating ZIP package instead..." -ForegroundColor Cyan
    
    # Create a ZIP package as alternative
    $zipPath = "installer\YouTubeVideoDownloader-v1.2.3.zip"
    $exePath = "YoutubeVideoDownloader\$publishPath\YoutubeVideoDownloader.Console.exe"
    
    if (Test-Path $exePath) {
        # Create installer directory if it doesn't exist
        if (-not (Test-Path "installer")) {
            New-Item -ItemType Directory -Path "installer" | Out-Null
        }
        
        # Create ZIP file
        $zipFullPath = Resolve-Path "installer" | Join-Path -ChildPath "YouTubeVideoDownloader-v1.2.3.zip"
        $exeFullPath = Resolve-Path $exePath
        
        Compress-Archive -Path $exeFullPath -DestinationPath $zipFullPath -Force
        
        if (Test-Path $zipFullPath) {
            $zipSize = (Get-Item $zipFullPath).Length / 1MB
            Write-Host "✓ ZIP package created successfully!" -ForegroundColor Green
            Write-Host "  Location: $zipFullPath" -ForegroundColor Cyan
            Write-Host "  Size: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  Users can extract and run: YoutubeVideoDownloader.Console.exe" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "  Standalone executable available at:" -ForegroundColor Cyan
    Write-Host "  $exePath" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

