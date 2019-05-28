using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Domains
{
  public  class Order
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public int  UserId { get; set; }
        public int PorductId { get; set; }
        public bool IsWin { get; set; }
        public  virtual  Products Products { get; set; }
        public virtual User Users { get; set; }
    }
}
