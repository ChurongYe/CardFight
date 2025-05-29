using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HurtUI : MonoBehaviour
{
    public GameObject bloodRed;            // ��ɫѪ��
    private Vector2 originalSize;          // Ѫ��ԭʼ���
    private RectTransform bloodRect;
    public TMP_Text damageText;
    private Canvas _canvas;

    public float floatUpDistance = 30f;
    public float duration = 0.8f;


    void Awake()
    {
        bloodRect = bloodRed.GetComponent<RectTransform>();
        originalSize = bloodRect.sizeDelta;

        // �Զ���ȡ�������е� TextMeshPro ���
        if (damageText == null)
        {
            damageText = GetComponentInChildren<TextMeshPro>();
            if (damageText == null)
            {
                Debug.LogWarning("HurtUI �Ҳ��� TextMeshPro �����");
            }
        }
        _canvas = GetComponentInChildren<Canvas>();
    }
    public void UpdateHealthBar(int current, int max)
    {
        float ratio = Mathf.Clamp01((float)current / max);
        bloodRect.sizeDelta = new Vector2(originalSize.x * ratio, originalSize.y);
    }
    public void ShowDamage(int damage, bool isCrit)
    {
        var go = Instantiate(damageText, _canvas.transform);
        go.text = damage.ToString();
        go.color = isCrit ? Color.red : Color.white;
        go.fontSize = isCrit ? 36 : 24;
        // ����ƫ��
        float xOffset = Random.Range(-0.5f, 0.5f);
        float yOffset = Random.Range(0f, 1f); 
        go.transform.localPosition = new Vector3(xOffset, yOffset, 0f);
        go.gameObject.SetActive(true);
        StartCoroutine(FadeAndMove(go));
    }
    IEnumerator FadeAndMove(TMP_Text text)
    {
        yield return new WaitForSeconds(0.8f);
        Color startColor = text.color;

        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;

            // ����
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(text.gameObject);
    }

}
