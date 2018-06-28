using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;


namespace SoundEditor
{
    class TrackEffectPitch: IEffect, ISampleProvider
    {
   
    public ISampleProvider SourceStream = null;
    private WaveFormat WFormat = null;
    private float Pitch = 1f; 
    private int _FFTSize;
    private long _osamp;
    private FFT ShifterLeft = new FFT(); //быстрое преобразование Фурье 
    private FFT ShifterRight = new FFT();

    // Ограничители
    const float LIM_THRESH = 0.95f;
    const float LIM_RANGE = (1f - LIM_THRESH);
    const float M_PI_2 = (float)(Math.PI / 2);

    public TrackEffectPitch(ISampleProvider SourceProvider)
        : this(SourceProvider, 4096, 4L, 1f) { }

    public TrackEffectPitch(ISampleProvider SourceProvider, int FFTSize, long osamp, float InitialPitch)
    {
        SourceStream = SourceProvider;
        WFormat = SourceProvider.WaveFormat;
        _FFTSize = FFTSize;
        _osamp = osamp;
        PitchFactor = InitialPitch;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int SampRead = SourceStream.Read(buffer, offset, count);
            float[] Left = new float[(SampRead >> 1)];
            float[] Right = new float[(SampRead >> 1)];
            int index = 0;
            for (int sample = offset; sample <= SampRead + offset - 1; sample += 2) {
                Left[index] = buffer[sample];
                Right[index] = buffer[sample + 1];
                index += 1;
            }
            ShifterLeft.PitchShift(Pitch, SampRead >> 1, _FFTSize, _osamp, WFormat.SampleRate, Left);
            ShifterRight.PitchShift(Pitch, SampRead >> 1, _FFTSize, _osamp, WFormat.SampleRate, Right);
            index = 0;
            for (int sample = offset; sample <= SampRead + offset - 1; sample += 2) {
                buffer[sample] = ApplyEffect(Left[index]);
                buffer[sample + 1] = ApplyEffect(Right[index]);
                index += 1;
            }
            return SampRead;
    }

    public NAudio.Wave.WaveFormat WaveFormat {
        get { return WFormat; }
    }

    public float PitchFactor {
        get { return Pitch; }
        set {
            Pitch = value;
        }
    }

    public float ApplyEffect(float Sample)
    {
        float res = 0f;
        if ((LIM_THRESH < Sample)) {
            res = (Sample - LIM_THRESH) / LIM_RANGE;
            res = (float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        } else if ((Sample < -LIM_THRESH)) {
            res = -(Sample + LIM_THRESH) / LIM_RANGE;
            res = -(float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
        } else {
            res = Sample;
        }
        return res;
    }
}
}