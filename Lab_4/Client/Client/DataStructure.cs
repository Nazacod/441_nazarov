using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Client
{
    public class ImageInfo
    {
        public int ImageInfoId { get; set; }
        public string filename { get; set; }
        public int hash { get; set; }
        public ImageValue value { get; set; }
        public ICollection<Emotion> emotions { get; set; }
        public ImageInfo()
        {
            this.filename = "Best name";

            emotions = new List<Emotion>();
        }
    }

    public class ImageValue
    {
        [Key]
        public int ImageInfoId { get; set; }
        public byte[] data { get; set; }
        public ImageInfo image { get; set; }
    }

    public class Emotion
    {
        public int EmotionId { get; set; }
        public float value { get; set; }
        public string name { get; set; }
        public int ImageInfoId { get; set; }
        public ImageInfo image { get; set; }
    }
}
