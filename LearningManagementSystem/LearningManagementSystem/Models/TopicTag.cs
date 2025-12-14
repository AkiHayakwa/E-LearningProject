using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class TopicTag
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string TagId { get; set; }

        [Required]
        [StringLength(50)]
        public string TopicId { get; set; }

        [Required]
        [StringLength(50)]
        public string TagName { get; set; }

        // Navigation properties
        public Topic? Topic { get; set; }
    }
}

