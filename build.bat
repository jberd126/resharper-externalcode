@set BASE_DIR=%~dp0
@set SOURCE_DIR=%BASE_DIR%src
@set INSTALL_DIR=%BASE_DIR%install
@set OUTPUT_DIR=%BASE_DIR%output

:: Build
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe %SOURCE_DIR%\EveningCreek.ReSharper.ExternalSources.sln -verbosity:m

:: Package
@if not exist %OUTPUT_DIR% @mkdir %OUTPUT_DIR%
%SOURCE_DIR%\.nuget\NuGet.exe pack %INSTALL_DIR%\resharper-externalcode.nuspec -BasePath %SOURCE_DIR%\EveningCreek.ReSharper.ExternalCode -NoPackageAnalysis -OutputDirectory %OUTPUT_DIR%
