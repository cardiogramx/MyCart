using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCart.Models
{
    public class Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public List<Product> Products { get; set; }

        [Required]
        public DateTime DateTimeAdded { get; set; }

        [Required]
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public bool IsCheckedOut { get; set; } = false;
        public DateTime? DateTimeCheckedOut { get; set; }

        [Required]
        public bool IsAbandoned { get; set; } = true;
        public DateTime? DateTimeAbandoned { get; set; }

        public DateTime? LastVist { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;
        public DateTime? DateTimeDeleted { get; set; }

        [Required]
        public bool IsReminded { get; set; } = false;
        public DateTime? DateTimeReminded { get; set; }
    }
}
