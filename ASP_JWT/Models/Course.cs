﻿namespace ASP_JWT.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageCoverPath { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;

    }
}
