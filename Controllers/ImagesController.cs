using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Db;
using Back.Models;
using Back.Utils;
using Newtonsoft.Json;
using front.Utils.Logger;
using Back.Utils.ImageFilteringCoC;
using Back.Attributes;

namespace Back.Controllers
{
    [Route("images")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly MySqlContext _db;
        private readonly ImageSaver _imageSaver;
        private readonly Pagination<Image> _pagination;
        private readonly BaseImageFilteringHandler _filter;
        private readonly ImageDownloadProxy _imageDownloadProxy;

        public ImagesController(MySqlContext context, ImageSaver imageSaver, ImageDownloadProxy idp)
        {
            _db = context;
            _imageSaver = imageSaver;
            _pagination = new Pagination<Image>(_db);
            _filter = new UserImageFilteringHandler(
                        new HashtagImageFilteringHandler(
                            new UploadImageFilteringHandler()), _db);
            _imageDownloadProxy = idp;
        }

        [HttpGet]
        public async Task<ActionResult<List<ImageRequest>>> GetImages(int page = 1)
        {
            Logger.Instance.Information($"Someone fetched page {page} of images");
            return _pagination.GetPage(page).Select(x => ImageToImageRequestConverter.Convert(x)).ToList();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<ImageRequest>>> GetFilteredImages(string? user, string? hashtag, string? date)
        {
            _filter.SetRequests(new FilterRequest( user, hashtag, date ));
            Logger.Instance.Information($"Someone filtered images by: user => {user}, hashtag => {hashtag}, date => {date}");
            return _filter.Handle(_db.Images.ToList()).Select(x => ImageToImageRequestConverter.Convert(x)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImageRequest>> GetImage(int id)
        {
            var image = await _db.Images.FindAsync(id);

            if (!ImageExists(id))
            {
                Logger.Instance.Warning($"Someone asked for image {id} that doesn't exist");
                return NotFound();
            }
            Logger.Instance.Information($"Someone asked for image {id}");
            return ImageToImageRequestConverter.Convert(image);
        }

        [UserOrAdmin]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchImage(int userId, int id, ImageDescription description)
        {
            if (!ImageExists(id))
            {
                return BadRequest();
            }
            string fileName = _db.Images.Find(id).Url;
            var oldHashtags = _db.ImageHashtags.Where(x => x.ImageId == id).ToList();
            foreach (var hashtag in oldHashtags)
            {
                _db.Entry(hashtag).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
            }
            _db.Images.Find(id).Description = description.Description;
            _imageSaver.AddHashtagsToDb(fileName, description.Hashtags);

            await _db.SaveChangesAsync();
            Logger.Instance.Information($"User {userId} modified image {id} description to: {description.Description}");

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Image>> PostImage()
        {
            if (!Request.HasFormContentType)
            {
                Logger.Instance.Error("User didnt send multipart/form-data request");
                return BadRequest("Expected multipart/form-data request");
            }

            try
            {
                var form = await Request.ReadFormAsync();
                var imageFile = form.Files.FirstOrDefault(f => f.Name == "image");
                var jsonData = form["data"];

                if (imageFile == null || string.IsNullOrEmpty(jsonData))
                {
                    Logger.Instance.Error("Missing image or JSON data for picture upload");
                    return BadRequest("Missing image or JSON data");
                }

                using (var memoryStream = new MemoryStream())
                {
                    imageFile.CopyTo(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    ImageRequest image = JsonConvert.DeserializeObject<ImageRequest>(jsonData);
                    image.ImageData.Content = bytes;
                    await _imageSaver.SaveImageAsync(image);
                    Logger.Instance.Information($"Image {image.Url} saved");
                    return Ok();
                }
            }
            catch {
                Logger.Instance.Error("Failed to save an image");
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            _db.Images.Remove(await _db.Images.FindAsync(id));
            _db.SaveChangesAsync();
            Logger.Instance.Information($"Image {id} deleted");
            return NoContent();
        }

        [HttpPost("download")]
        public async Task<ActionResult<int>> UploadImageDownload()
        {
            if (!Request.HasFormContentType)
            {
                Logger.Instance.Error($"User didn't send multipart/form-data request when tryng to download image");
                return BadRequest("Expected multipart/form-data request");
            }

            try
            {
                var form = await Request.ReadFormAsync();
                var imageFile = form.Files.FirstOrDefault(f => f.Name == "image");
                var jsonData = form["data"];

                if (imageFile == null || string.IsNullOrEmpty(jsonData))
                {
                    Logger.Instance.Error("Missing image or JSON data for image download");
                    return BadRequest("Missing image or JSON data");
                }

                using (var memoryStream = new MemoryStream())
                {
                    imageFile.CopyTo(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    ImageRequest image = JsonConvert.DeserializeObject<ImageRequest>(jsonData);
                    image.ImageData.Content = bytes;
                    
                    int id =_imageDownloadProxy.AddImage(image.ImageData);
                    Logger.Instance.Information($"Image {id} ready for download");
                    return Ok(id);
                }
            }
            catch
            {
                Logger.Instance.Error("Failed to save an image");
                return BadRequest();
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadPhoto(int id)
        {
            ImageData image = _imageDownloadProxy.GetImage(id);
            Logger.Instance.Information($"Image {id} downloaded");
            return File(image.Content, image.Format, $"image{GetFileExtension(image.Format)}");
        }

        private string GetMimeType(string filePath) =>
            Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => throw new Exception("Unexpected file format")
            };

        private string GetFileExtension(string mimeType) =>
            mimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => throw new Exception("kurac")
            };

        private bool ImageExists(int id)
        {
            return _db.Images.Any(x => x.Id == id);
        }
    }
}
