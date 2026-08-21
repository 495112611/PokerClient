using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    private Card card;
    private bool isSelect;
    private Image image;
    private bool isUp;

    private Color darkColor = new Color(0.6f, 0.6f, 0.6f, 1);
    private Color lightColor = new Color(1, 1, 1, 1);
    public bool IsSelect
    {
        get => isSelect;
        set
        {
            isSelect = value;
            if (isSelect)
            {
                image.color = darkColor;
            }
            else
            {
                image.color = lightColor;
            }
        }
    }

    private void Start()
    {
        card = CardManager.GetCard(gameObject.name);
        image = GetComponent<Image>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            GameManager.isPressing = false;
            if (IsSelect)
            {
                IsSelect = false;
                if (isUp)
                {
                    transform.position -= Vector3.up * 6;
                    isUp = false;
                    if (GameManager.selectCard.Contains(card))
                    {
                        GameManager.selectCard.Remove(card);
                    }
                }
                else
                {
                    transform.position += Vector3.up * 6;
                    isUp = true;
                    if (!GameManager.selectCard.Contains(card))
                    {
                        GameManager.selectCard.Add(card);
                    }
                }
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.isPressing = true;
        if (IsSelect)
        {
            IsSelect = false;
        }
        else
        {
            IsSelect = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.isPressing)
        {
            if (IsSelect)
            {
                IsSelect = false;
            }
            else
            {
                IsSelect = true;
            }
        }
    }

}
