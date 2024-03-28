// Copyright (c) Unity Technologies.
// This is helper to convert HDR color to normal color. I do not own this, I modified file from Unity Editor, so it can be used translate color to color32 vice versa in runtime.
using UnityEngine;

namespace AmoyFeels.SpriteShaderIntegration
{
    internal enum RgbaChannel { R, G, B, A }
    internal enum HsvChannel { H, S, V }

    // includes color setters for RGB float, RGB byte, and HSV
    // each mode preserves maximum componential integrity until editing using a different mode
    // exposure and HDR interfaces assume that the supplied originalColor and all per-channel modifications are in linear space
    [System.Serializable]
    internal class ColorMutator
    {
        // specifies the max byte value to use when decomposing a float color into bytes with exposure
        // this is the value used by Photoshop
        const byte k_MaxByteForOverexposedColor = 191;

        internal static void DecomposeHdrColor(Color linearColorHdr, out Color32 baseLinearColor, out float exposure)
        {
            baseLinearColor = linearColorHdr;
            var maxColorComponent = linearColorHdr.maxColorComponent;
            // replicate Photoshops's decomposition behaviour
            if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent >= 1 / 255f)
            {
                exposure = 0f;

                baseLinearColor.r = (byte)Mathf.RoundToInt(linearColorHdr.r * 255f);
                baseLinearColor.g = (byte)Mathf.RoundToInt(linearColorHdr.g * 255f);
                baseLinearColor.b = (byte)Mathf.RoundToInt(linearColorHdr.b * 255f);
            }
            else
            {
                // calibrate exposure to the max float color component
                var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
                exposure = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);

                // maintain maximal integrity of byte values to prevent off-by-one errors when scaling up a color one component at a time
                baseLinearColor.r = System.Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.r));
                baseLinearColor.g = System.Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.g));
                baseLinearColor.b = System.Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.b));
            }
        }

        [SerializeField] private Color m_OriginalColor;
        [SerializeField] private Color m_HDRBaseColor;
        [SerializeField] private byte[] m_Color = new byte[4];
        [SerializeField] private float[] m_ColorHdr = new float[4];
        [SerializeField] private float[] m_Hsv = new float[3];
        [SerializeField] private float m_ExposureValue;
        [SerializeField] private float m_BaseExposureValue;

        public Color originalColor => m_OriginalColor;
        public Color32 color => new Color32(m_Color[(int)RgbaChannel.R], m_Color[(int)RgbaChannel.G], m_Color[(int)RgbaChannel.B], m_Color[(int)RgbaChannel.A]);
        public Vector3 colorHsv => new Vector3(m_Hsv[(int)HsvChannel.H], m_Hsv[(int)HsvChannel.S], m_Hsv[(int)HsvChannel.V]);
        public Color exposureAdjustedColor => new Color(m_ColorHdr[(int)RgbaChannel.R], m_ColorHdr[(int)RgbaChannel.G], m_ColorHdr[(int)RgbaChannel.B], m_ColorHdr[(int)RgbaChannel.A]);

        public float exposureValue
        {
            get => m_ExposureValue;
            set
            {
                if (Mathf.Approximately(m_ExposureValue, value))
                    return;
                m_ExposureValue = value;
                var newRgbFloat = m_HDRBaseColor * Mathf.Pow(2f, m_ExposureValue - m_BaseExposureValue);
                m_ColorHdr[(int)RgbaChannel.R] = newRgbFloat.r;
                m_ColorHdr[(int)RgbaChannel.G] = newRgbFloat.g;
                m_ColorHdr[(int)RgbaChannel.B] = newRgbFloat.b;
            }
        }

        public byte GetColorChannel(RgbaChannel channel)
        {
            return m_Color[(int)channel];
        }

        public float GetColorChannelNormalized(RgbaChannel channel)
        {
            return m_Color[(int)channel] / 255f;
        }

        public void SetColorChannel(RgbaChannel channel, byte value)
        {
            var channelIndex = (int)channel;
            if (m_Color[channelIndex] == value)
                return;
            m_Color[channelIndex] = value;
            m_ColorHdr[channelIndex] = value / 255f;
            if (channel != RgbaChannel.A)
                m_ColorHdr[channelIndex] *= Mathf.Pow(2f, m_ExposureValue);
            Color.RGBToHSV(color, out m_Hsv[(int)HsvChannel.H], out m_Hsv[(int)HsvChannel.S], out m_Hsv[(int)HsvChannel.V]);
        }

        public void SetColorChannel(RgbaChannel channel, float normalizedValue)
        {
            SetColorChannel(channel, (byte)Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * 255f));
        }

        public float GetColorChannelHdr(RgbaChannel channel)
        {
            return m_ColorHdr[(int)channel];
        }

        public void SetColorChannelHdr(RgbaChannel channel, float value)
        {
            if (Mathf.Approximately(m_ColorHdr[(int)channel], value))
                return;
            m_ColorHdr[(int)channel] = value;
            m_HDRBaseColor = new Color(m_ColorHdr[0], m_ColorHdr[1], m_ColorHdr[2], m_ColorHdr[3]);
            OnRgbaHdrChannelChanged((int)channel);
            m_BaseExposureValue = m_ExposureValue;
        }

        public float GetColorChannel(HsvChannel channel) => m_Hsv[(int)channel];

        public void SetColorChannel(HsvChannel channel, float value)
        {
            m_Hsv[(int)channel] = Mathf.Clamp01(value);

            var newColor = Color.HSVToRGB(m_Hsv[(int)HsvChannel.H], m_Hsv[(int)HsvChannel.S], m_Hsv[(int)HsvChannel.V]);
            m_Color[(int)RgbaChannel.R] = (byte)Mathf.CeilToInt(newColor.r * 255f);
            m_Color[(int)RgbaChannel.G] = (byte)Mathf.CeilToInt(newColor.g * 255f);
            m_Color[(int)RgbaChannel.B] = (byte)Mathf.CeilToInt(newColor.b * 255f);

            newColor *= Mathf.Pow(2f, m_ExposureValue);
            m_ColorHdr[(int)RgbaChannel.R] = newColor.r;
            m_ColorHdr[(int)RgbaChannel.G] = newColor.g;
            m_ColorHdr[(int)RgbaChannel.B] = newColor.b;
            m_HDRBaseColor = new Color(m_ColorHdr[0], m_ColorHdr[1], m_ColorHdr[2], m_ColorHdr[3]);
        }

        public ColorMutator(Color originalColor)
        {
            m_OriginalColor = originalColor;
            Reset();
        }

        public void Reset()
        {
            if (m_ColorHdr.Length != 4)
                m_ColorHdr = new float[4];
            if (m_Color.Length != 4)
                m_Color = new byte[4];

            m_ColorHdr[(int)RgbaChannel.R] = m_OriginalColor.r;
            m_ColorHdr[(int)RgbaChannel.G] = m_OriginalColor.g;
            m_ColorHdr[(int)RgbaChannel.B] = m_OriginalColor.b;
            m_ColorHdr[(int)RgbaChannel.A] = m_OriginalColor.a;
            m_HDRBaseColor = new Color(m_ColorHdr[0], m_ColorHdr[1], m_ColorHdr[2], m_ColorHdr[3]);

            OnRgbaHdrChannelChanged(-1);
            m_BaseExposureValue = m_ExposureValue;
        }

        void OnRgbaHdrChannelChanged(int channel)
        {
            m_Color[(int)RgbaChannel.A] = (byte)Mathf.RoundToInt(Mathf.Clamp01(m_ColorHdr[(int)RgbaChannel.A]) * 255f);
            if (channel == (int)RgbaChannel.A)
                return;

            DecomposeHdrColor(exposureAdjustedColor, out var baseColor, out m_ExposureValue);

            m_Color[(int)RgbaChannel.R] = baseColor.r;
            m_Color[(int)RgbaChannel.G] = baseColor.g;
            m_Color[(int)RgbaChannel.B] = baseColor.b;
            Color.RGBToHSV(color, out m_Hsv[(int)HsvChannel.H], out m_Hsv[(int)HsvChannel.S], out m_Hsv[(int)HsvChannel.V]);
        }
    }

}