using Microsoft.AspNetCore.Identity;

namespace WebCalculator.App.Models
{
    public class Calculation
    {
        public int Id { get; set; }
        public string Expression { get; set; }
        public string Result { get; set; }
        public System.DateTime CalcDate { get; set; }
        public string UserId { get; set; }
    }
}