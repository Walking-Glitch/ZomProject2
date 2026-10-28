using System.Collections.Generic;
using UnityEngine;

// short streaks that fly from the muzzle to where a shot landed; the damage is still instant (hitscan),
// this is only the visual. Built on first use, so it needs no scene setup.
public class TracerPool : MonoBehaviour
{
    private const int MaxTracers = 64;

    private static TracerPool instance;
    private static Material defaultMaterial;

    private class Tracer
    {
        public LineRenderer Line;
        public Vector3 From;
        public Vector3 Direction;
        public float Distance;
        public float Speed;
        public float Length;
        public float Travelled;
        public Color Color;
    }

    private readonly List<Tracer> active = new List<Tracer>();
    private readonly Stack<Tracer> free = new Stack<Tracer>();
    private int created;

    public static void Spawn(Vector3 from, Vector3 to, Material material, Color color, float width, float speed, float length)
    {
        if (material == null || speed <= 0f) return;

        if (instance == null)
        {
            instance = new GameObject("TracerPool").AddComponent<TracerPool>();
        }

        instance.SpawnTracer(from, to, material, color, width, speed, length);
    }

    // the laser shader multiplies the line color by the material's tint (red) and a striped texture,
    // so tracers use one shared copy with a neutral tint and no texture; the tracer color then shows as set
    public static Material GetDefaultMaterial(Material laserMaterial)
    {
        if (defaultMaterial == null && laserMaterial != null)
        {
            defaultMaterial = new Material(laserMaterial) { name = "Tracer (runtime)" };

            // the shader doubles its output, so 0.5 is neutral
            if (defaultMaterial.HasProperty("_TintColor")) defaultMaterial.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));

            if (defaultMaterial.HasProperty("_MainTex"))
            {
                defaultMaterial.SetTexture("_MainTex", null);
                defaultMaterial.SetTextureScale("_MainTex", Vector2.one);
            }
        }

        return defaultMaterial;
    }

    private void SpawnTracer(Vector3 from, Vector3 to, Material material, Color color, float width, float speed, float length)
    {
        Tracer tracer;

        if (free.Count > 0)
        {
            tracer = free.Pop();
        }
        else if (created < MaxTracers)
        {
            tracer = CreateTracer();
        }
        else
        {
            // pool is full: reuse the oldest streak, it's almost done anyway
            tracer = active[0];
            active.RemoveAt(0);
        }

        Vector3 path = to - from;

        tracer.From = from;
        tracer.Distance = path.magnitude;
        tracer.Direction = tracer.Distance > 0f ? path / tracer.Distance : Vector3.forward;
        tracer.Speed = speed;
        tracer.Length = length;
        tracer.Travelled = 0f;
        tracer.Color = color;

        tracer.Line.sharedMaterial = material;
        tracer.Line.startWidth = width;
        tracer.Line.endWidth = width;

        UpdateTracer(tracer);
        tracer.Line.enabled = true;

        active.Add(tracer);
    }

    private Tracer CreateTracer()
    {
        created++;

        GameObject go = new GameObject("Tracer");
        go.transform.SetParent(transform, false);

        LineRenderer line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.numCapVertices = 0;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.allowOcclusionWhenDynamic = false;
        line.enabled = false;

        return new Tracer { Line = line };
    }

    private void Update()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Tracer tracer = active[i];
            tracer.Travelled += tracer.Speed * Time.deltaTime;

            // done once the tail has reached the impact point
            if (tracer.Travelled - tracer.Length >= tracer.Distance)
            {
                tracer.Line.enabled = false;
                active.RemoveAt(i);
                free.Push(tracer);
                continue;
            }

            UpdateTracer(tracer);
        }
    }

    private static void UpdateTracer(Tracer tracer)
    {
        float head = Mathf.Min(tracer.Travelled, tracer.Distance);
        float tail = Mathf.Clamp(tracer.Travelled - tracer.Length, 0f, tracer.Distance);

        tracer.Line.SetPosition(0, tracer.From + tracer.Direction * tail);
        tracer.Line.SetPosition(1, tracer.From + tracer.Direction * head);

        // bright head, faded tail
        Color tailColor = tracer.Color;
        tailColor.a = 0f;
        tracer.Line.startColor = tailColor;
        tracer.Line.endColor = tracer.Color;
    }
}
