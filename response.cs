using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demoMJPEGCamera
{
    internal class response
    {
        public int xmin;
        public int xmax;
        public int ymin;
        public int ymax;
        public int confidence;
        public int classe;
        public string name;

        public response(decimal xmin, decimal xmax, decimal ymin, decimal ymax, decimal confidence, int classe, string name)
        {
            this.xmin = int.Parse((decimal.Round(xmin,0)).ToString());
            this.xmax = int.Parse((decimal.Round(xmax, 0)).ToString());
            this.ymin = int.Parse((decimal.Round(ymin, 0)).ToString());
            this.ymax = int.Parse((decimal.Round(ymax, 0)).ToString());
            this.confidence = int.Parse((decimal.Round(confidence, 0)).ToString());
            this.classe = classe;
            this.name = name;
        }
    }
}
