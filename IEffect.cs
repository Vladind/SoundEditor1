using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace SoundEditor
{
    public interface IEffect
    {
        float ApplyEffect(float sample);
    }
}
