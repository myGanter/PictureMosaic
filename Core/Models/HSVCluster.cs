using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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
            var hsvO = Obj as HSVCluster;

            return hsvO == null ? false : Equals(hsvO);
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
