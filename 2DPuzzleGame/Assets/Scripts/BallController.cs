using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool IsTouch = false;

    RectTransform RectTransform;

    // 移動用のVectro2
    public Vector2 CurrentPos;

    // 盤面判定用のVector2(0,0)は一番左上の左端
    public Vector2 BoardPos;

    public bool DestroyFlag = false;

    public Sprite[] sprites = new Sprite[6];

    public enum BallType
    {
        Invalide = -1,
        Water,
        Wind,
        Fire,
        Dark,
        Light,
        Heal,
        Num
    }

    public BallType ThisBallType = BallType.Invalide;


    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();

        SetRandomType();

        StartCoroutine(SetCurrentPos());
    }

    public void SetRandomType()
    {
        var randomType = Random.Range(0, (int)BallType.Num);

        GetComponent<Image>().sprite = sprites[randomType];

        ThisBallType = (BallType)randomType;
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
