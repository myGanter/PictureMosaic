using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Core.Models
{
    public abstract class Cluster : IEquatable<Cluster>
    {
        public abstract bool Equals(Cluster Obj);
    }
}
