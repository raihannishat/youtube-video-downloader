; YouTube Video Downloader Installer Script for Inno Setup
; This script creates a professional Windows installer

[Setup]
AppId={{8A5F8B2E-3C4D-4E5F-9A6B-7C8D9E0F1A2B}
AppName=YouTube Video Downloader
AppVersion=1.1.0
AppPublisher=Raihan Nishat
AppPublisherURL=https://github.com/raihannishat
AppSupportURL=https://github.com/raihannishat/youtube-video-downloader
AppUpdatesURL=https://github.com/raihannishat/youtube-video-downloader/releases
DefaultDirName={autopf}\YouTubeVideoDownloader
DefaultGroupName=YouTube Video Downloader
AllowNoIcons=yes
LicenseFile=
OutputDir=installer
OutputBaseFilename=YouTubeVideoDownloader-Setup-v1.1.0
SetupIconFile=icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
DisableProgramGroupPage=no
UninstallDisplayIcon={app}\YoutubeVideoDownloader.Console.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Application files (self-contained build)
Source: "YoutubeVideoDownloader\src\YoutubeVideoDownloader.Console\bin\Release\net10.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Include icon file
Source: "icon.ico"; DestDir: "{app}"; Flags: ignoreversion

; Optional: Bundle FFmpeg (uncomment if you want to include FFmpeg)
; Source: "ffmpeg\*"; DestDir: "{app}\ffmpeg"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\YouTube Video Downloader"; Filename: "{app}\YoutubeVideoDownloader.Console.exe"; IconFilename: "{app}\icon.ico"
Name: "{group}\{cm:UninstallProgram,YouTube Video Downloader}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\YouTube Video Downloader"; Filename: "{app}\YoutubeVideoDownloader.Console.exe"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\YouTube Video Downloader"; Filename: "{app}\YoutubeVideoDownloader.Console.exe"; IconFilename: "{app}\icon.ico"; Tasks: quicklaunchicon; OnlyBelowVersion: 6.1

[Run]
Filename: "{app}\YoutubeVideoDownloader.Console.exe"; Description: "{cm:LaunchProgram,YouTube Video Downloader}"; Flags: nowait postinstall skipifsilent

[Code]
// Check for .NET 10.0 Runtime (optional check for non-self-contained builds)
function InitializeSetup(): Boolean;
var
  DotNetVersion: String;
  ErrorCode: Integer;
begin
  Result := True;
  
  // Note: If using self-contained build, .NET runtime is included
  // This check is only needed for framework-dependent deployments
  
  // Uncomment below if NOT using self-contained build
  {
  if not RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost', 'Version', DotNetVersion) then
  begin
    if MsgBox('.NET 10.0 Runtime is not installed.' + #13#10 + #13#10 +
              'The application requires .NET 10.0 Runtime to run.' + #13#10 +
              'Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
    Result := False;
  end;
  }
end;

