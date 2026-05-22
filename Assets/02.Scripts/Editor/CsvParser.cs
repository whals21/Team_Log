#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TeamLog.Editor
{
    /// <summary>
    /// Editor-only CSV 파서 — 헤더 기반 컬럼 접근, UTF-8 BOM 지원, 주석/빈 줄 스킵
    /// </summary>
    public class CsvParser
    {
        private readonly string[] _headers;
        private readonly List<string[]> _rows = new List<string[]>();

        public int RowCount => _rows.Count;

        /// <summary>
        /// CSV 파일 경로에서 파싱
        /// </summary>
        public CsvParser(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"[CsvParser] 파일 없음: {csvPath}");
                _headers = new string[0];
                return;
            }

            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);

            if (lines.Length == 0)
            {
                _headers = new string[0];
                return;
            }

            // 첫 줄 = 헤더
            _headers = SplitCsvLine(lines[0]);

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 빈 줄, 주석(#) 스킵
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                _rows.Add(SplitCsvLine(line));
            }
        }

        /// <summary>
        /// row 인덱스의 컬럼 값을 헤더 이름으로 조회
        /// </summary>
        public string Get(int row, string column)
        {
            int colIndex = FindColumnIndex(column);
            if (colIndex < 0) return "";
            if (row < 0 || row >= _rows.Count) return "";

            var cells = _rows[row];
            if (colIndex >= cells.Length) return "";

            return cells[colIndex];
        }

        /// <summary>
        /// row 인덱스의 컬럼 int 값을 헤더 이름으로 조회
        /// </summary>
        public int GetInt(int row, string column, int defaultValue = 0)
        {
            var raw = Get(row, column);
            return int.TryParse(raw, out var val) ? val : defaultValue;
        }

        private int FindColumnIndex(string column)
        {
            for (int i = 0; i < _headers.Length; i++)
                if (_headers[i].Trim() == column)
                    return i;
            return -1;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }
    }
}
#endif
