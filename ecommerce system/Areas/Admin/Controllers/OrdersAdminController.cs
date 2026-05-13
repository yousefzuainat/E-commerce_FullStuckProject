using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ecommerce_system.Data;
using ecommerce_system.Models;
using Microsoft.AspNetCore.Authorization;

namespace ecommerce_system.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
