using System;
using System.Drawing;
using Core.Expansions;

namespace Core.Models
{
    public class HashingMinifiedCluster : Cluster, IEquatable<HashingMinifiedCluster>
    {
        public ColorHSV AvColor { get; }

        public string Hash { get; }

        public HashingMinifiedCluster(string Hash, ColorHSV AvColor) 
        {
            if (Hash == null)
                throw new Exception("Hash invalid");

            this.Hash = Hash;
            this.AvColor = AvColor;
        }

        public bool Equals(HashingMinifiedCluster Obj)
        {
            return Obj != null && Obj.Hash == Hash;
        }

        public override bool Equals(Cluster Obj)
        {
            return Obj is HashingMinifiedCluster mh && Equals(mh);
        }

        public override Color GetAvColor()
        {
            return AvColor.ToARGBColor();
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }
}
