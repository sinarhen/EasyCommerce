﻿using ECommerce.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data.Repositories.Category;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductDbContext _dbContext;

    public CategoryRepository(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<ECommerce.Models.Entities.Category>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();
    }

    public Task<ECommerce.Models.Entities.Category> GetCategoryAsync(Guid id, Guid? sizeId = null, Guid? colorId = null)
    {
        throw new NotImplementedException();
    }

    public Task<ECommerce.Models.Entities.Category> CreateCategoryAsync(WriteCategoryDto categoryDto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCategoryAsync(Guid id, WriteCategoryDto productDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCategoryAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}