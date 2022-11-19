using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WpfApp
{
    public class ImageInfo
    {
        public int ImageInfoId { get; set; }
        public string filename { get; set; }
        public string path { get; set; }
        public int hash { get; set; }
        public ImageValue value { get; set; }
        public ICollection<Emotion> emotions { get; set; }
        public ImageInfo(string path)
        {
            string[] splittingPath = path.Split("\\");
            this.filename = splittingPath[splittingPath.Length - 1];
            
            this.path = path;

            emotions = new List<Emotion>();
        }

        //public List<(string, float)> dict { get; set; }

        //public ImageInfo(string path, IEnumerable<(string, float)> dict)
        //{
        //    string[] splittingPath = path.Split("\\");
        //    this.filename = splittingPath[splittingPath.Length - 1];

        //    this.path = path;
        //    this.dict = dict.ToList<(string, float)>().OrderBy(x => x.Item1).ToList();
        //}
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

    public class ApplicationContext : DbContext
    {
        public DbSet<ImageInfo> images { get; set; }
        public DbSet<Emotion> emotions { get; set; }
        public DbSet<ImageValue> values { get; set; }

        public ApplicationContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder o) =>
            o.UseSqlite("Data Source=images.db");
    }
}
