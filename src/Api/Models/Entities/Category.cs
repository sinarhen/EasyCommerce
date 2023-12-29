﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Models.Entities;

[PrimaryKey("Id")]
[Index("ParentCategoryId")]
public class Category
{
    [Key]
    public Guid Id { get; set; }
    
    
    public Guid? ParentCategoryId { get; set; }

    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ParentCategoryId")]
    public Category ParentCategory { get; set; }
    
    public ICollection<Category> SubCategories { get; set; } = new List<Category>(); 
    public ICollection<Product> Products { get; set; } = new List<Product>();
}