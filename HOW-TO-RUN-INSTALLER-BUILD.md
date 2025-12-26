# How to Run build-installer.ps1

This guide explains how to run the `build-installer.ps1` script to create a Windows installer for YouTube Video Downloader.

## 📋 Prerequisites

Before running the script, ensure you have:

1. **.NET 10.0 SDK** installed
   - Verify: `dotnet --version` should show `10.0.x` or higher
   - Download: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)

2. **PowerShell 5.1 or later** (usually pre-installed on Windows)
   - Verify: `$PSVersionTable.PSVersion`

3. **Inno Setup** (optional, but recommended for creating installer)
   - Download: [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)
   - If not installed, the script will still build the executable but won't create the installer

## 🚀 Running the Script

### Method 1: PowerShell (Recommended)

1. **Open PowerShell**
   - Press `Win + X` and select "Windows PowerShell" or "Terminal"
   - Or search for "PowerShell" in Start Menu

2. **Navigate to the project root directory**
   ```powershell
   cd D:\GitHub\youtube-video-downloader
   ```

3. **Check execution policy** (if needed)
   ```powershell
   Get-ExecutionPolicy
   ```
   
   If it shows `Restricted`, run:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

4. **Run the script**
   ```powershell
   .\build-installer.ps1
   ```

### Method 2: PowerShell ISE

1. Open PowerShell ISE
2. File → Open → Select `build-installer.ps1`
3. Press `F5` or click "Run Script"

### Method 3: Command Prompt (with PowerShell)

```cmd
powershell -ExecutionPolicy Bypass -File build-installer.ps1
```

### Method 4: Right-click and Run

1. Navigate to the file in Windows Explorer
2. Right-click `build-installer.ps1`
3. Select "Run with PowerShell"

## ⚠️ Common Issues and Solutions

### Issue 1: "Execution Policy" Error

**Error Message:**
```
.\build-installer.ps1 : File cannot be loaded because running scripts is disabled on this system.
```

**Solution:**
```powershell
# Run PowerShell as Administrator, then:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Or run with bypass:
```powershell
powershell -ExecutionPolicy Bypass -File .\build-installer.ps1
```

### Issue 2: "dotnet command not found"

**Error Message:**
```
dotnet : The term 'dotnet' is not recognized
```

**Solution:**
1. Install .NET 10.0 SDK from [Microsoft website](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Restart PowerShell/terminal
3. Verify: `dotnet --version`

### Issue 3: "Inno Setup not found"

**Message:**
```
⚠ Inno Setup not found
```

**Solution:**
- This is not an error! The script will still build the executable
- To create installer, download and install Inno Setup from [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)
- After installing, run the script again

### Issue 4: Build Fails

**Possible Causes:**
- Missing NuGet packages
- Network issues
- .NET SDK not properly installed

**Solution:**
```powershell
# Clean and restore
cd YoutubeVideoDownloader
dotnet clean
dotnet restore
dotnet build

# Then run the installer script again
cd ..
.\build-installer.ps1
```

## 📊 What the Script Does

The script performs these steps automatically:

1. **Cleans previous builds** - Removes old build artifacts
2. **Restores NuGet packages** - Downloads all required packages
3. **Builds self-contained application** - Creates executable with .NET runtime included
4. **Creates installer** - Uses Inno Setup to create professional installer (if available)

## 📁 Output Locations

After running the script successfully:

- **Self-contained executable:**
  ```
  YoutubeVideoDownloader\src\YoutubeVideoDownloader.Console\bin\Release\net10.0\win-x64\publish\YoutubeVideoDownloader.Console.exe
  ```

- **Installer** (if Inno Setup is installed):
  ```
  installer\YouTubeVideoDownloader-Setup-v1.0.0.exe
  ```

## ✅ Expected Output

When the script runs successfully, you should see:

```
========================================
YouTube Video Downloader - Build Script
========================================

[1/4] Cleaning previous builds...
✓ Cleaned

[2/4] Restoring NuGet packages...
✓ Packages restored

[3/4] Building self-contained application...
  This may take a few minutes...
✓ Build successful (Size: XX.XX MB)

[4/4] Creating installer...
  Found Inno Setup at: C:\Program Files (x86)\Inno Setup 6\ISCC.exe
✓ Installer created successfully!

Installer location: installer\YouTubeVideoDownloader-Setup-v1.0.0.exe

========================================
Build Complete!
========================================
```

## 🎯 Quick Reference

```powershell
# Navigate to project root
cd D:\GitHub\youtube-video-downloader

# Run the script
.\build-installer.ps1

# If execution policy error:
powershell -ExecutionPolicy Bypass -File .\build-installer.ps1
```

## 📝 Notes

- The build process may take 5-10 minutes depending on your system
- The self-contained executable will be ~70-100 MB
- The installer will be ~80-120 MB
- All dependencies are automatically included in the build

## 🆘 Still Having Issues?

1. Check that you're in the correct directory (project root)
2. Verify .NET SDK is installed: `dotnet --version`
3. Check PowerShell version: `$PSVersionTable.PSVersion`
4. Review the error messages for specific issues
5. See [installer-guide.md](installer-guide.md) for more detailed information

