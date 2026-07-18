# Team Log — UI 폰트 설정 가이드 (UI-A.4)

> **작성일**: 2026-07-17
> **목적**: Party Selection UI (다크 판타지 고딕 톤)에 사용할 폰트 에셋 생성 절차 안내.
> 웹 목업의 `Cinzel`(고딕 타이틀) + `Cormorant Garamond`(이탤릭 인용구) + `Nanum Myeongjo`(한국어 본문) 조합을 유니티에서 재현.

---

## 1. 현재 폰트 인프라

| 폰트 | 위치 | 용도 |
|------|------|------|
| **NanumGothic.ttf** | `Assets/08.Resource/Fonts/NanumGothic.ttf` | 한국어 기본 (기존 프로젝트 전체) |
| **NanumGothic SDF.asset** | `Assets/08.Resource/Fonts/NanumGothic SDF.asset` | TMP 에셋 (한국어) |
| LiberationSans SDF | `Assets/TextMesh Pro/Resources/Fonts & Materials/` | TMP 기본 영문 |

---

## 2. 신규 필요 폰트 (UI-A.4)

### 2.1 Cinzel — 중세 로마 고딕 (타이틀/강조)
- **용도**: 캐릭터 이름, 섹션 라벨, EMBARK 버튼, 섹션 헤더
- **다운로드**: Google Fonts — https://fonts.google.com/specimen/Cinzel
  - **다운로드할 가중치**: Regular(400), Medium(500), Bold(700), Black(900)
  - "Download family" → ZIP 압축 해제 후 TTF 4개 확보

### 2.2 Cormorant Garamond — 이탤릭 세리프 (정체성 인용구)
- **용도**: 캐릭터 정체성 한 문장, 강점/약점 설명
- **다운로드**: Google Fonts — https://fonts.google.com/specimen/Cormorant+Garamond
  - **다운로드 가중치**: Regular(400) Italic, Medium(500) Italic
  - ※ Italic 필수 (정체성 인용구가 이탤릭)

### 2.3 (선택) Nanum Myeongjo — 명조체 (한국어 본문 강화)
- **용도**: 현재는 NanumGothic(고딕)이지만, 명조체가 고딕 판타지에 더 어울림
- **다운로드**: Google Fonts — https://fonts.google.com/specimen/Nanum+Myeongjo
  - **가중치**: Regular(400), Bold(700), ExtraBold(800)
  - 기존 NanumGothic을 대체하려면 전체 UI 검증 필요 → 우선 신규 UI에만 적용 권장

---

## 3. 폰트 파일 배치

다운로드한 TTF 파일들을 아래 경로에 복사:

```
Assets/08.Resource/Fonts/
├── NanumGothic.ttf              (기존)
├── NanumGothic SDF.asset        (기존)
├── Cinzel-Regular.ttf           (신규)
├── Cinzel-Medium.ttf            (신규)
├── Cinzel-Bold.ttf              (신규)
├── Cinzel-Black.ttf             (신규)
├── CormorantGaramond-Italic.ttf (신규)
├── CormorantGaramond-MediumItalic.ttf (신규)
└── NanumMyeongjo.ttf            (선택)
```

---

## 4. TMP Font Asset 생성 절차

각 TTF에 대해 반복:

### 4.1 Cinzel Black SDF 생성 (메인 타이틀용)

1. Unity 상단 메뉴: **Window > TextMeshPro > Font Asset Creator**
2. 설정:
   - **Source Font File**: `Cinzel-Black.ttf`
   - **Sampling Point Size**: Custom Size → **64** (타이틀은 큼직하게)
   - **Padding**: **5**
   - **Packing Method**: Optimum
   - **Atlas Population Mode**: Static
   - **Character Set**: Extended ASCII
   - **Atlas Width**: 2048
   - **Atlas Height**: 2048
   - **Render Mode**: Distance Field 32 (권장 — 큰 텍스트도 선명)
3. **Generate Font Atlas!** 클릭
4. 미리보기 확인 후 **Save** 클릭
5. 저장 경로: `Assets/08.Resource/Fonts/Cinzel-Black SDF.asset`

### 4.2 Cinzel Bold SDF 생성 (캐릭터 이름/섹션 라벨)

동일 절차, **Source Font File**만 `Cinzel-Bold.ttf`, Sampling Point Size **48**.
저장: `Assets/08.Resource/Fonts/Cinzel-Bold SDF.asset`

### 4.3 Cinzel Regular SDF 생성 (일반 라벨)

동일, `Cinzel-Regular.ttf`, Point Size **36**.
저장: `Assets/08.Resource/Fonts/Cinzel-Regular SDF.asset`

### 4.4 Cormorant Garamond Italic SDF 생성 (인용구)

동일, `CormorantGaramond-MediumItalic.ttf`, Point Size **42**.
저장: `Assets/08.Resource/Fonts/CormorantGaramond-Italic SDF.asset`

---

## 5. 한국어 Fallback 체인 설정 (핵심)

Cinzel은 영문만 지원 → 한국어 텍스트가 들어가면 박스(□)로 표시됨.
**Fallback Font Asset** 설정으로 자동으로 NanumGothic SDF로 폴백.

### 5.1 Cinzel-Bold SDF에 Fallback 추가

1. `Assets/08.Resource/Fonts/Cinzel-Bold SDF.asset` 선택
2. Inspector 하단의 **Fallback Font Assets** 섹션 찾기 (없으면 `+` 버튼으로 추가)
3. 리스트에 `NanumGothic SDF.asset` 드래그 앤 드롭
4. Apply

이렇게 하면 Cinzel Bold로 한국어를 쓰면 자동으로 NanumGothic으로 렌더링됨 (글꼴은 달라지지만 가독성 보장).

### 5.2 동일 Fallback을 다른 Cinzel SDF에도 적용

- `Cinzel-Black SDF` → NanumGothic SDF fallback
- `Cinzel-Regular SDF` → NanumGothic SDF fallback
- `CormorantGaramond-Italic SDF` → NanumGothic SDF fallback

### 5.3 (선택) 명조체 사용 시

`NanumMyeongjo.ttf`로 `NanumMyeongjo SDF.asset` 생성 후, 이것을 Fallback으로 사용.

---

## 6. 검증

### 테스트 씬에서 폰트 렌더링 확인

1. 임시 씬에 `TextMeshPro - Text` 생성
2. 각 폰트에 대해 테스트:
   - Cinzel Black: "TEAM LOG" (대문자 영문)
   - Cinzel Bold: "ASHE — THE PYROMANCER"
   - Cinzel Regular: "EMBER × 2 / TURN"
   - Cormorant Italic: "She is not one who uses fire, but one who is burning."
   - 한국어 폴백: "타오르는 자, 자신을 재로 삼아..."
3. 한글이 박스(□) 없이 정상 표시되는지 확인

### 자주 발생하는 문제

| 문제 | 원인 | 해결 |
|------|------|------|
| 한국어가 □로 표시 | Fallback 미설정 | 섹션 5.1 참조 |
| 텍스트가 흐릿함 | Point Size 너무 작음 | 48 이상 권장 |
| 아틀라스 오버플로우 | Character Set = All 로 설정 | Extended ASCII 또는 Custom |
| 굵기가 다름 | Source TTF 가중치 실수 | TTF 파일명 재확인 |

---

## 7. 사용 가이드라인 (UI-B 이후)

| 용도 | 폰트 | 크기 가이드 |
|------|------|------------|
| 캐릭터 이름 (ASHE) | Cinzel Bold | 19-22pt |
| 캐릭터 부제 (the Pyromancer) | Cormorant Italic | 11-13pt |
| 타이틀 (TEAM LOG) | Cinzel Black | 22-26pt |
| 섹션 라벨 (SKILLS / EQUIPPED TRAIT) | Cinzel Medium | 10-11pt, letter-spacing 0.3em |
| 스킬 이름 | Cinzel Medium | 11-12pt |
| 스킬/특성 설명 (한국어) | NanumGothic | 10-11pt |
| 정체성 인용구 | Cormorant Italic + NanumGothic | 13-14pt |
| 스탯 숫자 | Cinzel Bold | 18-22pt |
| 버튼 (EMBARK) | Cinzel Bold | 16pt, letter-spacing 0.25em |

---

## 8. 완료 체크리스트

- [ ] Cinzel TTF 4종 (Regular/Medium/Bold/Black) 다운로드
- [ ] Cormorant Garamond Italic TTF 다운로드
- [ ] (선택) NanumMyeongjo TTF 다운로드
- [ ] 모든 TTF `Assets/08.Resource/Fonts/`에 복사
- [ ] Cinzel Black SDF 생성
- [ ] Cinzel Bold SDF 생성
- [ ] Cinzel Regular SDF 생성
- [ ] Cormorant Garamond Italic SDF 생성
- [ ] 각 SDF에 NanumGothic SDF Fallback 연결
- [ ] 테스트 씬에서 한글/영문 렌더링 확인

---

## 9. 참고 자료

- Unity TMP 매뉴얼: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/
- Fallback 시스템 설명: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/Fallback.html
- Google Fonts Cinzel: https://fonts.google.com/specimen/Cinzel
- Google Fonts Cormorant Garamond: https://fonts.google.com/specimen/Cormorant+Garamond

---

> 이 가이드대로 진행 후, `UI-B`에서 본격적으로 컴포넌트 작성 시 이 폰트 에셋들을 참조합니다.
