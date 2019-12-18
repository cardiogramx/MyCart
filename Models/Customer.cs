using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyCart.Models
{
    public class Customer
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }


    }
}
