using System.Drawing;
using Core.Models;

namespace Core.Services
{
    public abstract class BaseClusterService
    {
        public abstract Cluster CreateCluster(Bitmap Bmp);

        public abstract Cluster CreateCluster(Bitmap Bmp, int OffSetX, int OffSetY, int W, int H);
    }
}
