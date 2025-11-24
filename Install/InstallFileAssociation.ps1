# MyukView 파일 연결 자동 설치 스크립트
# 관리자 권한으로 실행 필요

# 관리자 권한 확인
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "이 스크립트는 관리자 권한이 필요합니다." -ForegroundColor Red
    Write-Host "PowerShell을 관리자 권한으로 실행한 후 다시 시도하세요." -ForegroundColor Yellow
    pause
    exit
}

# MyukView.exe 경로 찾기
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$exePath = Join-Path $projectRoot "MyukView\bin\Release\net6.0-windows\MyukView.exe"

# Debug 빌드도 확인
if (-not (Test-Path $exePath)) {
    $exePath = Join-Path $projectRoot "MyukView\bin\Debug\net6.0-windows\MyukView.exe"
}

if (-not (Test-Path $exePath)) {
    Write-Host "MyukView.exe를 찾을 수 없습니다." -ForegroundColor Red
    Write-Host "프로젝트를 먼저 빌드하세요." -ForegroundColor Yellow
    $exePath = Read-Host "또는 MyukView.exe의 전체 경로를 입력하세요"

    if (-not (Test-Path $exePath)) {
        Write-Host "유효하지 않은 경로입니다." -ForegroundColor Red
        pause
        exit
    }
}

Write-Host "MyukView.exe 경로: $exePath" -ForegroundColor Green

# 레지스트리 등록
$regContent = @"
Windows Registry Editor Version 5.00

; MyukView 이미지 뷰어 파일 연결 등록

; 프로그램 등록
[HKEY_CLASSES_ROOT\Applications\MyukView.exe]
@="MyukView Image Viewer"

[HKEY_CLASSES_ROOT\Applications\MyukView.exe\shell\open\command]
@="`"$($exePath.Replace('\','\\'))`" `"%1`""

; 이미지 파일 타입 정의
[HKEY_CLASSES_ROOT\MyukView.Image]
@="MyukView Image File"

[HKEY_CLASSES_ROOT\MyukView.Image\DefaultIcon]
@="`"$($exePath.Replace('\','\\'))`",0"

[HKEY_CLASSES_ROOT\MyukView.Image\shell\open\command]
@="`"$($exePath.Replace('\','\\'))`" `"%1`""

; 지원 파일 형식
[HKEY_CLASSES_ROOT\.jpg\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.jpeg\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.png\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.bmp\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.gif\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.webp\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.avif\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.tiff\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.tif\OpenWithProgids]
"MyukView.Image"=""

[HKEY_CLASSES_ROOT\.heic\OpenWithProgids]
"MyukView.Image"=""
"@

# 임시 레지스트리 파일 생성
$tempRegFile = Join-Path $env:TEMP "MyukView_Install.reg"
$regContent | Out-File -FilePath $tempRegFile -Encoding Unicode

# 레지스트리 적용
Write-Host "레지스트리를 등록하는 중..." -ForegroundColor Cyan
Start-Process -FilePath "regedit.exe" -ArgumentList "/s `"$tempRegFile`"" -Wait -NoNewWindow

# 임시 파일 삭제
Remove-Item $tempRegFile -Force

Write-Host ""
Write-Host "파일 연결이 완료되었습니다!" -ForegroundColor Green
Write-Host ""
Write-Host "이제 이미지 파일을 마우스 오른쪽 버튼으로 클릭하고" -ForegroundColor Yellow
Write-Host "'연결 프로그램' > 'MyukView Image Viewer'를 선택하면 됩니다." -ForegroundColor Yellow
Write-Host ""
Write-Host "지원 파일 형식: JPG, PNG, BMP, GIF, WEBP, AVIF, TIFF, HEIC" -ForegroundColor Cyan
Write-Host ""
pause
