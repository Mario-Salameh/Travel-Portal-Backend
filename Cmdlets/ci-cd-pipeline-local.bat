@echo off
echo ========================================
echo Starting build and deployment process...
echo ========================================

set SERVER_USER=mainuser
set SERVER_HOST=10.10.110.3
set SERVER_PASSWORD=mainpoamert5

echo.
echo [1/4] Building .NET Core project...
if exist C:\ReleaseSV rmdir /s /q C:\ReleaseSV
dotnet publish "%~dp0..\TrustSystem.Backend.csproj" -c Release -o C:/ReleaseSV --runtime linux-x64 --self-contained true

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build completed successfully!

echo.
echo [2/4] Copying files to server...
echo Deleting old configurations
plink -batch -P 2255 -pw %SERVER_PASSWORD% %SERVER_USER%@%SERVER_HOST% "rm -rf /var/www/service-nexus/*"

echo Copying new flies
pscp -batch -P 2255 -r -pw %SERVER_PASSWORD% C:/ReleaseSV/* %SERVER_USER%@%SERVER_HOST%:/var/www/service-nexus/
plink -batch -P 2255 -pw %SERVER_PASSWORD% %SERVER_USER%@%SERVER_HOST% "echo %SERVER_PASSWORD% | sudo -S chmod +x /var/www/service-nexus/TrustSystem.Backend"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: File copy failed!
    pause
    exit /b 1
)
echo Files copied successfully!

echo.
echo [3/4] Restarting service on server...

plink -batch -P 2255 -pw %SERVER_PASSWORD% %SERVER_USER%@%SERVER_HOST% "echo %SERVER_PASSWORD% | sudo -S systemctl restart service-nexus"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Service restart failed!
    pause
    exit /b 1
)
echo Service restarted successfully!

echo.
echo [4/4] Checking service status...
plink -batch -P 2255 -pw %SERVER_PASSWORD% %SERVER_USER%@%SERVER_HOST% "echo %SERVER_PASSWORD% | sudo -S systemctl status service-nexus"

echo.
echo ========================================
echo Deployment completed successfully!
echo ========================================