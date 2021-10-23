using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebCalculator.App.Models
{
    public enum NumbersType
    {
        [Display(Name = "Действительные числа")]
        RealNumbers = 1,
        [Display(Name = "Комплексные числа")]
        ComplexNumbers
    }
}
