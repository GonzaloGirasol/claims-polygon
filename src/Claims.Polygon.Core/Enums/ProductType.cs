using System.ComponentModel.DataAnnotations;

namespace Claims.Polygon.Core.Enums
{
    public enum ProductType
    {
        [Display(Name = "Comp")]
        Comp,
        [Display(Name = "Non-Comp")]
        NonComp
    }
}
