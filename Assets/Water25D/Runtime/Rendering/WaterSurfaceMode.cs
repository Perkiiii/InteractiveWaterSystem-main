namespace Water25D.Rendering
{
    /// <summary>
    /// Selects the presentation mode for a water surface. The numeric values are a
    /// serialization contract: zero must remain the legacy-compatible simulated path.
    /// </summary>
    public enum WaterSurfaceMode
    {
        SimulatedRipples = 0,
        FlatStylized = 1
    }
}
