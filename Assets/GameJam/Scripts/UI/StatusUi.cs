using System.Collections;
using TMPro;
using UnityEngine;

public class StatusUi : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas canvas;

    private void OnEnable()
    {
        Health.Damaged += OnDamaged;
        Health.Healed += OnHealed;
    }

    private void OnDisable()
    {
        Health.Damaged -= OnDamaged;
        Health.Healed -= OnHealed;
    }

    private void OnDamaged(Health health, float amount)
    {
        if (health == null) return;
        int value = Mathf.Max(1, Mathf.RoundToInt(amount));
        ShowDamage(value, health.transform);
    }

    private void OnHealed(Health health, float amount)
    {
        if (health == null) return;
        int value = Mathf.Max(1, Mathf.RoundToInt(amount));
        ShowHealth(value, health.transform);
    }

    private IEnumerator textdestroction(GameObject texto)
    {
        yield return new WaitForSeconds(1f);
        Destroy(texto);
    }
    public void ShowDamage(int damage, Transform parent)
    {
        texting(parent, "-" + damage.ToString(), Color.yellow);
    }

    public void ShowHealth(int health, Transform parent)
    {
        texting(parent, "+" + health.ToString(), Color.green);
    }
    
    public void showCriticalDamage(int damage, Transform parent)
    {
        texting(parent, "-" + damage.ToString() + "!", Color.red);
    }
    
    public void showCriticalHeal(int heal, Transform parent)
    {
        texting(parent, "+" + heal.ToString() + "!", Color.green);
    }
    private void texting(Transform parent, string text, Color color = new Color())
    {
        if (textPrefab == null)
        {
            return;
        }

        GameObject texto;
        if (canvas == null)
        {
            texto = Instantiate(textPrefab, parent.position, Quaternion.identity);
        }
        else
        {
            texto = Instantiate(textPrefab, canvas.transform);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform textoRect = texto.GetComponent<RectTransform>();
            Camera camera = Camera.main;

            if (canvasRect != null && textoRect != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, parent.position);
                Vector2 localPoint;
                Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out localPoint))
                {
                    textoRect.anchoredPosition = localPoint;
                }
            }
        }

        TMP_Text tmp = texto.GetComponent<TMP_Text>();
        
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        Rigidbody2D rb = texto.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up;
        }

        if (canvas == null)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                texto.transform.LookAt(camera.transform);
            }
        }
        
        StartCoroutine(textdestroction(texto));

    }
    
}
