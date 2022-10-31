using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class ImageInfo
    {
        public string filename { get; set; }
        public string path { get; set; }
        public List<(string, float)> dict { get; set; }

        public ImageInfo(string path, IEnumerable<(string, float)> dict)
        {
            string[] splittingPath = path.Split("\\");
            this.filename = splittingPath[splittingPath.Length - 1];

            this.path = path;
            this.dict = dict.ToList<(string, float)>().OrderBy(x => x.Item1).ToList();
        }
    }
}
