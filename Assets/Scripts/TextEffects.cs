// Assets/Scripts/TextEffects.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class TextEffects : MonoBehaviour
{
    [Header("Player & Enemy Values")]
    [SerializeField] private TMP_Text textValuePlayer;
    [SerializeField] private TMP_Text textValueEnemy;

    [Header("Skill Announcement Texts (LeftLeft, Left, Center, Right, RightRight)")]
    [SerializeField] private TMP_Text skillTextLeftLeft;   // 0
    [SerializeField] private TMP_Text skillTextLeft;       // 1
    [SerializeField] private TMP_Text skillTextCenter;     // 2
    [SerializeField] private TMP_Text skillTextRight;      // 3
    [SerializeField] private TMP_Text skillTextRightRight; // 4

    [Header("Optional Fallback Font (for accents)")]
    [SerializeField] private TMP_FontAsset fallbackFontForAccents;

    [Header("Value Minimal Style")]
    public float valueFontSize = 30f;
    public float valueRiseDistance = 14f;
    public float valueAppearTime = 0.25f;
    public float valueHoldTime = 0.9f;
    public float valueDisappearTime = 0.3f;

    [Header("Colors")]
    public Color holoColor = new Color(0f, 1f, 1f);
    public Color enemyColor = new Color(1f, 0.623f, 0.408f);

    [Header("Skill FX Settings (legacy 3-text)")]
    public float skillAppearTime = 0.6f;
    public float skillDisplayTime = 2.5f;
    public float skillDisappearTime = 0.6f;
    public Vector3 skillScale = new Vector3(1f, 1f, 1f);

    [Header("Skill Font Sizes")]
    public float skillFontSizeSingle = 32f;
    public float skillFontSizeMulti = 24f;
    public float skillFontSizeMin = 20f;

    [Header("NEW: 5-skill Unfold (center -> pairs)")]
    [Tooltip("X offsets for LeftLeft, Left, Center, Right, RightRight")]
    [SerializeField] private float[] xOffsets = new float[5] { -380f, -190f, 0f, 190f, 380f };
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float unfoldMoveTime = 0.6f;
    [SerializeField] private float unfoldStagger = 0.08f;
    [SerializeField] private float unfoldFadeOutTime = 0.4f;

    [Header("Initial alpha per ring (base -> pendant le move on va vers MaxAlpha)")]
    [Range(0f, 1f)] public float outerInitialAlpha = 0.6f;
    [Range(0f, 1f)] public float midInitialAlpha = 0.85f;
    [Range(0f, 1f)] public float centerInitialAlpha = 1f;

    [Header("Max alpha per ring (plafond visuel, ne monte pas à 1 si non désiré)")]
    [Range(0f, 1f)] public float outerMaxAlpha = 0.75f;
    [Range(0f, 1f)] public float midMaxAlpha = 0.9f;
    [Range(0f, 1f)] public float centerMaxAlpha = 1f;

    [Header("Hold after arrival per ring (propagation outer -> center)")]
    public float outerHoldAfterArrival = 0.15f;
    public float midHoldAfterArrival = 0.30f;
    public float centerHoldAfterArrival = 0.45f;

    private Coroutine playerValueRoutine;
    private Coroutine enemyValueRoutine;
    private Vector2 basePosPlayer;
    private Vector2 basePosEnemy;

    void OnValidate()
    {
        if (xOffsets == null || xOffsets.Length != 5)
            xOffsets = new float[5] { -380f, -190f, 0f, 190f, 380f };

        outerInitialAlpha = Mathf.Clamp01(outerInitialAlpha);
        midInitialAlpha = Mathf.Clamp01(midInitialAlpha);
        centerInitialAlpha = Mathf.Clamp01(centerInitialAlpha);

        outerMaxAlpha = Mathf.Clamp01(outerMaxAlpha);
        midMaxAlpha = Mathf.Clamp01(midMaxAlpha);
        centerMaxAlpha = Mathf.Clamp01(centerMaxAlpha);
    }

    void Awake()
    {
        if (textValuePlayer) basePosPlayer = textValuePlayer.rectTransform.anchoredPosition;
        if (textValueEnemy) basePosEnemy = textValueEnemy.rectTransform.anchoredPosition;
    }

    // ========================= VALUES =========================
    public void AnimTextPlayer(int value)
    {
        if (playerValueRoutine != null) StopCoroutine(playerValueRoutine);
        playerValueRoutine = StartCoroutine(AnimateValueMinimalNoFade(textValuePlayer, value, holoColor));
    }

    public void AnimTextEnemy(int value)
    {
        if (enemyValueRoutine != null) StopCoroutine(enemyValueRoutine);
        enemyValueRoutine = StartCoroutine(AnimateEnemyAndFadeBoth(textValueEnemy, value, enemyColor));
    }

    private IEnumerator AnimateValueMinimalNoFade(TMP_Text targetText, int value, Color color)
    {
        if (!targetText) yield break;
        targetText.SetText(value.ToString());
        targetText.color = new Color(color.r, color.g, color.b, 1f); // why: éviter alpha color < 1 qui multiplie
        targetText.fontSize = valueFontSize;
        targetText.alpha = 0f;

        var rt = targetText.rectTransform;
        Vector2 startPos = (targetText == textValuePlayer) ? basePosPlayer : basePosEnemy;
        Vector2 endPos = startPos + new Vector2(0f, valueRiseDistance);
        rt.anchoredPosition = startPos;

        float t = 0f;
        while (t < valueAppearTime)
        {
            t += Time.deltaTime;
            float k = t / valueAppearTime;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, k));
            targetText.alpha = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }
        rt.anchoredPosition = endPos;
        targetText.alpha = 1f;
        yield return new WaitForSeconds(valueHoldTime);
    }

    private IEnumerator AnimateEnemyAndFadeBoth(TMP_Text enemyText, int value, Color color)
    {
        if (!enemyText) yield break;
        enemyText.SetText(value.ToString());
        enemyText.color = new Color(color.r, color.g, color.b, 1f);
        enemyText.fontSize = valueFontSize;
        enemyText.alpha = 0f;

        var rtEnemy = enemyText.rectTransform;
        Vector2 startPosEnemy = basePosEnemy;
        Vector2 endPosEnemy = startPosEnemy + new Vector2(0f, valueRiseDistance);
        rtEnemy.anchoredPosition = startPosEnemy;

        float t = 0f;
        while (t < valueAppearTime)
        {
            t += Time.deltaTime;
            float k = t / valueAppearTime;
            rtEnemy.anchoredPosition = Vector2.Lerp(startPosEnemy, endPosEnemy, Mathf.SmoothStep(0f, 1f, k));
            enemyText.alpha = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }
        rtEnemy.anchoredPosition = endPosEnemy;
        enemyText.alpha = 1f;

        yield return new WaitForSeconds(valueHoldTime);

        if (textValuePlayer != null)
            StartCoroutine(FadeOutValue(textValuePlayer, basePosPlayer));
        yield return FadeOutValue(enemyText, basePosEnemy);
    }

    private IEnumerator FadeOutValue(TMP_Text targetText, Vector2 resetPos)
    {
        if (!targetText) yield break;
        float t = 0f;
        while (t < valueDisappearTime)
        {
            t += Time.deltaTime;
            float k = t / valueDisappearTime;
            targetText.alpha = Mathf.Lerp(1f, 0f, k);
            yield return null;
        }
        targetText.alpha = 0f;
        targetText.rectTransform.anchoredPosition = resetPos;
        targetText.SetText(string.Empty);
    }

    public void FadeOutPlayerValue(float delay = 0f)
    {
        if (playerValueRoutine != null)
        {
            StopCoroutine(playerValueRoutine);
            playerValueRoutine = null;
        }
        StartCoroutine(FadeOutValueDelayed(textValuePlayer, basePosPlayer, delay));
    }

    private IEnumerator FadeOutValueDelayed(TMP_Text targetText, Vector2 resetPos, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        yield return FadeOutValue(targetText, resetPos);
    }

    // ========================= NEW: 5 SKILLS (symétrique + alpha cap) =========================
    public void ShowSkillTextFive(string skillName)
    {
        TMP_Text[] slots = { skillTextLeftLeft, skillTextLeft, skillTextCenter, skillTextRight, skillTextRightRight };
        string formatted = FormatSkill(skillName, insertNewlineAfterFirstWord: true);

        for (int i = 0; i < slots.Length; i++)
        {
            var t = slots[i];
            if (!t) continue;
            PrepareTMP(t);
            t.fontSize = ComputeSkillFontSize(formatted);
            t.text = formatted;
            t.rectTransform.anchoredPosition = Vector2.zero;

            // alpha init par anneau (et couleur alpha = 1)
            float initA = GetInitialAlphaByIndex(i);
            Color c = t.color; t.color = new Color(c.r, c.g, c.b, 1f);
            t.alpha = initA;
        }

        StartCoroutine(Unfold5_PropagationFade(slots));
    }

    private IEnumerator Unfold5_PropagationFade(TMP_Text[] slots)
    {
        int[][] rings =
        {
            new[] { 2 },      // center
            new[] { 1, 3 },   // mid
            new[] { 0, 4 }    // outer
        };

        float longest = 0f;
        for (int step = 0; step < rings.Length; step++)
        {
            foreach (int idx in rings[step])
            {
                TMP_Text t = slots[idx];
                if (!t) continue;

                Vector2 to = new Vector2(xOffsets[idx], yOffset);
                float moveDelay = unfoldStagger * step;
                float initA = GetInitialAlphaByIndex(idx);
                float maxA = GetMaxAlphaByIndex(idx);
                float hold = GetHoldAfterArrivalByIndex(idx);

                StartCoroutine(MoveThenWaitThenFade(
                    t, Vector2.zero, to,
                    unfoldMoveTime, moveDelay,
                    initA, maxA, hold, unfoldFadeOutTime));

                longest = Mathf.Max(longest, moveDelay + unfoldMoveTime + hold + unfoldFadeOutTime);
            }
        }

        yield return new WaitForSeconds(longest + 0.02f);

        for (int i = 0; i < slots.Length; i++)
            if (slots[i]) slots[i].text = string.Empty;
    }

    private float GetInitialAlphaByIndex(int idx)
    {
        if (idx == 2) return centerInitialAlpha;
        if (idx == 1 || idx == 3) return midInitialAlpha;
        return outerInitialAlpha;
    }

    private float GetMaxAlphaByIndex(int idx)
    {
        if (idx == 2) return centerMaxAlpha;
        if (idx == 1 || idx == 3) return midMaxAlpha;
        return outerMaxAlpha;
    }

    private float GetHoldAfterArrivalByIndex(int idx)
    {
        if (idx == 2) return centerHoldAfterArrival;
        if (idx == 1 || idx == 3) return midHoldAfterArrival;
        return outerHoldAfterArrival;
    }

    private IEnumerator MoveThenWaitThenFade(
        TMP_Text t,
        Vector2 from, Vector2 to,
        float moveDuration, float moveDelay,
        float initialAlpha, float maxAlpha,
        float holdAfterArrival, float fadeDuration)
    {
        if (!t) yield break;
        if (moveDelay > 0f) yield return new WaitForSeconds(moveDelay);

        var rt = t.rectTransform;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            t.alpha = Mathf.Lerp(initialAlpha, maxAlpha, k); // <= plafonné à maxAlpha
            yield return null;
        }
        rt.anchoredPosition = to;
        t.alpha = maxAlpha; // <= reste au max

        if (holdAfterArrival > 0f)
            yield return new WaitForSeconds(holdAfterArrival);

        float f = 0f;
        while (f < fadeDuration)
        {
            f += Time.deltaTime;
            float k = Mathf.Clamp01(f / fadeDuration);
            t.alpha = Mathf.Lerp(maxAlpha, 0f, k);
            yield return null;
        }
        t.alpha = 0f;
    }

    // ========================= (Legacy) 3-skill helpers =========================
    public void ShowSkillText(string skillName)
    {
        string formatted = FormatSkill(skillName, insertNewlineAfterFirstWord: true);

        PrepareTMP(skillTextLeft);
        PrepareTMP(skillTextCenter);
        PrepareTMP(skillTextRight);

        float size = ComputeSkillFontSize(formatted);
        if (skillTextLeft) skillTextLeft.fontSize = size;
        if (skillTextCenter) skillTextCenter.fontSize = size;
        if (skillTextRight) skillTextRight.fontSize = size;

        if (skillTextLeft) skillTextLeft.text = formatted;
        if (skillTextCenter) skillTextCenter.text = formatted;
        if (skillTextRight) skillTextRight.text = formatted;

        if (skillTextLeft) StartCoroutine(AnimateSkillText(skillTextLeft, false));
        if (skillTextCenter) StartCoroutine(AnimateSkillText(skillTextCenter, false));
        if (skillTextRight) StartCoroutine(AnimateSkillText(skillTextRight, false));
    }

    private string FormatSkill(string s, bool insertNewlineAfterFirstWord)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Replace("/n", "\n").Replace("\\n", "\n");

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (i > 0 && char.IsUpper(c) && s[i - 1] != ' ' && s[i - 1] != '\n')
                sb.Append(' ');
            sb.Append(c);
        }
        s = sb.ToString();

        if (insertNewlineAfterFirstWord && !s.Contains("\n"))
        {
            int i = s.IndexOf(' ');
            if (i > 0 && i < s.Length - 1)
                s = s.Substring(0, i) + "\n" + s.Substring(i + 1);
        }
        return s;
    }

    private void PrepareTMP(TMP_Text t)
    {
        if (!t) return;
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Overflow;
        t.richText = true;

        // why: s'assurer que la couleur n’écrase pas alpha via multiplication
        var baseColor = holoColor;
        t.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        if (fallbackFontForAccents != null)
        {
            var current = t.font;
            if (current == null || current.fallbackFontAssetTable == null || !current.fallbackFontAssetTable.Contains(fallbackFontForAccents))
            {
                var inst = Instantiate(current);
                if (inst.fallbackFontAssetTable == null)
                    inst.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                inst.fallbackFontAssetTable.Add(fallbackFontForAccents);
                t.font = inst;
            }
        }
    }

    private IEnumerator AnimateSkillText(TMP_Text targetText, bool withGlitch)
    {
        if (!targetText) yield break;
        var rt = targetText.rectTransform;
        targetText.alpha = 0f;
        rt.localScale = Vector3.zero;

        float t = 0f;
        while (t < skillAppearTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / skillAppearTime);
            rt.localScale = Vector3.Lerp(Vector3.zero, skillScale, k);
            targetText.alpha = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }

        float elapsed = 0f;
        while (elapsed < skillDisplayTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        Vector3 startScale = rt.localScale;
        while (t < skillDisappearTime)
        {
            t += Time.deltaTime;
            float k = t / skillDisappearTime;
            rt.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            targetText.alpha = Mathf.Lerp(1f, 0f, k);
            yield return null;
        }
        targetText.text = string.Empty;
    }

    private float ComputeSkillFontSize(string text)
    {
        bool multi = text.Contains(" ") || text.Contains("\n");
        float size = multi ? skillFontSizeMulti : skillFontSizeSingle;

        string[] lines = text.Split('\n');
        int maxLen = 0;
        foreach (var line in lines) maxLen = Mathf.Max(maxLen, line.Length);

        if (maxLen > 12)
        {
            float factor = Mathf.Clamp(12f / maxLen, 0.6f, 1f);
            size *= factor;
        }
        return Mathf.Max(skillFontSizeMin, size);
    }
}
