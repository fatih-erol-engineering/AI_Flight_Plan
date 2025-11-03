using UnityEngine;

public class NeonWave : MonoBehaviour
{

    [SerializeField] private Material neonWaveMaterial;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Color color;
    private float rate;
    private float density;
    private MaterialPropertyBlock mpb;
    static readonly int ColorID = Shader.PropertyToID("_neonWaveColor");
    static readonly int RateID = Shader.PropertyToID("_neonWaveRate");
    static readonly int DensityID = Shader.PropertyToID("_neonWaveDensity");

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        color = neonWaveMaterial.GetColor(ColorID);
        rate = neonWaveMaterial.GetFloat(RateID);
        density = neonWaveMaterial.GetFloat(DensityID);
    }

    public void SetColor(Color _color, bool isImmediate = false)
    {
        if (color != _color || isImmediate)
        {
            color = _color;
            mpb.SetColor(ColorID, _color);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
    public void SetRate(float _rate, bool isImmediate = false)
    {
        if (rate != _rate || isImmediate)
        {
            rate = _rate;
            mpb.SetFloat(RateID, _rate);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }

    public void SetDensity(float _density, bool isImmediate = false)
    {
        if (density != _density || isImmediate)
        {
            density = _density;
            mpb.SetFloat(DensityID, _density);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
 

}