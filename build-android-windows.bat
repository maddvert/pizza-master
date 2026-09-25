@echo off
setlocal
set "PROJECT=%~dp0"
set "UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe"
set "ANDROID_MODULE=C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Data\PlaybackEngines\AndroidPlayer"

if not exist "%UNITY_EDITOR%" (
  echo Unity 2022.3.62f1 is missing. Install it in Unity Hub first.
  pause
  exit /b 1
)
if not exist "%ANDROID_MODULE%" (
  echo Android Build Support is missing. Add it in Unity Hub along with SDK, NDK and OpenJDK.
  pause
  exit /b 1
)

echo Building Pizza Master test APK. This can take several minutes on first run.
"%UNITY_EDITOR%" -batchmode -nographics -quit -projectPath "%PROJECT%" -buildTarget Android -executeMethod BuildGame.Android -logFile "%PROJECT%build-android.log"
set "BUILD_RESULT=%ERRORLEVEL%"
if not "%BUILD_RESULT%"=="0" (
  echo Build failed. Open build-android.log and send its last error lines for a fix.
  pause
  exit /b %BUILD_RESULT%
)
if not exist "%PROJECT%build\PizzaMaster-test.apk" (
  echo Unity exited successfully but the APK was not found. Check build-android.log.
  pause
  exit /b 1
)
echo APK ready: "%PROJECT%build\PizzaMaster-test.apk"
pause
