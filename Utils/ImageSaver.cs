using Back.Db;
using Back.Models;

namespace Back.Utils
{
    public class ImageSaver
    {
        private readonly string _storagePath;
        private readonly MySqlContext _db;
        private readonly PlanLimits _planLimits;
        private readonly ImageNameGenerator _imageNameGenerator;

        public ImageSaver(string storagePath, MySqlContext db)
        {
            _storagePath = storagePath;
            _db = db;
            _planLimits = new PlanLimits(_db);
            _imageNameGenerator = new ImageNameGenerator();
        }

        public async Task<Image> SaveImageAsync(ImageRequest imageRequest)
        {
            User postee = _db.Users.Find(imageRequest.Author);
            if (postee == null)
                throw new Exception("No user");
            if (imageRequest.ImageData == null)
                throw new Exception("No image");
            if (!_planLimits.CheckIfUnderLimit(postee.Role, _db.UserPlanUsages.Find(postee.Id).Usages))
                throw new Exception("Over the plan limit");

            string fileName = _imageNameGenerator.GenerateRandomName(imageRequest.ImageData.Format);

            while (_db.Images.Any(x => x.Url == fileName))
                fileName = _imageNameGenerator.GenerateRandomName(imageRequest.ImageData.Format);

            string filePath = Path.Combine(_storagePath, fileName);

            try
            {
                await File.WriteAllBytesAsync(filePath, imageRequest.ImageData.Content);

                _db.Add(new Image
                {
                    Author = imageRequest.Author,
                    Description = imageRequest.Description.Description,
                    Url = fileName,
                    Uploaded = DateTime.Now,
                });
                _db.SaveChanges();

                //if (imageRequest.Description.Hashtags.Count() != 0)
                //{
                //    imageRequest.Description.Hashtags.ForEach(x => AddHashtag(x));
                //    imageRequest.Description.Hashtags.ForEach(x => AddImageHashtag(x, fileName));
                //}
                AddHashtagsToDb(fileName, imageRequest.Description.Hashtags);

                return _db.Images.Where(x => x.Url == fileName).First();
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    Console.WriteLine("Failed at saving the image");
                }
                Console.WriteLine(ex.ToString());
            }

            throw new Exception("Image failed to save");
        }

        public void AddHashtagsToDb(string fileName, List<Hashtag> hashtags)
        {
            if (hashtags.Count() != 0)
            {
                hashtags.ForEach(x => AddHashtag(x));
                hashtags.ForEach(x => AddImageHashtag(x, fileName));
            }
        }

        private void AddHashtag(Hashtag hashtag)
        {
            if(!_db.Hashtags.Any(x => x.Name == hashtag.Name))
            {
                _db.Add(hashtag);
                _db.SaveChanges();
            };

        }

        private void AddImageHashtag(Hashtag hashtag, string fileName)
        {
            Hashtag _hashtag = _db.Hashtags.Where(x => x.Name == hashtag.Name).First();
            Image _image = _db.Images.Where(x => x.Url == fileName).First();
            _db.Add(new ImageHashtag { HashtagId = _hashtag.Id, ImageId = _image.Id });
            _db.SaveChanges();
        }
    }
}
