# Installer Creation Guide for YouTube Video Downloader

This guide explains how to create a professional installer that automatically resolves all dependencies.

## 🎯 Installer Options

### Option 1: Self-Contained Executable (Simplest)
Creates a single executable with all dependencies included.

### Option 2: Inno Setup (Recommended)
Professional Windows installer with dependency checking.

### Option 3: WiX Toolset
Microsoft's official installer technology.

---

## 📦 Option 1: Self-Contained Executable

### Advantages:
- ✅ Single .exe file
- ✅ No separate installer needed
- ✅ All dependencies included
- ✅ Works on any Windows machine

### Create Self-Contained Build:

```bash
# Navigate to project directory
cd YoutubeVideoDownloader/src/YoutubeVideoDownloader.Console

# Publish as self-contained single file
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Output: bin/Release/net10.0/win-x64/publish/YoutubeVideoDownloader.Console.exe
```

### What's Included:
- ✅ .NET 10.0 Runtime (embedded)
- ✅ All NuGet packages
- ✅ Application code
- ⚠️ FFmpeg (downloads on first run if not found)

**Size**: ~70-100 MB (includes .NET runtime)

---

## 🏗️ Option 2: Inno Setup Installer (Recommended)

### Advantages:
- ✅ Professional installer UI
- ✅ Dependency checking (.NET runtime)
- ✅ Start Menu shortcuts
- ✅ Uninstaller
- ✅ Can bundle FFmpeg
- ✅ Customizable branding

### Step 1: Install Inno Setup

Download from: https://jrsoftware.org/isdl.php

### Step 2: Create Installer Script

The `installer.iss` file is already created in the root directory.

### Step 3: Build Installer

1. Open Inno Setup Compiler
2. Open `installer.iss`
3. Click "Build" → "Compile"
4. Installer will be in `installer/` folder

---

## 🔧 Option 3: WiX Toolset (Advanced)

### Advantages:
- ✅ Microsoft's official tool
- ✅ MSI installer format
- ✅ Enterprise-grade
- ✅ More complex but powerful

### Installation:

```bash
# Install WiX Toolset
winget install WiXToolset.WiXToolset
```

### Create WiX Project:

Create `installer.wxs`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="YouTube Video Downloader" Language="1033" Version="1.0.0" Manufacturer="Raihan Nishat" UpgradeCode="YOUR-GUID-HERE">
    <Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />
    
    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate />
    
    <Feature Id="ProductFeature" Title="YouTube Video Downloader" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
  </Product>

  <Fragment>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="YouTubeVideoDownloader" />
      </Directory>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ApplicationProgramsFolder" Name="YouTube Video Downloader" />
      </Directory>
    </Directory>
  </Fragment>

  <Fragment>
    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <Component Id="ApplicationFiles">
        <File Id="YoutubeVideoDownloaderConsole.exe" Source="YoutubeVideoDownloader\src\YoutubeVideoDownloader.Console\bin\Release\net10.0\win-x64\publish\YoutubeVideoDownloader.Console.exe" KeyPath="yes" />
      </Component>
    </ComponentGroup>
  </Fragment>
</Wix>
```

Build:
```bash
candle installer.wxs
light installer.wixobj
```

---

## 🚀 Recommended Approach: Hybrid Solution

### Best Practice:

1. **Create Self-Contained Build** (includes .NET runtime)
2. **Use Inno Setup** to package it
3. **Bundle FFmpeg** (optional, or let it download)

### Complete Build Script:

The `build-installer.ps1` script is already created in the root directory. Simply run:

```powershell
.\build-installer.ps1
```

---

## 📋 Dependency Auto-Resolution Strategy

### What Gets Auto-Resolved:

1. **.NET 10.0 Runtime**
   - ✅ Self-contained build: Included
   - ✅ Installer: Check and prompt if missing

2. **NuGet Packages** (YoutubeExplode, FFMpegCore, etc.)
   - ✅ Always included in build

3. **FFmpeg**
   - ✅ Auto-downloads on first run (already implemented)
   - ✅ Option: Bundle in installer (50-100 MB)

### FFmpeg Bundling Option:

To bundle FFmpeg in installer:

1. Download FFmpeg manually
2. Extract to `ffmpeg/` folder in root
3. Update installer script to include it:

```iss
[Files]
Source: "ffmpeg\*"; DestDir: "{app}\ffmpeg"; Flags: ignoreversion recursesubdirs
```

---

## ✅ Testing the Installer

### Checklist:

- [ ] Installer runs without errors
- [ ] Application launches after installation
- [ ] Shortcuts created correctly
- [ ] Uninstaller works
- [ ] Works on clean Windows machine (no .NET installed)
- [ ] FFmpeg auto-downloads on first run (if not bundled)

---

## 📦 Distribution

### Recommended Structure:

```
Release/
├── YouTubeVideoDownloader-Setup.exe (Installer)
├── README.md
├── LICENSE.txt
└── CHANGELOG.md
```

### File Sizes:
- Self-contained .exe: ~70-100 MB
- With installer: ~80-120 MB
- With bundled FFmpeg: ~150-200 MB

---

## 🎯 Quick Start (Recommended)

1. **Build self-contained:**
   ```bash
   cd YoutubeVideoDownloader
   dotnet publish src/YoutubeVideoDownloader.Console/YoutubeVideoDownloader.Console.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

2. **Create installer with Inno Setup** (use provided `installer.iss`)

3. **Test on clean machine**

4. **Distribute installer**

---

## 📝 Notes

- Self-contained builds are larger but more portable
- FFmpeg can be bundled or downloaded on first run
- .NET runtime check in installer ensures compatibility
- All NuGet dependencies are automatically included

