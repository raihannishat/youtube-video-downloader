# How to Create GitHub Release v2.1.0

This guide will help you create a GitHub release and upload the installer file.

## Prerequisites

- .NET SDK installed
- Inno Setup installed (optional, for creating installer)
- Git repository is set up
- You have push access to the repository

## Steps to Create Release

### Step 1: Build the Installer

1. **Open PowerShell** in the root directory (`D:\GitHub\youtube-video-downloader`)

2. **Run the build script**:
   ```powershell
   .\build-installer.ps1
   ```

   This will:
   - Clean previous builds
   - Restore NuGet packages
   - Build the self-contained application
   - Create the installer (`installer\YouTubeVideoDownloader-Setup-v2.1.0.exe`)

3. **Verify the installer exists**:
   - Check: `installer\YouTubeVideoDownloader-Setup-v2.1.0.exe`

### Step 2: Commit and Push Changes

1. **Stage all changes**:
   ```bash
   git add .
   ```

2. **Commit changes**:
   ```bash
   git commit -m "Release v2.1.0 - Log files location fix and version update"
   ```

3. **Push to GitHub**:
   ```bash
   git push origin main
   ```

### Step 3: Create Release on GitHub

#### Option A: Using GitHub Web Interface (Recommended)

1. **Go to GitHub Repository**
   - Navigate to: https://github.com/raihannishat/youtube-video-downloader

2. **Create New Release**
   - Click on "Releases" (right sidebar or top menu)
   - Click "Draft a new release" or "Create a new release"

3. **Fill Release Details**
   - **Tag version**: `v2.1.0` (create new tag)
   - **Release title**: `YouTube Video Downloader v2.1.0`
   - **Description**: 
     ```markdown
     ## 🎉 YouTube Video Downloader v2.1.0

     ### ✨ New Features
     - 📋 Menu-driven interface with separate menus for each feature
     - 🎨 Improved UI with automatic screen clearing after menu operations
     - ⚙️ Enhanced default directory handling from configuration
     - 📜 Download History with pagination (25 entries per page)
     - ⚙️ Configuration file support
     - 📄 Batch download from file or direct URLs
     - 📁 Custom output directory selection
     - 📋 Playlist download support with history tracking

     ### 🐛 Bug Fixes
     - Fixed playlist downloads not being saved to history
     - Fixed playlist downloads not showing in statistics
     - Improved history tracking for batch playlist downloads
     - Fixed default download directory not respecting configuration

     ### 📦 Installation
     Download the installer and run it. No additional dependencies required.
     ```

4. **Upload Installer**
   - Scroll down to "Attach binaries by dropping them here or selecting them"
   - Click "Choose your files"
   - Select: `installer\YouTubeVideoDownloader-Setup-v2.1.0.exe`
   - Wait for upload to complete

5. **Publish Release**
   - Check "Set as the latest release" (if not already)
   - Click "Publish release"

#### Option B: Using Git Tags (Alternative)

1. **Create and Push Tag**:
   ```bash
   git tag -a v2.1.0 -m "Release version 2.1.0"
   git push origin v2.1.0
   ```

2. **Then follow Option A steps 2-5** to create the release on GitHub web interface

### Step 4: Verify Release

After creating the release, verify the direct download link works:
- https://github.com/raihannishat/youtube-video-downloader/releases/latest/download/YouTubeVideoDownloader-Setup-v2.1.0.exe

This link should automatically download the installer file.

## Troubleshooting

- **File too large**: GitHub has a 2GB limit for releases. The installer (~70-100 MB) should be fine.
- **Link not working**: Make sure:
  - The release is published (not draft)
  - The file name matches exactly: `YouTubeVideoDownloader-Setup-v2.1.0.exe`
  - The tag is `v2.1.0`
- **Build fails**: Make sure:
  - .NET SDK is installed
  - All dependencies are restored
  - Inno Setup is installed (for installer creation)

