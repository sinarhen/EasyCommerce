﻿using ECommerce.Config;
using ECommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public static class InitDb
{ 
    public static void Initialize(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ProductDbContext>();
        var userManager = services.GetRequiredService<UserManager<Customer>>();
        var roleManager = services.GetRequiredService<RoleManager<CustomerRole>>();
        
        SeedData(
            context: context, 
            userManager: userManager, 
            roleManager: roleManager
        );   
    }

    private static async Task EnsureRolesAreCreated(RoleManager<CustomerRole> roleManager)
    {

        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            var role = new CustomerRole
            {
                Name = UserRoles.Admin
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to create admin role");
            }
        }

        if (!await roleManager.RoleExistsAsync(UserRoles.Customer))
        {
            var role = new CustomerRole
            {
                Name = UserRoles.Customer
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to create customer role");
            }
        }
        
        if (!await roleManager.RoleExistsAsync(UserRoles.SuperAdmin))
        {
            var role = new CustomerRole
            {
                Name = UserRoles.SuperAdmin
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to create SuperAdmin role");
            }
        }

    }

    private static async Task EnsureAdminIsCreated(
        UserManager<Customer> userManager
    )
    {
        var adminEmail = Secrets.AdminEmail;
        var adminName = Secrets.AdminUsername;
        var adminPassword = Secrets.AdminPassword;
        
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            
            admin = new Customer
            {
                Email = adminEmail,
                UserName = adminName
            };
            
            var adminResult = await userManager.CreateAsync(admin, adminPassword);
            
            if (!adminResult.Succeeded)
            {
                throw new Exception("Failed to create admin user");
            }
            
        }
        if (await userManager.IsInRoleAsync(admin, UserRoles.SuperAdmin))
        {
            return;
        }
        var roleResult = await userManager.AddToRoleAsync(admin, UserRoles.SuperAdmin);
        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to add admin user to roles");
        }
    }

    private static List<ProductStock> GetProductStocksForSeeding(Product product)
    {
        var hexCodes = new [] { "#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#00FFFF", "#FF00FF", "#C0C0C0", "#800080", "#FFA500" };

        // Creating 10 product stocks for each size and color

        return (from Size size in Enum.GetValues(typeof(Size))
            from hexCode in hexCodes
            select new ProductStock
            {
                Product = product,
                // HexCode = hexCode, // Define a method to generate random hex code
                Stock = Random.Shared.Next(0, 25),
                Size = size
            }).ToList();
    }

    private static IEnumerable<Product> GetInitialProducts()
    {
        var white = new Color { Name = "White", HexCode = "#FFFFFF", Id = Guid.NewGuid() };
        var black = new Color { Name = "Black", HexCode = "#000000", Id = Guid.NewGuid() };
        var red = new Color { Name = "Red", HexCode = "#FF0000", Id = Guid.NewGuid() };
        var green = new Color { Name = "Green", HexCode = "#00FF00", Id = Guid.NewGuid() };
        var blue = new Color { Name = "Blue", HexCode = "#0000FF", Id = Guid.NewGuid() };
        var yellow = new Color { Name = "Yellow", HexCode = "#FFFF00", Id = Guid.NewGuid() };
        var cyan = new Color { Name = "Cyan", HexCode = "#00FFFF", Id = Guid.NewGuid() };
        var magenta = new Color { Name = "Magenta", HexCode = "#FF00FF", Id = Guid.NewGuid() };
        var silver = new Color { Name = "Silver", HexCode = "#C0C0C0", Id = Guid.NewGuid() };
        var purple = new Color { Name = "Purple", HexCode = "#800080", Id = Guid.NewGuid() };
        var orange = new Color { Name = "Orange", HexCode = "#FFA500", Id = Guid.NewGuid() };
        
        
        var shirtsCategory = new Category { Name = "Shirts", Id = Guid.NewGuid() };
        var shoesCategory = new Category { Name = "Shoes", Id = Guid.NewGuid() };
        var pantsCategory = new Category { Name = "Pants", Id = Guid.NewGuid() };
        var accessoriesCategory = new Category { Name = "Accessories", Id = Guid.NewGuid() };

        var productOne = new Product
        {
            Id = Guid.Parse("88febf1c-f5f3-4df8-98bb-09e5fee18195"),
            Category = shirtsCategory,
            Name = "Men's Casual Shirt",
            Description = "A comfortable and stylish shirt for everyday wear.",
            Price = (decimal)29.99,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow, 
        };
        productOne.Images = new List<ProductImage>
        {
            new ProductImage
            {
                Color = black,
                Product = productOne,
                Url = "https://i.pinimg.com/736x/94/0d/72/940d72d716ba0063347add99973bc85c.jpg"
            },
            new ProductImage
            {
                Color = white,
                Product = productOne,
                Url = "https://i.pinimg.com/736x/1b/7c/b6/1b7cb6fe341e990867f7f29d8fc44773.jpg"
            },
        };
        
        
        var shirtsProducts = new List<Product>();
        shirtsProducts.Add(productOne);
        shirtsProducts.Add(new Product
        {
            Id = Guid.Parse("a0e3b6a0-0b0a-4b0a-9b0a-0b0a0b0a0b0a"),
            Category = shirtsCategory,
            Name = "Women's Formal Blouse",
            Description = "Elegant blouse for formal occasions.",
            Price = (decimal)39.99,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
                    
        });
        shirtsProducts.Add(new Product
        {
            Id = Guid.Parse("b0b0b0b0-b0b0-b0b0-b0b0-b0b0b0b0b0b0"),
            Category = shirtsCategory,
            Name = "Men's Striped Polo Shirt",
            Description = "Casual polo shirt with stripes for a sporty look.",
            Price = (decimal)24.99,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
                    
        });
        shirtsProducts.Add(new Product
        {
            Id = Guid.Parse("c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0"),
            Category = shirtsCategory,
            Name = "Women's Denim Shirt",
            Description = "Denim shirt for a trendy and casual appearance.",
            Price = (decimal)44.99,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
                    
        });
        shirtsProducts.Add(new Product
        {
            Id = Guid.Parse("d0d0d0d0-d0d0-d0d0-d0d0-d0d0d0d0d0d0"),
            Category = shirtsCategory,
            Name = "Men's Oxford Dress Shirt",
            Description = "Classic dress shirt for a polished and sophisticated look.",
            Price = (decimal)54.99,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
                    
        });

        var shoesProducts = new List<Product>
        {

        };
            
            
        foreach (var product in shirtsProducts)
        {
            product.Stocks = GetProductStocksForSeeding(product);
        }
        return shirtsProducts.Concat(shoesProducts);
    }
    private static async Task EnsureInitialProductsAreCreated(ProductDbContext context)
    {

        if (!context.Products.Any())
        {
            // Add 10 products with new Color objects
            var products = GetInitialProducts();        
    
            await context.Products.AddRangeAsync(products);
        }

        // Save changes to persist the new products and their associated colors
        await context.SaveChangesAsync();
    }

    private static async void SeedData(ProductDbContext context, UserManager<Customer> userManager, RoleManager<CustomerRole> roleManager)
    {
        await context.Database.MigrateAsync();
        
        await EnsureRolesAreCreated(roleManager);
        await EnsureAdminIsCreated(userManager);
        await EnsureInitialProductsAreCreated(context: context);
        
        await context.SaveChangesAsync();
    }
    
}