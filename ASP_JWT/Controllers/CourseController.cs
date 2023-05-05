using ASP_JWT.Data;
using ASP_JWT.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Storage.Blob;
using Azure.Storage.Blobs;
using ASP_JWT.Models.Dto;
using ASP_JWT.Migrations;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;

namespace ASP_JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IMapper _mapper;
        public CourseController(IConfiguration configuration, DataContext context, BlobServiceClient blobServiceClient, IMapper mapper)
        {
            _configuration = configuration;
            _context = context;
            _blobServiceClient = blobServiceClient;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Set<Course>().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Set<Course>().FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse([FromForm]CourseDto course, IFormFile coverImageFile, IFormFile videoCoverFile)
        {
            var courseCreate = new CourseCreate();
            if (Request.Form.Files.Count > 0)
            {
                IFormFile coverImage = Request.Form.Files["coverImageFile"];
                IFormFile video = Request.Form.Files["videoCoverFile"];
               

                if (coverImage != null)
                {
                    courseCreate.ImageCoverPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], coverImage);
                }

                if (video != null)
                {
                    courseCreate.VideoPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], video);
                }
            }
            courseCreate.Name=course.Name;
            var objMap=_mapper.Map<Course>(courseCreate);
            _context.Set<Course>().Add(objMap);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = objMap.Id }, objMap);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse([FromForm] CourseDto course, int id,  IFormFile coverImageFile, IFormFile videoCoverFile)
        {
            

            var existingCourse = await _context.Set<Course>().FindAsync(id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            // Reload the entity from the database
            _context.Entry(existingCourse).Reload();

            // Make changes to the entity
            existingCourse.Name = course.Name;
            if (Request.Form.Files.Count > 0)
            {
                IFormFile coverImage = Request.Form.Files["coverImageFile"];
                IFormFile video = Request.Form.Files["videoCoverFile"];

                if (coverImage != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.ImageCoverPath))
                    {
                        await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], existingCourse.ImageCoverPath);
                    }

                    existingCourse.ImageCoverPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], coverImage);
                }
                else
                {
                    existingCourse.ImageCoverPath = existingCourse.ImageCoverPath;
                }

                if (video != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.VideoPath))
                    {
                        await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], existingCourse.VideoPath);
                    }

                    existingCourse.VideoPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], video);
                }
                else
                {
                    existingCourse.VideoPath = existingCourse.VideoPath;
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle the exception by logging and returning an error response
                return Conflict();
            }

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCourse( [FromForm] CourseUpdate course, IFormFile coverImageFile, IFormFile videoCoverFile)
        {
            var existingCourse = await _context.Set<Course>().FindAsync(course.Id);

            if (existingCourse == null)
            {
                return NotFound();
            }
            var couserCreate = new CourseCreate();
            if (Request.Form.Files.Count > 0)
            {
                IFormFile coverImage = Request.Form.Files["coverImageFile"];
                IFormFile video = Request.Form.Files["videoCoverFile"];
               
                if (coverImage != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.ImageCoverPath))
                    {
                        await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], existingCourse.ImageCoverPath);
                    }

                    existingCourse.ImageCoverPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], coverImage);
                }


                if (video != null)
                {
                    if (!string.IsNullOrEmpty(existingCourse.VideoPath))
                    {
                        await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], existingCourse.VideoPath);
                    }

                    existingCourse.VideoPath = await UploadFileToBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], video);
                }

            }
            existingCourse.Name = course.Name;
/*            var objCourseMap = _mapper.Map<Course>(couserCreate);
            _context.Entry(objCourseMap).State = EntityState.Modified;*/
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!CourseExists(course.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return (_context.Set<Course>()?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Set<Course>().FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            // Delete files from Blob Storage
            if (!string.IsNullOrEmpty(course.ImageCoverPath))
            {
                await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:CoverImageContainerName"], course.ImageCoverPath);
            }

            if (!string.IsNullOrEmpty(course.VideoPath))
            {
                await DeleteFileFromBlobStorageAsync(_configuration["AzureBlobStorage:VideoContainerName"], course.VideoPath);
            }

            // Remove course from database
            _context.Set<Course>().Remove(course);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        private async Task DeleteFileFromBlobStorageAsync(string containerName, string blobName)
        {
            string fileName = Path.GetFileName(blobName);
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            try
            {
                await blobClient.DeleteAsync();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error deleting blob {blobName} from container {containerName}: {ex.Message}");
                throw;
            }
        }


        private async Task<string> UploadFileToBlobStorageAsync(string containerName, IFormFile file)
        {
            // Create a reference to the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Get a unique name for the blob
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Upload the file to the container
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(file.OpenReadStream(), true);

            // Return the URL of the uploaded file
            return blobClient.Uri.AbsoluteUri;
        }
        
    }
}
