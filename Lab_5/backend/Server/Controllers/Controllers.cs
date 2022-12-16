using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private IImagesDb dB;
    public ImagesController(IImagesDb db)
    {
        this.dB = db;
    }
    [HttpPost]
    public async Task<bool> AddPhoto(byte[] img) 
    {
        return await dB.PostImage(img);
    }

    [HttpGet]
    public async Task<IEnumerable<int>> GetImages() 
    {
        var phs = dB.GetAllImagesId();
        return phs;
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<ImageInfo>> GetImageById(int id) 
    {
        var image =  dB.GetImageById(id);
        if (image != null)
            return image;
        return StatusCode(404); 
    }

    [HttpDelete]
    public async Task<bool> DeleteImages() 
    {
        return await dB.DeleteAllImages();
    }
}
