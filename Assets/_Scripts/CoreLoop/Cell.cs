using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite highlight;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }
    public void Normal()
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;
        spriteRenderer.sprite = normal;
    }
    public void Highlight()
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;
        spriteRenderer.sprite = highlight;
    }
    public void Hover() 
    {
        gameObject.SetActive(true);
        spriteRenderer.color = new(1.0f, 1.0f, 1.0f, 0.5f);
        spriteRenderer.sprite = normal;
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
    public void PlayBreakAnimation(float delay)
    {
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);

        transform.localScale = originalScale;
        spriteRenderer.sprite = highlight;
        spriteRenderer.color = Color.white;
        gameObject.SetActive(true);

        Sequence s = DOTween.Sequence();

        s.AppendInterval(delay);

        s.Append(transform.DOScale(originalScale * 1.5f, 0.3f).SetEase(Ease.OutQuad));
        s.Join(spriteRenderer.DOFade(0f, 0.3f).SetEase(Ease.InQuad));

        s.OnComplete(() => {
            transform.localScale = originalScale;
            Hide();
        });
    }
}
