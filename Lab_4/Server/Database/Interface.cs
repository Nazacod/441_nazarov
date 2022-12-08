using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using NNLibrary;

namespace Server
{
    public interface IImagesDb 
    {
        Task<bool> PostImage(byte[] img);
        IEnumerable<int> GetAllImagesId();
        ImageInfo? GetImageById(int id);
        Task<bool> DeleteAllImages();
    }

    public class InMemoryDb : IImagesDb
    {
        private NN nnModel = new NN();
        public async Task<bool> PostImage(byte[] img)
        {
            int hash = Tools.ComputeHash(img);

            using (var db = new ApplicationContext())
            {
                var query = db.images.Where(x => x.hash == hash).Include(item => item.value);
                var item = query.Where(x => Enumerable.SequenceEqual(x.value.data, img))
                            .Include(x => x.emotions)
                            .FirstOrDefault();
                if ((item != null) && (item.hash == hash))
                    return false;
                else
                {
                    var result = await nnModel.InferenceAsync(img);
                    var tmpImage = new ImageInfo();
                    tmpImage.value = new ImageValue() { data = img, image = tmpImage };
                    tmpImage.hash = hash;

                    foreach (var elem in result)
                    {
                        tmpImage.emotions.Add(new Emotion() { value = elem.Item2, name = elem.Item1, image = tmpImage });
                    }

                    db.images.Add(tmpImage);
                    db.SaveChanges();
                    
                    return true;
                }
            }
        }
        public IEnumerable<int> GetAllImagesId()
        {
            using (var db = new ApplicationContext())
            {
                return db.images.Select(x => x.ImageInfoId).ToList();
            }
        }

        public ImageInfo? GetImageById(int id)
        {
            using (var db = new ApplicationContext())
            {
                return db.images.Where(x => x.ImageInfoId == id)
                                .Include(x => x.value)
                                .Include(x => x.emotions).FirstOrDefault();
            }
        }
        public async Task<bool> DeleteAllImages()
        {
            try
            {
                using (var db = new ApplicationContext())
                {   
                    await db.Database.ExecuteSqlRawAsync("DELETE FROM [images]");
                }
                
                return true;       
            }
            catch
            {
                return false;
            }
        }
    }
}