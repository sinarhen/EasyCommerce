﻿using ECommerce.Config;
using ECommerce.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public static class InitDb
{ 
    public static async Task InitializeAsync(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ProductDbContext>();
        var userManager = services.GetRequiredService<UserManager<Customer>>();
        var roleManager = services.GetRequiredService<RoleManager<CustomerRole>>();
        
        await SeedDataAsync(
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

    private static List<ProductStock> GetProductStocksForSeeding(Product product, IEnumerable<Color> colors, int price, IReadOnlyCollection<Size> sizes)
    {
        // Creating 10 product stocks for each size and color
        var list = new List<ProductStock>();
        foreach (var color in colors)
        {
            foreach (var size in sizes)
            {
                list.Add(new ProductStock
                    {
                        Product = product,
                        Color = color,
                        Size = size,
                        Price = Random.Shared.Next(price -5, price + 5) - (decimal)0.01,
                        Stock = 10,
                    }
                );
            }
        }
        return list;
    }
    
    private static ProductImage CreateProductImageEntity(Product product, Color color, List<string> imagesUrl)
    {
        return new ProductImage
        {
            Product = product,
            Color = color,
            ImageUrls = imagesUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
    private static Occasion CreateOccasionEntity(string name, string description)
    {
        return new Occasion
        {
            Name = name,
            Description = description,
            Id = Guid.NewGuid()
        };
    }
    private static Color CreateColorEntity(string name, string hexCode)
    {
        return new Color
        {
            Name = name,
            HexCode = hexCode,
            Id = Guid.NewGuid()
        };
    }
    private static Category CreateCategoryEntity(string name, Category parentCategory = null)
    {
        return new Category
        {
            Name = name,
            Id = Guid.NewGuid(),
            ParentCategory = parentCategory
        };
    }
    
    private static Material CreateMaterialEntity(string name)
    {
        return new Material
        {
            Name = name,
            Id = Guid.NewGuid()
        };
    }
    private static ProductMaterial CreateProductMaterialEntity(Product product, Material material, double percentage)
    {
        return new ProductMaterial
        {
            Product = product,
            Material = material,
            Percentage = percentage,
        };
    }

    private static Product CreateProductEntity(string name, string description,
        Material mainMaterial, Gender gender, Occasion occasion, Season season, int collectionYear)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Season = season,
            MainMaterial = mainMaterial,
            CollectionYear = collectionYear,
            Gender = gender,
            Occasion = occasion
        };
    }
    private static void AddImagesToProduct(Product product, Color color, List<string> imagesUrl)
    {
        product.Images.Add(
            CreateProductImageEntity(product, color, imagesUrl)
        );
    }
    private static void AddMaterialsToProduct(Product product, Material material, double percentage)
    {
        product.Materials.Add(CreateProductMaterialEntity(product, material, percentage));
    }
    private static void AddCategoriesToProduct(Product product, IEnumerable<Category> categories)
    {
        product.Categories = CreateProductCategoryEntitiesFromList(categories, product);
    }
    private static void AddStocksToProduct(Product product, IEnumerable<Color> colors, int price, IReadOnlyCollection<Size> sizes)
    {
        product.Stocks = GetProductStocksForSeeding(product, colors, price, sizes);
    }

    private static ICollection<CategorySize> CreateCategorySizeEntitiesFromList(IReadOnlyCollection<Size> sizes, IEnumerable<Category> categories)
    {
        
        var list = new List<CategorySize>();
        foreach (var category in categories)
        {
            list.AddRange(sizes.Select(size => new CategorySize { Category = category, Size = size, })); }

        return list;
    }
    private static void AddSizesToCategory(Category category, IReadOnlyCollection<Size> sizes)
    {
        category.Sizes = CreateCategorySizeEntitiesFromList(sizes, new[] { category });
    }
    
    private static ICollection<ProductCategory> CreateProductCategoryEntitiesFromList(IEnumerable<Category> categories, Product product)
    {
        return categories.Select((category, index) => new ProductCategory { Category = category, Product = product, Order = index + 1}).ToList();
    }
    
    private static async Task SeedInitialProducts(ProductDbContext context)
    {
        // Colors
        var white = CreateColorEntity("White", "#FFFFFF");
        var black = CreateColorEntity("Black", "#000000");
        var red = CreateColorEntity("Red", "#FF0000");
        var green = CreateColorEntity("Green", "#008000");
        var blue = CreateColorEntity("Blue", "#0000FF");
        var yellow = CreateColorEntity("Yellow", "#FFFF00");
        var cyan = CreateColorEntity("Cyan", "#00FFFF");
        var silver = CreateColorEntity("Silver", "#C0C0C0");
        var purple = CreateColorEntity("Purple", "#800080");
        var orange = CreateColorEntity("Orange", "#FFA500");
        var violet = CreateColorEntity("Violet", "#EE82EE");
        var brown = CreateColorEntity("Brown", "#A52A2A");
        var khaki = CreateColorEntity("Khaki", "#F0E68C");
        
        
        await context.Colors.AddRangeAsync(new []{white, black, red, green, blue, yellow, cyan, silver, purple, orange, violet, brown, khaki});
        
        // Occasions
        var casualOccasion = CreateOccasionEntity("Casual", "Outfits for casual occasions");
        
        var formalOccasion = CreateOccasionEntity("Formal", "Outfits for formal occasions");
        
        var sportyOccasion = CreateOccasionEntity("Sporty", "Outfits for sporty occasions");
        
        var businessOccasion = CreateOccasionEntity("Business", "Outfits for business occasions");
        
        var streetOccasion = CreateOccasionEntity("Street", "Outfits for street occasions");
        
        await context.Occasions.AddRangeAsync(new []{casualOccasion, formalOccasion, sportyOccasion, businessOccasion, streetOccasion});
        
        // Categories
        var shirtsCategory = CreateCategoryEntity("Shirts");
        var shoesCategory = CreateCategoryEntity("Shoes");
        var pantsCategory = CreateCategoryEntity("Pants");
        var accessoriesCategory = CreateCategoryEntity("Accessories");
        
        await context.Categories.AddRangeAsync(new []{shirtsCategory, shoesCategory, pantsCategory, accessoriesCategory});
        
        // Create subcategories
        var poloShirtSubcategory = CreateCategoryEntity("Polo Shirt", shirtsCategory);
        var runningShoesSubcategory = CreateCategoryEntity("Running Shoes", shoesCategory);
        var casualShoesSubcategory = CreateCategoryEntity("Casual Shoes", shoesCategory);
        var chinosSubcategory = CreateCategoryEntity("Chinos", pantsCategory);
        var beltsSubcategory = CreateCategoryEntity("Belts", accessoriesCategory);
        var tShirtsSubcategory = CreateCategoryEntity("T-Shirts", shirtsCategory);
        var blouseSubcategory = CreateCategoryEntity("Blouse", shirtsCategory);

        await context.Categories.AddRangeAsync(new []{poloShirtSubcategory, runningShoesSubcategory, casualShoesSubcategory, chinosSubcategory, beltsSubcategory, tShirtsSubcategory, blouseSubcategory});
        
        // Materials
        var cottonMaterial = CreateMaterialEntity("Cotton");
        var leatherMaterial = CreateMaterialEntity("Leather");
        var polyesterMaterial = CreateMaterialEntity("Polyester");
        var silkMaterial = CreateMaterialEntity("Silk");
        var woolMaterial = CreateMaterialEntity("Wool");
        var rubberMaterial = CreateMaterialEntity("Rubber");
        var suedeMaterial = CreateMaterialEntity("Suede");
        
        await context.Materials.AddRangeAsync(new []{cottonMaterial, leatherMaterial, polyesterMaterial, silkMaterial, woolMaterial, rubberMaterial, suedeMaterial});
        
        var products = new List<Product>();
        // Specific Sizes for each category


        var shirtsSizes = CreateSizeEntitiesFromList(new Dictionary<string, int>{
            {"XS", -6},
            {"S", -5},
            {"M", -4},
            {"L", -3},
            {"XL", -2},
            {"XXL", -1},
        });
        var shirtsCategorySizes = CreateCategorySizeEntitiesFromList(shirtsSizes, new [] {shirtsCategory, tShirtsSubcategory, poloShirtSubcategory, blouseSubcategory});

        
        var shoesSizes = CreateSizeEntitiesFromList(new Dictionary<string, int>{
            {"36", 36},
            {"37", 37},
            {"38", 38},
            {"39", 39},
            {"40", 40},
            {"41", 41},
            {"42", 42},
            {"43", 43},
            {"44", 44},
            {"45", 45},
            {"46", 46},
            {"47", 47},
        });
        var shoesCategorySizes = CreateCategorySizeEntitiesFromList(shoesSizes, 
            new [] {shoesCategory, runningShoesSubcategory, casualShoesSubcategory,});

        
        var pantsSizes = CreateSizeEntitiesFromList(new Dictionary<string, int>{
            {"XS", 28},
            {"S", 30},
            {"M", 32},
            {"L", 34},
            {"XL", 36},
            {"XXL", 38},
        });
        var pantsCategorySizes = CreateCategorySizeEntitiesFromList(pantsSizes, 
            new [] {pantsCategory, chinosSubcategory});

        var accessoriesSizes = CreateSizeEntitiesFromList(new Dictionary<string, int>{
            {"XS", 28},
            {"S", 30},
            {"M", 32},
            {"L", 34},
            {"XL", 36},
            {"XXL", 38},
        });
        
        var beltsSizes = CreateSizeEntitiesFromList(new Dictionary<string, int>{
            {"XS", 28},
            {"S", 30},
            {"M", 32},
            {"L", 34},
            {"XL", 36},
            {"XXL", 38},
        });
        var accessoriesCategorySizes = CreateCategorySizeEntitiesFromList(accessoriesSizes, new []{accessoriesCategory, beltsSubcategory});

        // Add all sizes to the db
        await context.CategorySizes.AddRangeAsync(shirtsCategorySizes);
        await context.CategorySizes.AddRangeAsync(shoesCategorySizes);
        await context.CategorySizes.AddRangeAsync(pantsCategorySizes);
        await context.CategorySizes.AddRangeAsync(accessoriesCategorySizes);
        
        
        // Products with category "Shirts"
        var shirtsProductOne = CreateProductEntity( "Men's Casual Shirt", "A comfortable and stylish shirt for everyday wear.", cottonMaterial, Gender.Unisex, casualOccasion, Season.Summer, 2021);
        AddImagesToProduct(shirtsProductOne, white, new List<string> { "https://i.pinimg.com/564x/1b/7c/b6/1b7cb6fe341e990867f7f29d8fc44773.jpg" });
        AddMaterialsToProduct(shirtsProductOne, cottonMaterial, 0.9);
        AddMaterialsToProduct(shirtsProductOne, polyesterMaterial, 0.1);
        AddStocksToProduct(shirtsProductOne, new[] { white, black }, 30, shirtsSizes);
        AddCategoriesToProduct(shirtsProductOne, new[] { shirtsCategory, tShirtsSubcategory });
        
        products.Add(shirtsProductOne);
        var shirtsProductTwo = CreateProductEntity("Women's Blouse", "Casual blouse for casual occasions.", silkMaterial, Gender.Female, formalOccasion, Season.Summer, 2021);
        AddImagesToProduct(shirtsProductTwo, white, new List<string> { "https://i.pinimg.com/564x/f5/58/9d/f5589d631ab3686d469ec93ac23ebc9f.jpg" });
        AddImagesToProduct(shirtsProductTwo, black, new List<string> { "https://i.pinimg.com/564x/82/2a/ac/822aac770bc03449bfb85a7d63e276d4.jpg" });
        AddMaterialsToProduct(shirtsProductTwo, silkMaterial, 0.9);
        AddMaterialsToProduct(shirtsProductTwo, woolMaterial, 0.1);
        AddStocksToProduct(shirtsProductTwo, new[] { black, white }, 40, shirtsSizes);
        AddCategoriesToProduct(shirtsProductTwo, new[] { shirtsCategory, blouseSubcategory });
        products.Add(shirtsProductTwo);

        var shirtsProductThree = CreateProductEntity("Men's Striped Polo Shirt", "Casual polo shirt with stripes for a sporty look.", cottonMaterial, Gender.Male, casualOccasion, Season.Summer, 2021);
        AddImagesToProduct(shirtsProductThree, black, new List<string> { "https://i.pinimg.com/564x/70/da/dd/70dadd5f402821dcae83e7be32e29ce7.jpg" });
        AddMaterialsToProduct(shirtsProductThree, polyesterMaterial, 0.5);
        AddMaterialsToProduct(shirtsProductThree, cottonMaterial, 0.5);
        AddStocksToProduct(shirtsProductThree, new[] { black }, 25, shirtsSizes);
        AddCategoriesToProduct(shirtsProductThree, new[] { shirtsCategory, poloShirtSubcategory });
        products.Add(shirtsProductThree);


        // Products with category "Shoes"    
        var shoesProducts = new List<Product>();
        var pantsProducts = new List<Product>();
        var accessoriesProducts = new List<Product>();

        // Shoes product one
        var shoesProductOne = CreateProductEntity("Men's Running Shoes", "Comfortable running shoes for active individuals.", polyesterMaterial, Gender.Male, sportyOccasion, Season.DemiSeason, 2022);
        AddImagesToProduct(shoesProductOne, black, new List<string> { "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/61ZA59Q2OIL._AC_SY395_.jpg", /*...other urls...*/ });
        AddImagesToProduct(shoesProductOne, white, new List<string> { "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/51+49v7ksXL._AC_SY395_.jpg", /*...other urls...*/ });
        AddMaterialsToProduct(shoesProductOne, rubberMaterial, 0.4);
        AddMaterialsToProduct(shoesProductOne, polyesterMaterial, 0.6);
        AddStocksToProduct(shoesProductOne, new[] { black, white }, 80, shoesSizes);
        shoesProducts.Add(shoesProductOne);

        var shoesProductTwo = CreateProductEntity( "Unisex Casual Sneakers", "Comfortable and stylish casual sneakers for both men and women.", cottonMaterial, Gender.Unisex, streetOccasion, Season.DemiSeason, 2022);
        AddImagesToProduct(shoesProductTwo, violet, new List<string> { "https://www.nike.org.ua/files/resized/products/85_1.700x800.png" });
        AddImagesToProduct(shoesProductTwo, white, new List<string> { "https://www.nike.org.ua/files/resized/products/84_1.700x800.png" });
        AddImagesToProduct(shoesProductTwo, black, new List<string> { "https://www.nike.org.ua/files/resized/products/80_1.700x800.png" });
        AddMaterialsToProduct(shoesProductTwo, cottonMaterial, 0.8);
        AddMaterialsToProduct(shoesProductTwo, rubberMaterial, 0.2);
        AddCategoriesToProduct(shoesProductTwo, new[] { shoesCategory, casualShoesSubcategory });
        shoesProducts.Add(shoesProductTwo);

        var shoesProductThree = CreateProductEntity("Women's Casual Sneakers", "Stylish and comfortable casual sneakers for women.", cottonMaterial, Gender.Female, streetOccasion, Season.DemiSeason, 2022);
        AddImagesToProduct(shoesProductThree, black, new List<string> { "https://images.puma.com/image/upload/f_auto,q_auto,b_rgb:fafafa/global/397549/01/sv01/fnd/UKR/w/1000/h/1000/fmt/png" });
        AddMaterialsToProduct(shoesProductThree, suedeMaterial, 0.8);
        AddMaterialsToProduct(shoesProductThree, rubberMaterial, 0.2);
        AddStocksToProduct(shoesProductThree, new[] { black }, 140, shoesSizes);
        AddCategoriesToProduct(shoesProductThree, new[] { shoesCategory, casualShoesSubcategory });
        shoesProducts.Add(shoesProductThree);

        var pantsProductOne = CreateProductEntity( "Men's Casual Pants", "Comfortable and stylish pants for everyday wear.", cottonMaterial, Gender.Male, casualOccasion, Season.DemiSeason, 2022);
        pantsProductOne.SizeChartImageUrl = "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/61q0QLQ1EFL._AC_SX342_.jpg";
        AddImagesToProduct(pantsProductOne, black, new List<string> 
        {
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/81uCZWI6cUL._AC_SY445_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/71UQKwNyieL._AC_SY445_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/71obafsgPfL._AC_SY445_.jpg",
        });
        AddImagesToProduct(pantsProductOne, khaki, new List<string> 
        {
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/81uoaWwCjuL._AC_SY445_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/81j44iOBYCL._AC_SY445_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/712wBPy9LkL._AC_SY445_.jpg",
        });
        AddMaterialsToProduct(pantsProductOne, woolMaterial, 1);
        AddStocksToProduct(pantsProductOne, new[] { black, khaki }, 50, pantsSizes);
        AddCategoriesToProduct(pantsProductOne, new[] { pantsCategory, chinosSubcategory });
        pantsProducts.Add(pantsProductOne);

        var accessoriesProductOne = CreateProductEntity( "Leather Belt", "Stylish leather belt for men and women.", leatherMaterial, Gender.Unisex, businessOccasion, Season.All, 2022);
        AddImagesToProduct(accessoriesProductOne, brown, new List<string> 
        {
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/61u1DkV6u8L._AC_SX679_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/61nfdjkLWwL._AC_SX385_.jpg",
            "https://m.media-amazon.com/images/W/MEDIAX_792452-T1/images/I/61uBafwoEGL._AC_SX342_.jpg",
        });
        AddMaterialsToProduct(accessoriesProductOne, leatherMaterial, 1);
        AddStocksToProduct(accessoriesProductOne, new[] { brown }, 100, beltsSizes);
        AddCategoriesToProduct(accessoriesProductOne, new[] { accessoriesCategory, beltsSubcategory });
        accessoriesProducts.Add(accessoriesProductOne);

        
        var allProducts = new List<Product>();
        allProducts.AddRange(shoesProducts);
        allProducts.AddRange(pantsProducts);
        allProducts.AddRange(accessoriesProducts);
        allProducts.AddRange(products);
        
        await context.Products.AddRangeAsync(allProducts);

        await context.SaveChangesAsync();
    }

    private static IReadOnlyCollection<Size> CreateSizeEntitiesFromList(IReadOnlyDictionary<string, int> sizes)
    {
        return sizes.Select(size => new Size
        {
            Name = size.Key,
            Value = size.Value,
            Id = Guid.NewGuid()
        }).ToList();
    }

    private static async Task EnsureInitialProductsAreCreated(ProductDbContext context)
    {

        if (!context.Products.Any())
        {
            // Add 10 products with new Color objects
            await SeedInitialProducts(context);        
        }
    }

    private static async Task SeedDataAsync(ProductDbContext context, UserManager<Customer> userManager, RoleManager<CustomerRole> roleManager)
    {
        await context.Database.MigrateAsync();
        
        await EnsureRolesAreCreated(roleManager);
        await EnsureAdminIsCreated(userManager);
        await EnsureInitialProductsAreCreated(context: context);
        
        await context.SaveChangesAsync();
    }
    
}