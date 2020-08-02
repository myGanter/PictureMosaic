using System;
using System.Drawing;

namespace Core.Models
{
    public abstract class Cluster : IEquatable<Cluster>
    {
        public abstract bool Equals(Cluster Obj);

        public abstract Color GetAvColor();
    }
}
