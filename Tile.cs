using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // 타일 오브젝트의 SpriteRenderer를 필드로 받음
        if (spriteRenderer == null)
        {
            Debug.Log("spriteRendererisnull");
        }
    }
    public Color Color // 타일 오브젝트의 '색깔'에 접근(SpriteRenderer 컴포넌트의 정적 멤버 변수)
    {
        set
        {
            spriteRenderer.color = value;
        }

        get
        {
            return spriteRenderer.color;
        }
    }

    public int sortingOrder // 타일 오브젝트의 '정렬 순서'에 접근
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }
        get
        {
            return spriteRenderer.sortingOrder;
        }
    }
}
