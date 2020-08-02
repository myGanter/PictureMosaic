using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Core.Expansions;

namespace Core.Models
{
    public class HSVCluster : Cluster, IEquatable<HSVCluster>
    {
        public ColorHSV[] Colors { get; }

        public HSVCluster(ColorHSV[] Colors) 
        {
            if (Colors == null || Colors.Length == 0)
                throw new Exception("Colors empty");

            this.Colors = Colors;
        }
        public override Color GetAvColor()
        {
            int h = 0, s = 0, v = 0;

            for (var i = 0; i < Colors.Length; ++i) 
            {
                h += Colors[i].H;
                s += Colors[i].S;
                v += Colors[i].V;
            }

            var resHsv = new ColorHSV((short)(h / Colors.Length), (short)(s / Colors.Length), (short)(v / Colors.Length));

            return resHsv.ToARGBColor();
        }

        public bool Equals([AllowNull] HSVCluster Obj)
        {
            if (Obj == null || Obj.Colors.Length != Colors.Length)
                return false;

            var objCols = Obj.Colors;
            for (var i = 0; i < Colors.Length; ++i) 
            {
                if (!Colors[i].Equals(objCols[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals([AllowNull] Cluster Obj)
        {
            return Obj is HSVCluster hsvO && Equals(hsvO);
        }

        public override int GetHashCode()
        {
            long sum = 0;
            for (var i = 0; i < Colors.Length; ++i) 
            {
                sum += Colors[i].GetHashCode();
            }

            return (int)(sum / Colors.Length);
        }
    }
}
