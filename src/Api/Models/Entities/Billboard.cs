﻿using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.Entities;

public class Billboard : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Subtitle { get; set; }
    
    public string ImageUrl { get; set; }
    
    public Guid CollectionId { get; set; }
    
    public Guid BillboardFilterId { get; set; }
    
    public string FilterTitle { get; set; }
    
    public string FilterSubtitle { get; set; }
    
    // Navigation properties
    public Collection Collection { get; set; }
    
    
    public BillboardFilter BillboardFilter { get; set; }
}