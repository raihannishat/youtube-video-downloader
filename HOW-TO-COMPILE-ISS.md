# How to Compile .iss File (Create Installer)

An `.iss` file is an **Inno Setup script**, not an installer itself. You need to **compile** it using Inno Setup to create the actual installer.

## 📋 Prerequisites

1. **Inno Setup** must be installed
   - Download: [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)
   - Install with default settings

## 🚀 Method 1: Using Inno Setup Compiler (GUI)

### Step 1: Install Inno Setup

1. Download Inno Setup from [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)
2. Run the installer
3. Use default installation settings
4. Complete the installation

### Step 2: Open Inno Setup Compiler

1. Open **Inno Setup Compiler** from Start Menu
   - Search for "Inno Setup Compiler"
   - Or navigate to: `C:\Program Files (x86)\Inno Setup 6\Compil32.exe`

### Step 3: Open the .iss File

1. In Inno Setup Compiler, go to **File** → **Open**
2. Navigate to your project root directory
3. Select `installer.iss`
4. Click **Open**

### Step 4: Compile the Installer

1. Click **Build** → **Compile** (or press `F9`)
2. Wait for compilation to complete
3. Check the output window for any errors

### Step 5: Find the Installer

The compiled installer will be in:
```
installer\YouTubeVideoDownloader-Setup-v1.0.0.exe
```

## 🚀 Method 2: Using Command Line

### Step 1: Open PowerShell or Command Prompt

Navigate to the project root directory:
```powershell
cd D:\GitHub\youtube-video-downloader
```

### Step 2: Compile Using ISCC.exe

```powershell
# If Inno Setup is in default location
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

Or:
```powershell
# If Inno Setup is in Program Files (64-bit)
& "C:\Program Files\Inno Setup 6\ISCC.exe" installer.iss
```

### Step 3: Check Output

The installer will be created in the `installer\` folder.

## 🚀 Method 3: Using the Build Script (Easiest)

The `build-installer.ps1` script automatically compiles the .iss file if Inno Setup is installed:

```powershell
.\build-installer.ps1
```

This will:
1. Build the application
2. Automatically compile the .iss file
3. Create the installer

## ✅ Verification

After compilation, you should see:

```
Compiling [installer.iss]
Compiler version: Inno Setup 6.x.x
Compiling installer code...
Successfully compiled: installer\YouTubeVideoDownloader-Setup-v1.0.0.exe
```

## 🔍 Troubleshooting

### Problem: "ISCC.exe not found"

**Solution:**
- Verify Inno Setup is installed
- Check installation path: `C:\Program Files (x86)\Inno Setup 6\`
- Use full path in command: `& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss`

### Problem: "Source files not found"

**Error:**
```
Source: "YoutubeVideoDownloader\src\...\*" - File not found
```

**Solution:**
1. Make sure you've built the application first:
   ```powershell
   cd YoutubeVideoDownloader
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   cd ..
   ```
2. Then compile the .iss file

### Problem: Compilation Errors

**Common Issues:**
- Missing source files (build the app first)
- Incorrect paths in .iss file
- Missing dependencies

**Solution:**
- Check the error message in Inno Setup Compiler
- Verify all paths in `installer.iss` are correct
- Ensure the application is built before compiling

## 📝 Quick Reference

```powershell
# 1. Build the application first
cd YoutubeVideoDownloader
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
cd ..

# 2. Compile the installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss

# Or use the build script (does both)
.\build-installer.ps1
```

## 🎯 Summary

- **.iss file** = Script/Configuration file
- **Inno Setup** = Tool to compile the script
- **.exe installer** = Final output (what users install)

**Process:**
```
installer.iss → [Inno Setup Compiler] → YouTubeVideoDownloader-Setup-v1.0.0.exe
```

## 📚 Additional Resources

- Inno Setup Documentation: [https://jrsoftware.org/ishelp/](https://jrsoftware.org/ishelp/)
- Inno Setup Download: [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)

