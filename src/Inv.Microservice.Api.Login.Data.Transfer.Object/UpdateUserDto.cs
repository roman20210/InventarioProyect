﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inv.Microservice.Api.Login.Data.Transfer.Object
{
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
        public string? AdditionalPassword { get; set; }

    }
}
