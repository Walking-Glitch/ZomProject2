﻿namespace VLB
{
    public enum FeatureEnabledColorGradient
    {
        Off,        // Do not support having a gradient as color
        HighOnly,   // Support gradient color only for devices with Shader Day = 35 or higher
        HighAndLow  // Support gradient color for all devices
    };

    public enum ColorMode
    {
        Flat,       // Apply a flat/plain/single color
        Gradient    // Apply a gradient
    }

    public enum AttenuationEquation
    {
        Linear = 0,     // Simple linear attenuation.
        Quadratic = 1,  // Quadratic attenuation, which usually gives more realistic results.
        Blend = 2       // Custom blending mix between linear and quadratic attenuation formulas. Use attenuationEquation property to tweak the mix.
    }

    public enum AttenuationEquationHD
    {
        Linear = 0,     // Simple linear attenuation.
        Quadratic = 1,  // Quadratic attenuation, which usually gives more realistic results.
    }

    public enum BlendingMode
    {
        Additive,
        SoftAdditive,
        TraditionalTransparency,
    }

    public enum ShaderAccuracy
    {
        /// <summary> Default accuracy: a lot of computation are done on the vertex shader to maximize performance. </summary>
        Fast,
        /// <summary> Higher accuracy: most of the computation are done on the pixel shader to maximize graphical quality at some performance cost. </summary>
        High,
    }

    public enum NoiseMode
    {
        /// <summary> 3D Noise is disabled </summary>
        Disabled,
        /// <summary> 3D Noise is enabled: noise will look static compared to the world </summary>
        WorldSpace,
        /// <summary> 3D Noise is enabled: noise will look static compared to the beam position </summary>
        LocalSpace,
    }

    public enum MeshType
    {
        Shared, // Use the global shared mesh (recommended setting, since it will save a lot on memory). Will use the geometry properties set on Config.
        Custom, // Use a custom mesh instead. Will use the geometry properties set on the beam.
    }

    public enum RenderPipeline
    {
        /// <summary> Unity's built-in Render Pipeline. </summary>
        BuiltIn,
        /// <summary> Use the Universal Render Pipeline. </summary>
        URP,
        /// <summary> Use the High Definition Render Pipeline. </summary>
        HDRP,
    }

    public enum ShaderMode { SD, HD }

    public enum RenderingMode
    {
        /// <summary> Use the 2 pass shader. Will generate 2 drawcalls per beam (Not compatible with custom Render Pipeline such as HDRP and LWRP).</summary>
        MultiPass,
        /// <summary> Use the 1 pass shader. Will generate 1 drawcall per beam. </summary>
        Default,
        /// <summary> Dynamically batch multiple beams to combine and reduce draw calls. </summary>
        GPUInstancing,
        /// <summary> Use the SRP Batcher to automatically batch multiple beams and reduce draw calls. Only available when using SRP. </summary>
        SRPBatcher,
    }

    public enum RenderQueue
    {
        /// Specify a custom render queue.
        Custom = 0,

        /// This render queue is rendered before any others.
        Background = 1000,

        /// Opaque geometry uses this queue.
        Geometry = 2000,

        /// Alpha tested geometry uses this queue.
        AlphaTest = 2450,

        /// Last render queue that is considered "opaque".
        GeometryLast = 2500,

        /// This render queue is rendered after Geometry and AlphaTest, in back-to-front order.
        Transparent = 3000,

        /// This render queue is meant for overlay effects.
        Overlay = 4000,
    }

    public enum Dimensions
    {
        /// <summary> 3D </summary>
        Dim3D,

        /// <summary> 2D </summary>
        Dim2D
    }

    public enum PlaneAlignment
    {
        /// <summary>Align the plane to the surface normal which blocks the beam. Works better for large occluders such as floors and walls.</summary>
        Surface,
        /// <summary>Keep the plane aligned with the beam direction. Works better with more complex occluders or with corners.</summary>
        Beam
    }

    [System.Flags]
    public enum DynamicOcclusionUpdateRate
    {
        Never = 1 << 0,
        OnEnable = 1 << 1,
        OnBeamMove = 1 << 2,
        EveryXFrames = 1 << 3,
        OnBeamMoveAndEveryXFrames = OnBeamMove | EveryXFrames,
    }

    public enum ParticlesDirection
    {
        /// <summary> Random direction. </summary>
        Random,
        /// <summary> Particles follow the velicity direction in local space (Z is along the beam). </summary>
        LocalSpace,
        /// <summary> Particles follow the velicity direction in world space. </summary>
        WorldSpace
    }

    [System.Flags]
    public enum ShadowUpdateRate
    {
        Never = 1 << 0,
        OnEnable = 1 << 1,
        OnBeamMove = 1 << 2,
        EveryXFrames = 1 << 3,
        OnBeamMoveAndEveryXFrames = OnBeamMove | EveryXFrames,
    }


    public enum CookieChannel
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Alpha = 3,
        RGBA = 4
    }

    [System.Flags]
    public enum DirtyProps
    {
        None = 0,
        Intensity = 1 << 1,
        HDRPExposureWeight = 1 << 2,
        ColorMode = 1 << 3,
        Color = 1 << 4,
        BlendingMode = 1 << 5,
        Cone = 1 << 6,
        SideSoftness = 1 << 7,
        Attenuation = 1 << 8,
        Dimensions = 1 << 9,
        RaymarchingQuality = 1 << 10,
        Jittering = 1 << 11,
        NoiseMode = 1 << 12,
        NoiseIntensity = 1 << 13,
        NoiseVelocityAndScale = 1 << 14,
        CookieProps = 1 << 15,
        ShadowProps = 1 << 16,
        AllWithoutMaterialChange = Intensity | HDRPExposureWeight | Color | Cone | SideSoftness | Jittering | NoiseIntensity | NoiseVelocityAndScale | CookieProps | ShadowProps,
        OnlyMaterialChangeOnly = Attenuation | ColorMode | BlendingMode | Dimensions | RaymarchingQuality | NoiseMode,
        All = AllWithoutMaterialChange | OnlyMaterialChangeOnly,
    }

    [System.Flags]
    public enum BeamProps
    {
        Transform = 1 << 0,
        Color = 1 << 1,
        BlendingMode = 1 << 2,
        Intensity = 1 << 3,
        SideSoftness = 1 << 4,
        SpotShape = 1 << 5,
        FallOffAttenuation = 1 << 6,
        Noise3D = 1 << 7,
        SDConeGeometry = 1 << 8,
        SDSoftIntersectBlendingDist = 1 << 9,
        Props2D = 1 << 10,
    }
}
