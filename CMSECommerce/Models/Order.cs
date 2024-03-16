﻿using System.ComponentModel.DataAnnotations.Schema;

namespace CMSECommerce.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public bool Shipped { get; set; } = false;

        [Column(TypeName = "decimal(8, 2)")]
        public decimal GrandTotal { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
