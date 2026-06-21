# Duran, the Warrior — "불멸의 성벽"

> **캐릭터 상세 설계 문서**. 전체 개편 개요는 [CharacterConceptReview.md](../CharacterConceptReview.md) 참조.
> 기존 8캐릭터 중 하나 (Warrior 슬롯 계승, 컨셉 전면 재설계)

---

## 1. 정체성 (한 문장)

Duran은 **충격을 흡수하여 역석으로 전환하는 불멸의 성벽**이다. 쉴드가 벽이라면, Duran은 벽 너머에서 적을 짓이기는 투석기다. 그는 받은 고통을 되갚으며, 부서지는 대신 더 단단해진다.

---

## 2. 배경 스토리

Duran은 용병단에서 가장 먼저 전선에 서는 돌격대장이었다. 그가 세운 방패 뒤에서 동료들은 살아남았고, 그 방패에 부딪힌 적은 그 반동으로 무너졌다. 어느 전투에서 동료 전원이 쓰러졌을 때, 홀로 남은 그는 동료들의 시신 위에 방패를 세우고 이틀 밤을 버텼다. 그는 죽지 않았다 — 부서지지 않았기 때문이 아니라, **받은 충격을 모두 되돌릴 수 있었기 때문이다.**

**이름 어원**: Duran = 라틴어 *durandus* ("견고한"). 어원 자체가 "안 부서지는" 의미.

**감정 키워드**: 인내, 책임, 속죄, 고독한 견딤. "받은 고통을 네게 돌려주겠다" — 응징의 서사이자 묵묵한 수호의 미학.

---

## 3. 왜 Team Log에서만 가능한 컨셉인가

| Team Log 특수성 | Duran 결합 방식 |
|---------------|----------------|
| **적 intent 공개** | "다음 턴 보스가 Healer를 공격" → Shield Wall을 Healer에게 → 원격 Vengeance 축적. ItB식 정보 기반 전략 |
| **부활 시스템** (CC-0) | Last Bastion 궁극기 = 광역 도발로 사망 위험. 부활(HP 50% + MaxHP -10%)로 "희생해도 다시 일어나는" 서사 정당화 |
| **AP 공유** | Shield Wall(AP 1) + Provoking Shield(AP 1)로 저비용 충전. 남은 AP로 다른 아군 딜 |
| **드로우 운** | Revenge Strike는 강화 조건 없이 항상 (10 + Vengeance) 발동. 드로우 운 의존 최소 |

---

## 4. 역할군 명시

- **주 역할군**: 메인 탱커 (Main Tank) — 쉴드/도발로 적 공격을 받아내며 파티 보호
- **부 역할군**: 버스트 딜러 (Burst Striker) — Vengeance 축적 후 단일 결정타

Ashe(순간 폭딜, 자기 파괴)와 Duran(지속 탱킹 + 역석 버스트)은 **정반대 타이밍의 진가**. Ashe는 자원을 써서 폭발, Duran은 맞아서 자원을 얻어 폭발.

---

## 5. 핵심 메카닉: Vengeance (복수 게이지)

### 축적 조건 — 2가지 경로

```
경로 A: Duran 본인이 받는 데미지 (쉴드 흡수 + HP 직접) 1:1 축적
경로 B: Duran이 부여한 쉴드가 다른 아군에서 흡수한 데미지 1:1 축적 (원격 흡수)
```

→ Shield Wall로 아군에게 쉴드를 줘도, 그 아군이 맞을 때 Duran의 Vengeance가 쌓임. **능동적 자원 축적**.

### 핵심 수치

```
Vengeance 최대치: 20 (캡)
자연 감소: 없음 (Duran이 안 맞으면 자연히 쌓이지 않음 — 이게 자연 제한)
소비: Revenge Strike(전량 소모) / Last Bastion(15 소모)
```

### 핵심 긴장감

> **"이번 턴 쉴드를 누구에게 씌울까 + 언제 Vengeance를 쓸까"**

Vengeance는 **Duran(또는 Duran이 부여한 쉴드)이 적에게 맞아야만 쌓임**. 안 맞으면 안 쌓이므로 자연 제한이 이미 존재. 절반 소실 같은 인위적 감소는 불필요 — 대신 "Vengeance가 쌓였을 때 얼른 소비할까, 더 모을까" 타이밍 딜레마가 핵심 재미.

---

## 6. 스킬 4종 — 2:2 구조 (충전 2 / 소비 2)

> 4.5 원칙 준수. 충전 스킬 2개는 모두 Vengeance 비례 강화 (임계값 차별화). 소비 스킬 2개는 강화 조건 없음(Revenge Strike) / 사용 제약(Last Bastion).

### 충전 스킬 2종 (Vengeance 비례 강화)

| 스킬 | AP | 기본 효과 (항상 발동) | 강화 조건 | 충족 시 쉴드 공식 |
|-----|-----|---------------------|---------|----------------|
| **Shield Wall** (방패벽) | 1 | 아군 1명(자신 포함 선택) 쉴드 부여 | Vengeance 10+ | 쉴드 = **10 + Vengeance/2** |
| **Provoking Shield** (도발 방패) | 1 | 단일 적 ForcedTarget(Duran) 1턴 + 자신 쉴드 부여 | Vengeance 5+ | 쉴드 = **6 + Vengeance/2** |

### 소비 스킬 2종

| 스킬 | AP | 기본 효과 (항상 발동) | 조건 |
|-----|-----|---------------------|------|
| **Revenge Strike** (복수의 일격) | 2 | 단일 **(10 + Vengeance)** 데미지, Vengeance 전량 소모 | (조건 없음) |
| **Last Bastion** (최후의 보루) | 3 | 모든 적 도발 1턴 + **본인 HP 회복 25** + **본인 쉴드 25** | ⚠️ 사용 제약: Vengeance 15+ 필수 → Vengeance 15 소모 |

---

## 7. 충전 스킬 상세

### Shield Wall — 아군 선택형 능동 쉴드

**용도**: 적 intent를 보고 가장 위험한 아군에게 쉴드 배치. 그 아군이 맞으면 **Duran의 Vengeance가 원격 축적**.

**Vengeance 비례 강화 곡선**:

| 현재 Vengeance | Shield Wall 쉴드 |
|--------------|----------------|
| 0~9 | 10 (기본) |
| 10 | 15 |
| 15 | 17 |
| 20 (캡) | **20** |

**전략**:
- Vengeance 0~9: 기본 10. 가성비 쉴드.
- Vengeance 10+: 강화 발동. 적 intent 보고 위험한 아군에게 강력한 쉴드.
- 자신에게도 사용 가능 (Duran이 직접 맞아서 Vengeance 추가 축적).

### Provoking Shield — 가성비 도발+자신 쉴드

**용도**: 단일 적을 Duran에게 유인. 도발로 맞으면서 Vengeance 축적 가속. AP 1이라 자주 사용 가능.

**Vengeance 비례 강화 곡선**:

| 현재 Vengeance | Provoking Shield 쉴드 | 도발 지속 |
|--------------|---------------------|---------|
| 0~4 | 6 (기본) | 1턴 |
| 5 | 8 | 1턴 |
| 10 | 11 | 1턴 |
| 20 (캡) | **16** | 1턴 |

**전략**:
- AP 1로 매 턴 사용 가능한 가성비 도발기.
- 다수전에서는 1마리만 묶이지만, 위협적인 적 1마리(예: 보스)를 Duran에게 고정.
- Vengeance 5+ 강화는 빠른 발동 (Shield Wall보다 낮은 임계).

---

## 8. 소비 스킬 상세

### Revenge Strike — Vengeance 전환 단일 딜

**공식**: `(10 + Vengeance)` 데미지, Vengeance 전량 소모

**데미지 곡선**:

| 현재 Vengeance | 데미지 | 비고 |
|--------------|-------|------|
| 0 | 10 | 평타 (Vengeance 없을 때) |
| 5 | 15 | 약한 버스트 |
| 10 | 20 | 평균 버스트 |
| 15 | 25 | 강력 |
| 20 (캡) | **30** | 최대 버스트 |

**설계 의도**: 강화 조건/보너스 없이 **Vengeance 양 자체가 데미지**. 단순하고 직관적. 드로우 운 의존도 하락 (어떤 상황에서도 사용 가능).

### Last Bastion — 궁극기 (본인 생존 + 광역 도발)

**사용 제약** (규칙 5B): Vengeance 15+ 필수 → Vengeance 15 소모

**효과**:
- 모든 적 도발 1턴 (광역 어그로)
- 본인 HP 회복 25
- 본인 쉴드 25

**사이클**:
```
Last Bastion 사용 (Vengeance 15 소모)
  → HP 25 회복 + 쉴드 25 + 모든 적 도발
적 턴: 모든 적(3마리)이 Duran 공격
  → 쉴드 25 흡수 + HP 손실 → Vengeance 대량 축적 (캡 20 도달 가능)
다음 턴: Revenge Strike (30 데미지)
```

**왜 본인 HP 회복+쉴드인가**:
- 광역 도발 = 다음 턴 Duran 집중 공격당함
- 회복+쉴드 없으면 3마리에게 맞고 사망 → 부활 페널티
- 버티면 다음 턴 Vengeance 폭발 → Revenge Strike 강화 타이밍
- **"맞아서 쓰러지지 않는 불멸의 성벽"** 서사 완성

**왜 파티 쉴드가 아닌가**:
- 광역 도발로 모든 적이 Duran만 공격 → 다른 아군은 안 맞음
- 파티 쉴드는 낭비. 본인 생존이 사이클 완성의 열쇠.

---

## 9. 조건 다양성 검증 (규칙 2)

| 스킬 | 조건 | 유형 |
|-----|------|------|
| Shield Wall | Vengeance 10+ | 자원 임계 (중간) |
| Provoking Shield | Vengeance 5+ | 자원 임계 (낮음) |
| Revenge Strike | (조건 없음) | 무조건 |
| Last Bastion | Vengeance 15+ (사용제약) | 자원 임계 (높음) |

**3개가 Vengeance 자원 기반이지만 임계값이 다름 (5/10/15) + 1개는 무조건**. 임계값 차별화로 4.5 원칙 2 준수.

---

## 10. 핵심 플레이 루프

### 시나리오 A: 적 intent 기반 능동 플레이 (원격 흡수)
```
적 intent: "보스가 Healer를 30 데미지로 공격"
턴 1: Shield Wall을 Healer에게 → Healer 쉴드 10
적 턴: 보스가 Healer 공격 → 쉴드 10 흡수 → Duran Vengeance +10
턴 2: Shield Wall 강화 발동 (Vengeance 10+) → 쉴드 15를 다른 위험 아군에게
적 턴: 또 다른 아군이 맞음 → Vengeance 추가 축적
턴 3: Revenge Strike (Vengeance 20) → 30 데미지
```

### 시나리오 B: 자기 희생 루프 (직접 흡수)
```
턴 1: Provoking Shield (Vengeance 5+) → 보스 도발 + Duran 쉴드 8
적 턴: 보스가 Duran 공격 → 쉴드 8 흡수 + HP 손실 → Vengeance +15
턴 2: Revenge Strike (Vengeance 15) → 25 데미지. Vengeance 0.
```

### 시나리오 C: 궁극기 콤보
```
턴 1-2: Shield Wall + Provoking Shield로 Vengeance 15+ 축적
턴 3: Last Bastion (Vengeance 15 소모) → HP 25 회복 + 쉴드 25 + 광역 도발
적 턴: 모든 적(3마리)이 Duran 공격 → 쉴드 25 흡수 + HP 손실 → Vengeance +20 (캡)
턴 4: Revenge Strike (Vengeance 20) → 30 데미지. 사이클 반복
```

---

## 11. 파티 시너지 매트릭스

| 조합 | 시너지 | 핵심 |
|------|-------|------|
| **Duran + Healer** | ★★★★★ | Healer 힐로 Duran HP 회복 → Duran이 계속 맞으면서 Vengeance 축적. Healer를 Shield Wall로 보호하면 Healer가 안전하게 힐. 상호 보호 루프 |
| **Duran + Ashe** | ★★★★ | Ashe(자동 폭발) + Duran(능동 버스트). 구조가 달라 간섭 없이 딜 병합. 둘 다 단일 딜 특화 |
| **Duran + Cryomancer** | ★★★★ | Blizzard AtkDown → Duran HP 손실 감소 → 더 오래 Vengeance 사이클 유지. Frost Armor로 Duran 쉴드 강화 |
| **Duran + Alchemist** | ★★★★ | Alchemist 힐 포션 → Healer와 유사한 시너지. Poison 도트로 적 약화 |
| **Duran + Rogue** | ★★★ | Duran이 도발로 적 고정 → Rogue 안전하게 콤보 적재. 간접 시너지 |

---

## 12. 어센션 시나리오

### 어센션 5 (적 HP +5%, 적 ATK +5%)
- Duran의 쉴드가 약간 더 빨리 깨지지만, Vengeance 축적 속도도 증가 (더 많이 맞으므로)
- Shield Wall 강화 발동 창 유지. 정상 작동

### 어센션 10 (적 ATK +10%, 파티 MaxHP -5%, 힐 -20%)
- 적 ATK 증가 → Vengeance 축적 가속 (자연 보정)
- 힐 감소 → Duran HP 회복이 느려짐 → Last Bastion(본인 HP 회복) 가치 상승
- Provoking Shield(AP 1)가 더 자주 쓰임 — 비싼 스킬 못 쓰는 상황 대응

### 어센션 15 (적 ATK +10%, 보스 HP +20%, 파티 MaxHP -10%)
- F4 보스 HP 384. Revenge Strike 30 데미지 = 보스 HP 7.8%
- Duran의 역할이 "버스트 딜러"에서 "퓨어 탱커"로 자연 전환
- Last Bastion(광역 도발 + 본인 힐)이 보스전 핵심 — 다음 턴 Vengeance 폭발로 파티 딜 지원
- 부활 시스템과 결합: 사망 위험 감수하며 궁극기 사용 → "불멸의 성벽" 서사 정점

---

## 13. UI/연출

### Vengeance 게이지
- **위치**: Duran 캐릭터 패널(PlayerSidebarPanel) HP 바 아래, 쉴드 바 위
- **시각**: 진한 보라색 바 (Ashe Ember=주황과 대비)
- **임계 표시**: 5 (Provoking 강화), 10 (Shield Wall 강화), 15 (Last Bastion 사용 가능)에 눈금
- **연출**: Vengeance 15+ 도달 시 게이지 지속적 펄스 + Duran 캐릭터 주변 보라색 오라

### 스킬 연출
- **Shield Wall**: 반투명 방패 이펙트가 대상 아군 앞에 전개. 기존 Shield VFX 재사용. 강화 시 방패가 더 크고 빛남
- **Provoking Shield**: Duran이 방패를 타격하며 적을 도발. 적 머리 위 빨간 느낌표. 기존 Debuff VFX 재사용
- **Revenge Strike**: 보라색 잔상과 함께 돌진. 기존 Slash VFX에 보라 색조. Vengeance가 높을수록 잔상 길어짐
- **Last Bastion**: Duran이 방패를 땅에 내리꽎으면서 황금빛 충격파. 모든 적에게 빨간 도발 표식. Duran에게 황금빛 보호막 이펙트

### Vengeance 축적 연출
- 쉴드가 데미지를 흡수할 때마다 Duran 캐릭터에 짧은 보라색 입자 이펙트 (0.3초)
- 매 피격마다 "+12 Vengeance" 플로팅 텍스트
- 원격 흡수(아군 쉴드) 시에도 Duran에게 같은 연출 — "원격으로 에너지를 받음" 시각화

---

## 14. 구현 관점

### Character 클래스 확장
```csharp
// Character.cs에 추가
public int Vengeance { get; private set; } = 0;
public const int VENGEANCE_MAX = 20;

public void AddVengeance(int amount) {
    Vengeance = Mathf.Min(VENGEANCE_MAX, Vengeance + amount);
    OnVengeanceChanged?.Invoke(Vengeance);
}

public void ConsumeVengeance(int amount) {
    Vengeance = Mathf.Max(0, Vengeance - amount);
    OnVengeanceChanged?.Invoke(Vengeance);
}
```

### HealthComponent 이벤트 확장
```csharp
// 쉴드 흡수 시 이벤트 발생
public event Action<Character, int> OnShieldAbsorbed; // (대상, 흡수량)

// 쉴드가 데미지를 흡수했을 때
private void AbsorbDamageWithShield(int damage) {
    int absorbed = Mathf.Min(_currentShield, damage);
    _currentShield -= absorbed;
    OnShieldAbsorbed?.Invoke(this, absorbed); // 새 이벤트
    // ...
}
```

### Vengeance 축적 로직 — 자연 감소 없음, 피격/흡수 시에만 축적
```csharp
// Duran이 직접 맞을 때
Duran.Health.OnDamageTaken += (delta) => {
    Duran.AddVengeance(Mathf.Abs(delta));
};

// Duran이 부여한 쉴드가 다른 아군에서 흡수할 때 (원격 흡수)
아군.Health.OnShieldAbsorbed += (target, absorbed) => {
    if (target.Health.HasShieldFrom(Duran)) { // Duran이 부여한 쉴드인지
        Duran.AddVengeance(absorbed);
    }
};

// 주의: 턴 종료 시 자연 감소 없음.
// Vengeance는 오직 (1) 피격/흡수 축적 (2) 소비 스킬 사용 으로만 변동.
// Duran이 안 맞으면 자연히 쌓이지 않으므로 자연 제한이 이미 존재.
```

---

## 15. 후속 작업 후보

- [ ] 특성 3종 재설계 (CharacterConceptReview.md 섹션 6 — 기존 Warrior 특성은 구 컨셉이라 Vengeance에 맞춰 재작성)
- [ ] 어센션 × Vengeance 상호작용 시뮬레이션 (어센션 10 + Vengeance 캡 20 도달 시나리오)
- [ ] Duran 스킬 에셋 DataGenerator 확장 (Shield Wall, Provoking Shield, Revenge Strike, Last Bastion — 4개 스킬 CSV/SkillData)
- [ ] ForcedTarget 메카닉 확인 (기존 StatusEffectType.ForcedTarget — Provoking Shield/Last Bastion 도발에 활용)
