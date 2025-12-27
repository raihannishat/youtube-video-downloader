# How to Create GitHub Release v1.2.1

This guide will help you create a GitHub release and upload the installer file.

## Prerequisites

- Installer file exists: `installer\YouTubeVideoDownloader-Setup-v1.2.1.exe`
- Git repository is set up
- You have push access to the repository

## Steps to Create Release

### Option 1: Using GitHub Web Interface (Recommended)

1. **Commit and Push Changes**
   ```bash
   git add README.md installer/
   git commit -m "Update README with new screenshots and direct download link"
   git push origin main
   ```

2. **Create Release on GitHub**
   - Go to: https://github.com/raihannishat/youtube-video-downloader
   - Click on "Releases" (right sidebar or top menu)
   - Click "Draft a new release" or "Create a new release"

3. **Fill Release Details**
   - **Tag version**: `v1.2.1`
   - **Release title**: `YouTube Video Downloader v1.2.1`
   - **Description**: 
     ```
     ## 🎉 YouTube Video Downloader v1.2.1

     ### ✨ New Features
     - 📜 Download History with pagination (25 entries per page)
     - ⚙️ Configuration file support
     - 📄 Batch download from file or direct URLs
     - 📁 Custom output directory selection
     - 📋 Playlist download support

     ### 🐛 Bug Fixes
     - Fixed desktop icon display issue
     - Improved error handling

     ### 📦 Installation
     Download the installer and run it. No additional dependencies required.
     ```

4. **Upload Installer**
   - Scroll down to "Attach binaries"
   - Click "Choose your files"
   - Select: `installer\YouTubeVideoDownloader-Setup-v1.2.1.exe`
   - Wait for upload to complete

5. **Publish Release**
   - Check "Set as the latest release" (if not already)
   - Click "Publish release"

### Option 2: Using Git Tags (Alternative)

1. **Create and Push Tag**
   ```bash
   git tag -a v1.2.1 -m "Release version 1.2.1"
   git push origin v1.2.1
   ```

2. **Then follow Option 1 steps 2-5** to create the release on GitHub web interface

## Verify Release

After creating the release, verify the direct download link works:
- https://github.com/raihannishat/youtube-video-downloader/releases/latest/download/YouTubeVideoDownloader-Setup-v1.2.1.exe

This link should automatically download the installer file.

## Troubleshooting

- **File too large**: GitHub has a 2GB limit for releases. The installer (~70-100 MB) should be fine.
- **Link not working**: Make sure:
  - The release is published (not draft)
  - The file name matches exactly: `YouTubeVideoDownloader-Setup-v1.2.1.exe`
  - The tag is `v1.2.1`

