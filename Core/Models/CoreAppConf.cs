using Core.Attributes;

namespace Core.Models
{
    public class CoreAppConf
    {
        [Arg("-Alg")]
        public string Clustering { get; set; }
    }
}
