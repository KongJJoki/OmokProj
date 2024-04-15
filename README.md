# OmokProj
## API 서버

## Hive Server

### 계정 생성
- 클라이언트 → 서버 전송 데이터
  - account : 이메일
  - password : 패스워드
1. account가 이메일 형식인지 확인
2. account가 accountDB에 이미 등록되어 있는지 확인
3. DB에 account와 password 등록(계정 생성)
- 요청 예시
  ```csharp
        POST http://localhost:5021/accountcreate
        Content-Type: application.json
        
        {
              "account" : "kong@gmail.com",
              "password" : "1234"
        }
  ```
  - 응답 예시
    - 이메일 형식이 아닌 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - 이미 등록된 이메일인 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - 계정이 정상적으로 생성된 경우(ErrorCode = 0)
      ```csharp
        {
              "result" : 0
        }
      ```

<br>

### 계정 생성 Sequence Flow
***

```mermaid
sequenceDiagram
    actor A as client
    participant B as Hive_Server
    participant C as Account_DB

    %% 계정 생성
    note left of A: 계정 생성
    A->>B: [POST] /accountcreate
    B->>C: 이메일 중복 조회
    C--)B: 중복 여부 응답
    B--)A: [ErrorCode:?]이메일이 이미 등록된 경우(중복)
    B->>C: 이메일 / 패스워드 등록
    B--)A: [ErrorCode:0] 성공 응답
```
<br>

### 클라이언트 - Hive 서버 로그인
- 클라이언트 → 서버 전송 데이터
  - account : 이메일
  - password : 패스워드
1. accountDB에서 account에 해당하는 데이터 얻어오기
2. account가 존재하는지 확인
3. 전달받은 password와 얻어온 password가 일치하는지 확인
4. 인증토큰 생성

- 요청 예시
  ```csharp
        POST http://localhost:5021/hivelogin
        Content-Type: application.json
        
        {
              "account" : "kong@gmail.com",
              "password" : "1234"
        }
  ```
  - 응답 예시
    - 존재하지 않는 계정인 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - 패스워드가 일치하지 않는 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - 로그인에 성공한 경우(ErrorCode = 0)
      ```csharp
        {
              "result" : 0,
              "authToken" : ERQWEROJJP123
        }
      ```

<br>

### 인증토큰 검증
- 게임 서버 → Hive 서버 전송 데이터
  - account : 이메일
  - authToken : 인증토큰
1. account를 이용해서 인증토큰 발급하는 로직 수행
2. 전달받은 authToken과 새로 만든 인증토큰이 동일한지 확인
3. 게임 서버에 응답

- 요청 예시
  ```csharp
        POST http://localhost:5021/validauthtoken
        Content-Type: application.json
        
        {
              "account" : "kong@gmail.com",
              "authToken" : "EQNKFLWE123"
        }
  ```
  - 응답 예시
    - 인증토큰이 일치하지 않는 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - 인증토큰이 일치하는 경우(ErrorCode = 0)
      ```csharp
        {
              "result" : 0
        }
      ```

<br>

## Game Server

### 클라이언트 - 게임 서버 로그인
- 클라이언트 → 서버 전송 데이터
  - account : 이메일
  - authToken : Hive 서버에게서 받은 인증토큰
1. Hive 서버에 인증토큰 유효성 검증 요청
2. Hive 서버의 응답 결과에 따라 응답

- 요청 예시
  ```csharp
        POST http://localhost:5021/gamelogin
        Content-Type: application.json
        
        {
              "account" : "kong@gmail.com",
              "authToken" : "EIOQWENLFLQW123"
        }
  ```
  - 응답 예시
    - Hive 서버가 일치하지 않는다고 응답한 경우(ErrorCode = ?)
      ```csharp
        {
              "result" : ?
        }
      ```
    - Hive 서버가 일치한다고 응답한 경우(ErrorCode = 0)
      ```csharp
        {
              "result" : 0
        }
      ```

<br>

### 클라이언트 - 게임 서버 로그인 Sequence Flow
***

```mermaid
sequenceDiagram
    actor A as client
    participant B as Game_Server
    participant C as Hive_Server

    %% 클라이언트-게임 서버 로그인
    note left of A: 클라이언트-게임 서버 로그인
    A->>B: [POST] /gamelogin
    B->>C: 인증토큰 유효성 검증 요청
    C--)B: 검증 결과 응답
    B--)A: [ErrorCode:?] 인증토큰이 유효하지 않은 경우
    B--)A: [ErrorCode:0] 인증토큰이 유효한 경우
```
<br>

