using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool IsTouch = false;

    RectTransform RectTransform;

    public Vector2 CurrentPos;

    public Sprite[] sprites = new Sprite[6];

    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();

        var randomColor = Random.Range(0, 6);

        GetComponent<Image>().sprite = sprites[randomColor];


        StartCoroutine(SetCurrentPos());
    }

    IEnumerator SetCurrentPos()
    {
        yield return new WaitForEndOfFrame();
        CurrentPos = RectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsTouch)
        {
            RectTransform.position = Input.mousePosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsTouch = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsTouch = false;
        this.transform.position = CurrentPos;
    }

    public void SetPos(Vector3 nextPos)
    {
        this.RectTransform.position = nextPos;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTouch)
        {
            var afterPos = CurrentPos;
            collision.GetComponent<BallController>().SetPos(CurrentPos);
            CurrentPos = collision.GetComponent<BallController>().CurrentPos;
             collision.GetComponent<BallController>().CurrentPos = afterPos;
        }
    }
}
