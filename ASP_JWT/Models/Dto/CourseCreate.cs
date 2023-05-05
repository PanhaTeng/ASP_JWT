namespace ASP_JWT.Models.Dto
{
    public class CourseCreate
    {
        public string Name { get; set; } = string.Empty;
        public string ImageCoverPath { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;
    }
}
