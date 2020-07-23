using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Core.Models
{
    public readonly struct ColorHSV : IEquatable<ColorHSV>
    {
        public readonly short H;

        public readonly short S;

        public readonly short V;

        public ColorHSV(short H, short S, short V) 
        {
            this.H = H;
            this.S = S;
            this.V = V;
        }

        public bool Equals(ColorHSV C)
        {
            return H == C.H && S == C.S && V == C.V;
        }
    }
}
