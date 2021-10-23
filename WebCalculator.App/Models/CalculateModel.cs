using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebCalculator.App.Models
{
    public class CalculateModel
    {
        [Display(Name = "Выражение")]
        public string Expression { get; set; }

        [Display(Name = "Тип чисел")]
        public int NumbersType { get; set; }

        [Display(Name = "Выражение в радианах")]
        public bool IsInRadians { get; set; }
    }
}
