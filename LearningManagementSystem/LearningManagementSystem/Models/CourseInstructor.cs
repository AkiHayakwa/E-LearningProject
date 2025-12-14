    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

    namespace LearningManagementSystem.Models
    {
        public class CourseInstructor
        {
            [Required]
            [StringLength(50)]
            public string CourseId { get; set; }

            [Required]
            [StringLength(50)]
            public string UserName { get; set; }

        // Navigation properties
        [ValidateNever]
        public Course Course { get; set; }
        [ValidateNever]
        public User User { get; set; }
        }
    }