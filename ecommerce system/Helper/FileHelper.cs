namespace ecommerce_system.Helper
{
    public class FileHelper
    {
        public static async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {

            if (file == null || file.Length == 0)
            {
                return null;
            }

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }


            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadFolder, fileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
