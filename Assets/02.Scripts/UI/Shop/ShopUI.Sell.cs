using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Reward;

namespace TeamLog.UI.Shop
{
    /// <summary>
    /// ShopUI 판매 탭 로직 (partial) — 유물 판매만 지원 (고정 스킬셋)
    /// 진입점: ShopUI.cs
    /// </summary>
    public partial class ShopUI
    {
        private int _currentFloorNumber;

        /// <summary>
        /// 판매 목록 새로고침 — 획득 유물 표시
        /// </summary>
        private void RefreshSellList()
        {
            if (_sellContainer == null || _runState == null) return;

            ClearSellList();

            var font = _goldLabel?.font;

            // 획득 유물 목록
            if (_runState.RelicHandler.Relics.Count > 0)
            {
                CreateSellHeader("유물", font);
                foreach (var relic in _runState.RelicHandler.Relics)
                {
                    var capturedRelic = relic;
                    CreateSellItem(
                        relic.Icon,
                        relic.RelicName,
                        _shopManager.GetRelicSellPrice(_currentFloorNumber),
                        () => OnSellRelic(capturedRelic));
                }
            }

            if (_sellContainer.childCount == 0)
            {
                CreateSellHeader("판매할 수 있는 항목이 없습니다", font);
            }
        }

        private void OnSellRelic(RelicData relic)
        {
            if (_confirmationDialog != null)
            {
                int price = _shopManager.GetRelicSellPrice(_currentFloorNumber);
                _confirmationDialog.Show(
                    $"{relic.RelicName}을(를) {price}G에 판매하시겠습니까?",
                    () => ConfirmSellRelic(relic));
            }
            else
            {
                ConfirmSellRelic(relic);
            }
        }

        private void ConfirmSellRelic(RelicData relic)
        {
            if (_shopManager.SellRelic(relic, _runState, _currentFloorNumber))
            {
                int price = _shopManager.GetRelicSellPrice(_currentFloorNumber);
                UpdateGoldDisplay();
                RefreshSellList();
                AudioManager.Instance?.PlayUIGoldSpend();
                ToastUI.Show($"{relic.RelicName}을(를) {price}G에 판매했습니다.");
            }
        }

        private void CreateSellHeader(string text, TMP_FontAsset font)
        {
            var labelObj = new GameObject("SellHeader");
            labelObj.transform.SetParent(_sellContainer, false);
            var rect = labelObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 28);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.96f, 0.82f, 0.25f);
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = false;
        }

        private void CreateSellItem(Sprite icon, string name, int price, System.Action onSell)
        {
            var row = new GameObject("SellItem");
            row.transform.SetParent(_sellContainer, false);
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 40);

            var rowImg = row.AddComponent<Image>();
            rowImg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // 아이콘
            if (icon != null)
            {
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(row.transform, false);
                var iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(32, 32);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.raycastTarget = false;
            }

            // 이름
            var nameLabel = new GameObject("NameLabel");
            nameLabel.transform.SetParent(row.transform, false);
            var nameRect = nameLabel.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(280, 32);
            var nameTmp = nameLabel.AddComponent<TextMeshProUGUI>();
            nameTmp.font = _goldLabel?.font;
            nameTmp.text = name;
            nameTmp.fontSize = 16;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.enableWordWrapping = false;
            nameTmp.raycastTarget = false;

            // 가격
            var priceLabel = new GameObject("PriceLabel");
            priceLabel.transform.SetParent(row.transform, false);
            var priceRect = priceLabel.AddComponent<RectTransform>();
            priceRect.sizeDelta = new Vector2(80, 32);
            var priceTmp = priceLabel.AddComponent<TextMeshProUGUI>();
            priceTmp.font = _goldLabel?.font;
            priceTmp.text = $"{price} G";
            priceTmp.fontSize = 14;
            priceTmp.color = new Color(0.96f, 0.82f, 0.25f);
            priceTmp.alignment = TextAlignmentOptions.MidlineRight;
            priceTmp.enableWordWrapping = false;
            priceTmp.raycastTarget = false;

            // 판매 버튼
            var sellBtn = new GameObject("SellButton");
            sellBtn.transform.SetParent(row.transform, false);
            var sellBtnRect = sellBtn.AddComponent<RectTransform>();
            sellBtnRect.sizeDelta = new Vector2(60, 28);
            var sellBtnImg = sellBtn.AddComponent<Image>();
            sellBtnImg.color = new Color(0.6f, 0.3f, 0.15f);
            var btn = sellBtn.AddComponent<Button>();
            btn.targetGraphic = sellBtnImg;
            btn.onClick.AddListener(() => onSell());

            var btnText = new GameObject("T");
            btnText.transform.SetParent(sellBtn.transform, false);
            SetFillParent(btnText.GetComponent<RectTransform>());
            var btnTmp = btnText.AddComponent<TextMeshProUGUI>();
            btnTmp.font = _goldLabel?.font;
            btnTmp.text = "판매";
            btnTmp.fontSize = 14;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.color = Color.white;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.raycastTarget = false;
        }

        private void ClearSellList()
        {
            if (_sellContainer == null) return;
            for (int i = _sellContainer.childCount - 1; i >= 0; i--)
                Destroy(_sellContainer.GetChild(i).gameObject);
        }

        private static void SetFillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
