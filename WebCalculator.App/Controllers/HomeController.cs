using IntegralCalculator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebCalculator.App.Models;

namespace WebCalculator.App.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CalculateModel calculateModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                object result;
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                switch ((NumbersType)calculateModel.NumbersType)
                {
                    case NumbersType.RealNumbers:
                        {
                            var parser = new MathParser();
                            result = parser.Parse(calculateModel.Expression, calculateModel.IsInRadians);
                            break;
                        }
                    case NumbersType.ComplexNumbers:
                        {
                            result = null;
                            break;
                        }
                    default:
                        return RedirectToAction(nameof(Error));
                }
                var calculation = _dbContext.Calculations.Add(new Calculation()
                {
                    Expression = calculateModel.Expression,
                    Result = result.ToString(),
                    UserId = user.Id
                });
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("ShowResult", "Home", new { id = calculation.Entity.Id });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home", e.Message);
            }
            

            
        }

        [HttpGet]
        public async Task<IActionResult> ShowResult(string id)
        {
            var calc = await _dbContext.Calculations.FirstOrDefaultAsync(c => c.Id == int.Parse(id));
            return View(calc);
        }

        [HttpGet]
        public async Task<IActionResult> ShowHistory()
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var calculations = await _dbContext.Calculations.Where(c => c.UserId == user.Id).ToListAsync();
            return View(calculations);
        }

        [AllowAnonymous]
        public IActionResult Help()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            return View(new ErrorViewModel { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = message 
            });
        }
    }
}
