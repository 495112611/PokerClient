using UnityEngine;
using UnityEngine.UI;

// UI 只展示服务器广播的秒数，不在客户端触发超时或轮换。
public sealed class TurnTimerView : MonoBehaviour
{
    private RectTransform rect;
    private Text phaseText;
    private Text secondsText;
    private Image progress;
    private readonly Color normal = new Color(1f, 0.83f, 0.35f);
    private readonly Color urgent = new Color(1f, 0.32f, 0.25f);

    public void Initialize(Font font)
    {
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(110, 76);
        Image background = gameObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.12f, 0.2f, 0.92f);
        background.raycastTarget = false;
        Outline outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.96f, 0.7f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(1, -1);
        phaseText = CreateText("Phase", font, 18, new Vector2(0, 23), new Vector2(108, 25));
        secondsText = CreateText("Seconds", font, 36, new Vector2(0, -7), new Vector2(108, 46));
        secondsText.fontStyle = FontStyle.Bold;
        GameObject bar = new GameObject("Progress", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(transform, false);
        progress = bar.GetComponent<Image>();
        progress.raycastTarget = false;
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0);
        barRect.anchorMax = new Vector2(1, 0);
        barRect.pivot = new Vector2(0, 0);
        barRect.sizeDelta = new Vector2(0, 3);
        barRect.anchoredPosition = Vector2.zero;
        gameObject.SetActive(false);
    }

    private Text CreateText(string objectName, Font font, int size, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = dimensions;
        return text;
    }

    public void Show(MsgTurnState state)
    {
        bool knownPlayer = state.id == GameManager.id || state.id == GameManager.leftId || state.id == GameManager.rightId;
        gameObject.SetActive(state.active && knownPlayer);
        if (!gameObject.activeSelf)
            return;
        rect.anchoredPosition = state.id == GameManager.id ? new Vector2(-335, -105) :
            state.id == GameManager.leftId ? new Vector2(-380, 142) : new Vector2(380, 142);
        int seconds = Mathf.Max(0, state.secondsRemaining);
        phaseText.text = state.phase == 0 ? "叫地主" : state.phase == 1 ? "抢地主" : "出牌";
        secondsText.text = seconds.ToString();
        Color color = seconds <= 5 ? urgent : normal;
        phaseText.color = color;
        secondsText.color = color;
        progress.color = color;
        progress.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(seconds / (float)Mathf.Max(1, state.duration)), 0);
    }
}
