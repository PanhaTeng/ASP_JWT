using System.ComponentModel.DataAnnotations;

namespace ASP_JWT.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string ?Email { get; set; }

        public string ?PhoneNumber { get; set; }
        public string? photo { get; set;}

        public Customer(string? name, string? email, string? phoneNumber, string? photo)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            this.photo = photo;
        }
    }
}
