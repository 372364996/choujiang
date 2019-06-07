using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Domains
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProductHeadImg { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime OpenTime { get; set; }
        public int BusinessId { get; set; }
        public virtual Business Business { get; set; }
        public virtual List<Order> Orders { get; set; }
        public virtual List<ProductImages> ProductImages { get; set; }
    }
    public class ProductImages {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductImageUrl { get; set; }
        public DateTime CreateTime { get; set; }
        public virtual Products Product { get; set; }
    }
}
