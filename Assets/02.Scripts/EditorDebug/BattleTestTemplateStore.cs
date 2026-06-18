using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TeamLog.EditorDebug
{
    /// <summary>
    /// 파티/유물/적 조합 템플릿 단위. 인덱스 기반 저장.
    /// 인덱스는 BattleTestSceneSetup의 드롭다운 value와 1:1 대응 (0="(없음)").
    /// 에셋 풀 순서가 바뀌면 무효화될 수 있음 (디버그 도구이므로 허용).
    /// </summary>
    [Serializable]
    public class BattleTestTemplate
    {
        public string name;
        public int[] indices;
        public int floorIndex;  // 적 템플릿 전용
        public bool isBoss;      // 적 템플릿 전용
    }

    /// <summary>
    /// 3카테고리 템플릿 저장소. JSON 직렬화로 Application.persistentDataPath에 영속화.
    /// CLAUDE.md 규칙 준수 — PlayerPrefs 대신 파일 I/O 사용.
    /// </summary>
    [Serializable]
    public class BattleTestTemplateStore
    {
        public List<BattleTestTemplate> party = new List<BattleTestTemplate>();
        public List<BattleTestTemplate> relic = new List<BattleTestTemplate>();
        public List<BattleTestTemplate> enemy = new List<BattleTestTemplate>();

        private const string FILE_NAME = "TeamLog_BattleTemplates.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

        /// <summary>
        /// 디스크에서 스토어 로드. 파일 없거나 오류 시 빈 스토어 반환.
        /// </summary>
        public static BattleTestTemplateStore Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var store = JsonUtility.FromJson<BattleTestTemplateStore>(json);
                    if (store != null)
                    {
                        store.party ??= new List<BattleTestTemplate>();
                        store.relic ??= new List<BattleTestTemplate>();
                        store.enemy ??= new List<BattleTestTemplate>();
                        return store;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BattleTestTemplateStore] 로드 실패: {e}");
            }
            return new BattleTestTemplateStore();
        }

        /// <summary>
        /// 디스크에 저장. 실패 시 로그만 출력 (데이터는 메모리에 유지).
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BattleTestTemplateStore] 저장 실패: {e}");
            }
        }
    }
}
