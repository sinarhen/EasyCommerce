﻿using ECommerce.Models.DTOs;


namespace Ecommerce.Data.Repositories.Category;

public interface ICategoryRepository
{
    Task<List<ECommerce.Models.Entities.Category>> GetCategoriesAsync();
    Task<ECommerce.Models.Entities.Category> GetCategoryAsync(Guid id, Guid? sizeId = null, Guid? colorId = null);
    Task<ECommerce.Models.Entities.Category> CreateCategoryAsync(WriteCategoryDto categoryDto);
    Task UpdateCategoryAsync(Guid id, WriteCategoryDto productDto);
    Task DeleteCategoryAsync(Guid id);
    
}