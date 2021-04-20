﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace travelus.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string StreetAddress { get; set; }
        public string PostalCode { get; set; }
    }
}
