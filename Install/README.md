# MyukView 설치 가이드

## 파일 연결 설치

Windows에서 이미지 파일을 MyukView로 열 수 있도록 설정하는 방법입니다.

### 방법 1: PowerShell 스크립트 사용 (권장)

1. **관리자 권한**으로 PowerShell 실행
2. 이 폴더로 이동
3. 다음 명령 실행:
   ```powershell
   .\InstallFileAssociation.ps1
   ```

이 스크립트는:
- MyukView.exe 경로를 자동으로 찾습니다
- 레지스트리를 자동으로 등록합니다
- 지원하는 모든 이미지 형식을 연결합니다

### 방법 2: 레지스트리 파일 수동 편집

1. `RegisterFileAssociation.reg` 파일을 텍스트 에디터로 열기
2. 모든 `%INSTALL_PATH%`를 실제 MyukView.exe 경로로 변경
   - 예: `C:\\Program Files\\MyukView\\MyukView.exe`
   - 주의: 경로에서 `\`를 `\\`로 이중 백슬래시 사용
3. 파일 저장 후 더블클릭하여 레지스트리 등록

### 방법 3: Windows 설정에서 직접 연결

1. 이미지 파일을 **마우스 오른쪽 버튼**으로 클릭
2. **연결 프로그램** > **다른 앱 선택**
3. **이 PC에서 다른 앱 찾기**
4. MyukView.exe 파일 선택
5. **항상 이 앱을 사용하여 파일 열기** 체크

## 지원 파일 형식

설치 후 다음 파일 형식을 MyukView로 열 수 있습니다:

- **JPG/JPEG** - 일반 이미지 및 Motion Photo (갤럭시)
- **PNG** - 투명도 지원
- **BMP** - Windows 비트맵
- **GIF** - 애니메이션 지원
- **WEBP** - Google 이미지 형식
- **AVIF** - 차세대 이미지 형식
- **TIFF/TIF** - 고품질 이미지
- **HEIC** - 아이폰 Live Photos

## Motion Photo & Live Photo 지원

### Motion Photo (갤럭시)

갤럭시 스마트폰으로 촬영한 Motion Photo는 JPG 파일 내부에 짧은 동영상이 포함되어 있습니다.

**사용 방법:**
1. Motion Photo를 MyukView로 열기
2. 이미지 하단에 "Motion Photo 감지됨" 표시 확인
3. "동영상 추출" 버튼 클릭
4. 추출된 MP4 파일이 원본과 같은 폴더에 저장됨

### Live Photo (아이폰)

아이폰의 Live Photo는 HEIC 파일 형식으로 저장되며, 내부에 이미지와 동영상이 함께 포함되어 있습니다.

**지원 예정:**
- HEIC 파일 로드
- Live Photo 동영상 추출
- 재생 기능

## 제거 방법

파일 연결을 제거하려면:

1. Windows 설정 > 앱 > 기본 앱
2. 파일 형식별 기본 앱에서 각 이미지 형식 찾기
3. MyukView를 다른 앱으로 변경

또는 레지스트리 에디터에서 직접 삭제:
- `HKEY_CLASSES_ROOT\MyukView.Image`
- `HKEY_CLASSES_ROOT\Applications\MyukView.exe`

## 문제 해결

### "MyukView.exe를 찾을 수 없습니다" 오류

- 프로젝트를 빌드했는지 확인
- Debug 또는 Release 빌드 경로 확인:
  - `MyukView\bin\Debug\net6.0-windows\MyukView.exe`
  - `MyukView\bin\Release\net6.0-windows\MyukView.exe`

### 이미지 파일을 열 수 없음

- MyukView.exe가 이동되지 않았는지 확인
- 레지스트리에 등록된 경로가 정확한지 확인
- 설치 스크립트를 다시 실행

### Motion Photo가 감지되지 않음

- 파일이 실제 Motion Photo인지 확인 (갤럭시에서 촬영)
- 파일 크기가 일반 JPG보다 큰지 확인 (동영상 데이터 포함)
- 파일이 수정되거나 재압축되지 않았는지 확인

## 추가 정보

MyukView는 다음 기능을 제공합니다:

- **이미지 뷰어**: 확대/축소, 회전, 팬
- **포맷 변환**: AVIF/WEBP → PNG/JPG
- **이미지 편집**: 크기 조정, 자르기, 필터, 색상 조정
- **동영상 처리**: 오디오 추출, 텍스트 변환 (Whisper)
- **슬라이드쇼**: 전체화면 자동 재생
- **Motion Photo**: 갤럭시 움직이는 사진 지원

자세한 기능은 메인 README.md 파일을 참조하세요.
