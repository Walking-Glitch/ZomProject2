using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > ZomProject > Build HUD: lays out the player HUD inside the scene's UIManager canvas and wires
// UIManager's references. Safe to run again: existing elements are found by name and repositioned,
// so restyle freely afterwards (rerunning resets positions, sizes and font sizes).
public static class HudBuilder
{
    private const float Margin = 20f;

    [MenuItem("Tools/ZomProject/Build HUD")]
    private static void Build()
    {
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null || ui.Money == null)
        {
            EditorUtility.DisplayDialog("Build HUD", "No UIManager with a Money text was found in the open scene.", "OK");
            return;
        }

        RectTransform panel = (RectTransform)ui.Money.transform.parent;
        TMP_FontAsset font = ui.Money.font;

        Undo.SetCurrentGroupName("Build HUD");
        int undoGroup = Undo.GetCurrentGroup();
        Undo.RecordObject(ui, "Build HUD");

        // top-left: health
        RectTransform health = ui.HealthFill != null ? ui.HealthFill.transform.parent as RectTransform : null;
        if (health != null)
        {
            float rightEdge = health.anchoredPosition.x + health.sizeDelta.x * (1f - health.pivot.x);
            ui.HealthText = Text(panel, "Health Text", font, 30, TextAlignmentOptions.Left,
                new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(rightEdge + 12f, health.anchoredPosition.y), new Vector2(200, 40));
        }

        Transform fps = panel.Find("FPS Text");
        if (fps != null)
        {
            Place(fps as RectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(Margin, -70), new Vector2(200, 30));
            ui.FpsText = fps.gameObject;
        }

        // top-center: cabins
        ui.CabinList = Rect(panel, "Cabin List", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -15), new Vector2(1000, 40));
        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(ui.CabinList.gameObject);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 30f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        ui.CabinRowTemplate = CabinRowTemplate(ui.CabinList, font);

        ui.ToastText = Text(panel, "Toast Text", font, 36, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1000, 50));
        ui.ToastText.color = new Color(1f, 0.85f, 0.3f);

        // top-right: day cycle, zombies, turret supplies
        Vector2 topRight = new Vector2(1, 1);
        Place(ui.Level.rectTransform, topRight, topRight, new Vector2(-Margin, -15), new Vector2(300, 40));
        Style(ui.Level, 34, TextAlignmentOptions.Right);

        DayCycle dayCycle = Object.FindFirstObjectByType<DayCycle>();
        if (dayCycle != null && dayCycle.timeText != null)
        {
            Place(dayCycle.timeText.rectTransform, topRight, topRight, new Vector2(-Margin, -55), new Vector2(300, 36));
            Style(dayCycle.timeText, 30, TextAlignmentOptions.Right);
        }

        ui.PhaseText = Text(panel, "Phase Text", font, 26, TextAlignmentOptions.Right, topRight, topRight, new Vector2(-Margin, -92), new Vector2(300, 32));
        ui.ZombieText = Text(panel, "Zombie Text", font, 26, TextAlignmentOptions.Right, topRight, topRight, new Vector2(-Margin, -126), new Vector2(300, 32));

        ui.TurretFuelFill = SupplyBar(panel, "Turret Fuel", font, new Vector2(-Margin, -170), out ui.TurretFuelText);
        ui.TurretAmmoFill = SupplyBar(panel, "Turret Ammo", font, new Vector2(-Margin, -215), out ui.TurretAmmoText);

        // bottom-left: build resources
        Vector2 bottomLeft = new Vector2(0, 0);
        ui.GeneratorsText = Text(panel, "Generators Text", font, 26, TextAlignmentOptions.Left, bottomLeft, bottomLeft, new Vector2(Margin, Margin), new Vector2(320, 34));
        ui.FuelBarrelsText = Text(panel, "Fuel Barrels Text", font, 26, TextAlignmentOptions.Left, bottomLeft, bottomLeft, new Vector2(Margin, Margin + 36), new Vector2(320, 34));
        ui.SparePartsText = Text(panel, "Spare Parts Text", font, 26, TextAlignmentOptions.Left, bottomLeft, bottomLeft, new Vector2(Margin, Margin + 72), new Vector2(320, 34));
        Place(ui.Money.rectTransform, bottomLeft, bottomLeft, new Vector2(Margin, Margin + 110), new Vector2(320, 44));
        Style(ui.Money, 36, TextAlignmentOptions.Left);
        ui.Money.color = new Color(0.55f, 1f, 0.55f);

        // bottom-right: weapon
        Vector2 bottomRight = new Vector2(1, 0);
        ui.FireModeText = Text(panel, "Fire Mode Text", font, 24, TextAlignmentOptions.Right, bottomRight, bottomRight, new Vector2(-Margin, Margin), new Vector2(300, 30));
        ui.AmmoText = Text(panel, "Ammo Text", font, 56, TextAlignmentOptions.Right, bottomRight, bottomRight, new Vector2(-Margin, Margin + 30), new Vector2(300, 66));

        // contextual
        if (ui.Interact != null)
        {
            Place(ui.Interact.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -140), ui.Interact.rectTransform.sizeDelta);
        }

        ui.BuildHintText = Text(panel, "Build Hint Text", font, 24, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, Margin), new Vector2(1300, 34));

        EditorUtility.SetDirty(ui);
        EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log("HUD built under " + panel.name + ". Save the scene to keep it.");
    }

    private static GameObject CabinRowTemplate(RectTransform list, TMP_FontAsset font)
    {
        RectTransform row = Rect(list, "Cabin Row Template", new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(300, 36));

        Vector2 left = new Vector2(0, 0.5f);
        Text(row, "Name", font, 24, TextAlignmentOptions.Left, left, left, new Vector2(0, 0), new Vector2(100, 34));

        RectTransform bar = Rect(row, "Bar", left, left, new Vector2(100, 0), new Vector2(140, 16));
        GetOrAdd<Image>(bar.gameObject).color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform fill = Rect(bar, "Fill", Vector2.zero, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        Stretch(fill);
        GetOrAdd<Image>(fill.gameObject).color = new Color(0.35f, 0.8f, 0.35f);

        Text(row, "Value", font, 22, TextAlignmentOptions.Right, left, left, new Vector2(245, 0), new Vector2(55, 34));

        row.gameObject.SetActive(false);
        return row.gameObject;
    }

    private static RectTransform SupplyBar(RectTransform panel, string name, TMP_FontAsset font, Vector2 position, out TextMeshProUGUI label)
    {
        Vector2 topRight = new Vector2(1, 1);
        RectTransform group = Rect(panel, name, topRight, topRight, position, new Vector2(300, 40));

        label = Text(group, "Label", font, 22, TextAlignmentOptions.Right, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(300, 24));

        RectTransform bar = Rect(group, "Bar", new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(300, 12));
        GetOrAdd<Image>(bar.gameObject).color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform fill = Rect(bar, "Fill", Vector2.zero, new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
        Stretch(fill);
        GetOrAdd<Image>(fill.gameObject).color = new Color(0.35f, 0.8f, 0.35f);

        return fill;
    }

    #region Helpers

    private static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        Transform existing = parent.Find(name);
        RectTransform rect;

        if (existing != null)
        {
            rect = (RectTransform)existing;
            Undo.RecordObject(rect, "Build HUD");
        }
        else
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Build HUD");
            go.layer = parent.gameObject.layer;
            rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
        }

        Place(rect, anchor, pivot, position, size);
        return rect;
    }

    private static TextMeshProUGUI Text(Transform parent, string name, TMP_FontAsset font, float size, TextAlignmentOptions alignment,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 boxSize)
    {
        RectTransform rect = Rect(parent, name, anchor, pivot, position, boxSize);

        TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
            text.text = name;
        }

        if (font != null) text.font = font;
        text.color = Color.white;
        text.raycastTarget = false;
        Style(text, size, alignment);

        return text;
    }

    private static void Style(TextMeshProUGUI text, float size, TextAlignmentOptions alignment)
    {
        Undo.RecordObject(text, "Build HUD");
        text.fontSize = size;
        text.alignment = alignment;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        if (rect == null) return;

        Undo.RecordObject(rect, "Build HUD");
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(go);
    }

    #endregion
}
