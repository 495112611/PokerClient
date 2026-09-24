using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂在战斗背景上。背景被点击时，取消当前玩家选中的所有手牌。
/// 卡牌和按钮会拦截自己的点击，不会误触发这里。
/// </summary>
public sealed class BlankClickCatcher : MonoBehaviour, IPointerDownHandler
{
    public BattlePanel panel;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        if (target != null && (target.GetComponentInParent<CardUI>() != null ||
            target.GetComponentInParent<Selectable>() != null))
            return;
        panel?.ClearSelectedCards();
    }
}
