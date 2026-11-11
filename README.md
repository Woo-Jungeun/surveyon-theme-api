# 🚀 설문온 NewProject - 표준 코어 API & Web UI 템플릿

본 프로젝트는 **설문온 시스템의 표준 아키텍처**를 기반으로 구축된 최신 백엔드(.NET 8 WebAPI) 및 프론트엔드(Vite + React + TypeScript + Tailwind CSS) 스타터 템플릿입니다.

신규 메뉴 또는 신규 모듈을 개발하는 담당자는 본 문서의 가이드를 숙지한 후 개발을 시작해 주시기 바랍니다.

---

## 📁 프로젝트 디렉터리 아키텍처

```
D:\python\Antigravity\(260804)설문온_NewProject\
├── README.md                          # 담당자용 프로젝트 전체 아키텍처 및 개발 가이드
├── CoreApi\                           # [.NET 8 WebAPI C# 백엔드 솔루션]
│   ├── Program.cs                     # Swagger Auth, CORS, EF Core, HttpClient("AuthCore") 통합
│   ├── appsettings.json               # 공통 설정 (PostgresHO, Auth 공통 규격)
│   ├── appsettings.Development.json   # 테스트/개발 서버 설정 (https://stest.hrc.kr/APIs/o/ 탑재)
│   ├── appsettings.Production.json    # 운영 서버 설정 (https://s.hrc.kr/APIs/o/ 탑재)
│   ├── Common\
│   │   ├── ApiResponse.cs             # HRC 표준 Return JSON 응답 래퍼 (code: 200, success: "777"/errorcode)
│   │   └── ErrorCodes.cs              # 공통 에러코드 정의
│   ├── Services\
│   │   ├── ErrorCodeService.cs        # PostgresHO pro_error_code DB errortext 자동 매핑 서비스
│   │   └── ProjectSyncService.cs      # APIs/o/project 비동기 동기화 릴레이 서비스
│   ├── Infrastructure\
│   │   ├── AuthCheckFilter.cs         # 동적 stest.hrc.kr / s.hrc.kr 토큰 검증 필터 (30s 메모리 캐시)
│   │   ├── AuthOption.cs              # 인증 설정 바인딩 모델
│   │   └── GlobalExceptionMiddleware.cs # 전역 예외 캐치 및 pro_error_code 에러 JSON 반환 미들웨어
│   ├── Data\
│   │   ├── AppDbContext.cs            # EF Core 메인 DbContext
│   │   ├── PostgresHoDbContext.cs      # PostgresHO (HOpen) 에러코드 전용 DbContext
│   │   └── Entities\                  # ProErrorCode.cs, BaseEntity.cs, User.cs, ProjectItem.cs
│   └── Controllers\
│       ├── AuthController.cs          # 로그인 컨트롤러
│       ├── ProjectController.cs       # 프로젝트 동기화 (list_start, list_progress) 프록시 컨트롤러
│       └── HealthController.cs        # 헬스체크 컨트롤러
└── WebUI\                             # [Vite + React + TS + Tailwind CSS 프론트엔드]
    ├── src\
    │   ├── api\client.ts              # X-Auth-Token 자동 주입 & success == "777" 검증 Axios 클라이언트
    │   ├── components\layout\         # 상단 헤더, 프로그래스바, 다크/라이트 테마 레이아웃
    │   ├── context\AuthContext.tsx    # 로그인 세션 Provider
    │   └── pages\
    │       ├── LoginPage.tsx          # syhong / 2402033 사전 입력 셋팅 로그인 페이지
    │       └── ProjectListPage.tsx    # 동기화 폴링 (list_start/list_progress) & 데이터 테이블 UI
    ├── package.json
    └── vite.config.ts
```

---

## 🔑 핵심 시스템 구현 규격

### 1. HTTP 200 기반 HRC 표준 Return JSON 규격 (`ApiResponse<T>`)
모든 백엔드 API는 **HTTP Status Code 200 OK**로 응답하며, 바디의 `success` 필드로 성공/실패 여부를 표시합니다.

#### 성공 응답 (`success: "777"`)
```json
{
  "code": 200,
  "success": "777",
  "message": "성공적으로 처리되었습니다.",
  "data": { ... },
  "timestamp": "2026-08-04T16:20:00Z"
}
```

#### 에러 응답 (`success = errorcode`, `PostgresHO` `pro_error_code` 메세지 자동 매핑)
에러 발생 시 `success: "401"`과 같이 에러코드가 반환되며, `message` 필드에는 `PostgresHO` DB (`HOpen`)의 `pro_error_code` 테이블에서 `error_code = "401"`에 해당하는 `errortext` 컬럼 메세지가 자동 매핑됩니다.
```json
{
  "code": 200,
  "success": "401",
  "message": "[pro_error_code/errortext] 비정상적 접속을 제한 합니다.",
  "data": null,
  "timestamp": "2026-08-04T16:20:00Z"
}
```

---

### 2. 환경별 중앙 토큰 인증 연동 (`AuthCheckFilter`)
- 헤더: `X-Auth-Token` 또는 `Authorization: Bearer <token>`
- **테스트/개발 환경 (`appsettings.Development.json`)**: `https://stest.hrc.kr/APIs/o/login/token/validate`
- **운영 환경 (`appsettings.Production.json`)**: `https://s.hrc.kr/APIs/o/login/token/validate`

---

### 3. 프로젝트 목록 비동기 동기화 폴링 규격 (`APIs/o/project`)
1. **동기화 시작 (`gb: "list_start"`)**:
   - Request: `{ "gb": "list_start", "user": "syhong" }`
   - Response: `{ "success": "777", "resultjson": { "jobId": "a1b2...", "status": "processing", "percent": 0 } }`
2. **주기적 폴링 (`gb: "list_progress"`)**:
   - Request: `{ "gb": "list_progress", "jobid": "a1b2...", "user": "syhong" }`
   - Response (진행중): `{ "resultjson": { "status": "processing", "percent": 45, "message": "[7/15] n2026_007 동기화 처리 중..." } }`
   - Response (완료): `{ "resultjson": { "status": "completed", "percent": 100, "resultjson": [ { ... } ] } }`
3. **UI 구성**: 프론트엔드에서 0~100% 프로그래스바 및 완료 시 데이터 테이블 자동 표출.

---

## 🛠️ 프로젝트 실행 방법

### 백엔드 (`CoreApi`) 실행
```bash
cd CoreApi
dotnet restore
# 테스트 환경 실행 (stest.hrc.kr 호출)
dotnet run --environment Development

# 운영 환경 실행 (s.hrc.kr 호출)
dotnet run --environment Production
```
- Swagger UI: `http://localhost:5000/swagger`

### 프론트엔드 (`WebUI`) 실행
```bash
cd WebUI
npm install
npm run dev
```
- 개발 서버: `http://localhost:5173`
- 기본 로그인 정보: 아이디 `syhong` / 비밀번호 `2402033`
