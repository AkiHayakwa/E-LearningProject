using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LearningManagementSystem.Models
{
    public class CourseTag
    {
        [MaxLength(50)]
        public string CourseId { get; set; } = null!;

        [MaxLength(50)]
        public string TagId { get; set; } = null!;

        [ValidateNever]
        public Course Course { get; set; } = null!;
        [ValidateNever]
        public Tag Tag { get; set; } = null!;
    }
}

