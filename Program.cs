using NLog;
using Helper;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try {
    var db = new NWContext();
    char option;

    // Update this array with each menu option added
    char[] optionArray = new char[] {'1', '2', '3', '4', '5', '6', 'q', 'Q'};
    char[] menuOptionsArray = new char[6];
    char[] optionYN = new char[] {'y', 'n', 'Y', 'N'};
    Array.Copy(optionArray, 0, menuOptionsArray, 0, 6);

    do {
        // Main user menu
        Console.Clear();
        Console.WriteLine("Enter an option or 'q' to quit:");
        Console.WriteLine("1) Add a record");
        Console.WriteLine("2) Edit a record");
        Console.WriteLine("3) Display records");
        Console.WriteLine("4) Search products");
        Console.WriteLine("5) Delete a record");
        Console.WriteLine("6) Generate inventory report");
        option = Inputs.GetChar("> ", optionArray);
        logger.Info("User choice: {option}", option);

        switch(option) {

            // Add records
            // Choose whether to add a Product or Category
            case '1':
            Console.Clear();
            Console.WriteLine("Add a product or category? ('q' to quit)");
            Console.WriteLine("1) Product");
            Console.WriteLine("2) Category");
            char addOption = Inputs.GetChar("> ", new char[] {'1', '2', 'q', 'Q'});
            logger.Info($"User choice: {addOption}");

            // Creates a new product
            if (addOption == '1') {
                Console.Clear();
                var productName = Inputs.GetString("Enter a new product name > ");
                ValidationContext context = new ValidationContext(productName, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(productName, context, results, true);
                if (isValid) {
                    if (db.Products.Any(p => p.ProductName == productName)) {
                        isValid = false;
                        results.Add(new ValidationResult("Product name already exists", new string[] { "Name" }));
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    } else {
                        logger.Info("Validation passed");
                        var productCategory = GetCategory(db, logger);
                        if (productCategory == null) {
                            do {
                                logger.Error("Product must belong to a category to continue");
                                productCategory = GetCategory(db, logger);
                            } while (productCategory == null); 
                        }
                        string productQuantity = Inputs.GetString("Enter quantity per unit > ");
                        decimal? productPrice = Inputs.GetDecimal("Enter product price > ");
                        var product = new Product {
                            ProductName = productName,
                            CategoryId = productCategory.CategoryId,
                            QuantityPerUnit = productQuantity,
                            UnitPrice = productPrice};
                        db.AddProduct(product);
                        logger.Info("Product added = {name}", product.ProductName);
                    }

                }

            // Creates a new category
            } else if (addOption == '2') {
                Console.Clear();
                var categoryName = Inputs.GetString("Enter a new category name > ");
                ValidationContext context = new ValidationContext(categoryName, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(categoryName, context, results, true);
                if (isValid) {
                    if (db.Categories.Any(c => c.CategoryName == categoryName)) {
                        isValid = false;
                        results.Add(new ValidationResult("Category name already exists", new string[] { "Name" }));
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    } else {
                        logger.Info("Validation passed");
                        string categoryDescription = Inputs.GetString("Enter a category description: ");
                        var category = new Category {
                            CategoryName = categoryName,
                            Description = categoryDescription};
                        db.AddCategories(category);
                        logger.Info("Category added = {name}", category.CategoryName);
                    }
                }
            }
            break;



            // Edit records
            // Choose whether to edit Products or Categories
            case '2':
            Console.Clear();
            Console.WriteLine("Edit what records? ('q' to quit)");
            Console.WriteLine("1) Products");
            Console.WriteLine("2) Categories");
            char editOption = Inputs.GetChar("> ", new char[] {'1', '2', 'q', 'Q'});
            logger.Info($"User choice: {editOption}");

            // Edits a product
            if (editOption == '1'){
                Console.Clear();
                var product = GetProduct(db, logger);
                if (product != null) {
                    Product editedProduct = InputProduct(db, logger);
                    if (editedProduct != null) {
                        editedProduct.ProductId = product.ProductId;
                        editedProduct.Discontinued = Inputs.GetBool("Discontinued? (True/False) > ");
                        var productCategory = GetCategory(db, logger);
                        editedProduct.CategoryId = productCategory.CategoryId;
                        editedProduct.UnitPrice = Inputs.GetDecimal("Enter unit price > ");
                        editedProduct.QuantityPerUnit = Inputs.GetString("Enter quantity per unit > ");
                        editedProduct.UnitsInStock = Inputs.GetShort("Enter units in stock > ");
                        editedProduct.UnitsOnOrder = Inputs.GetShort("Enter units on order > ");
                        editedProduct.ReorderLevel = Inputs.GetShort("Enter reorder level > ");
                        db.EditProduct(editedProduct);
                        logger.Info($"Product {product.ProductId}: {product.ProductName} updated");
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    }
                }

            // Edits a category
            } else if (editOption == '2') {
                Console.Clear();
                var category = GetCategory(db, logger);
                if (category != null) {
                    Category editedCategory = InputCategory(db, logger);
                    if (editedCategory != null) {
                        editedCategory.CategoryId = category.CategoryId;
                        editedCategory.CategoryName = Inputs.GetString("Enter a name for the category: ");
                        editedCategory.Description = Inputs.GetString("Enter a description: ");
                        db.EditCategory(editedCategory);
                        logger.Info($"Category {category.CategoryName} updated");
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    }
                }
            }
            break;



            // Display records
            // Add options to search or display all records or search by category
            // Ask if user wants to search through discontinued products as well
            // Color discontinued products red
            case '3':
            Console.Clear();
            Console.WriteLine("Display what records? ('q' to quit)");
            Console.WriteLine("1) Products");
            Console.WriteLine("2) Products by category");
            Console.WriteLine("3) Discontinued Products");
            Console.WriteLine("4) Categories");
            Console.WriteLine("5) Categories and their active products");
            char displayOption = Inputs.GetChar("> ", new char[] {'1', '2', '3', '4', '5', 'q', 'Q'});
            logger.Info($"User choice: {displayOption}");

            // Displays all products
            if (displayOption == '1') {
                Console.Clear();
                DisplayProduct(db, logger);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays products by category
            } else if (displayOption == '2') {
                Console.Clear();
                Category searchCategory = null;
                do {
                    searchCategory = GetCategory(db, logger);
                } while (searchCategory == null); 
                Console.Clear();
                logger.Info($"Searching products by: [{searchCategory.CategoryName}]");
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{searchCategory.CategoryName}");
                Console.ResetColor();
                var productByCategory = db.Products.OrderBy(p => p.ProductId).Where(p => p.Category == searchCategory);
                foreach (Product p in productByCategory) {
                    if (p.Discontinued == false) {
                        Console.WriteLine($" {p.ProductId}: {p.ProductName}");
                    } 
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays discontinued products
            } else if (displayOption == '3') {
                Console.Clear();
                DiscontinuedProducts(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays list of categories
            } else if (displayOption == '4') {
                Console.Clear();
                DisplayCategory(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Display categories with active products
            } else if (displayOption == '5') {
                Console.Clear();
                DisplayCategoryProducts(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            break;



            // Search through products
            case '4':
            Console.Clear();
            string query = Inputs.GetString("Search for product name > ").ToLower();
            logger.Info("User query: {query}", query);
            var queryProducts = from p in db.Products
                join c in db.Categories on p.CategoryId equals c.CategoryId
                where p.ProductName.ToLower().Contains(query)
                select new { Record = $"{p.ProductId,3}: {p.ProductName,-40} | {p.UnitPrice,8:C} | {p.QuantityPerUnit,-20} | {p.UnitsInStock,4}", CategoryName = c.CategoryName};
                
            Console.WriteLine($"{queryProducts.Count()} products match your search criteria");
            Console.BackgroundColor= ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ID                Product                        Price         Quantity          Stock    Category  ");
            Console.ResetColor();
            if (queryProducts.Any()) {
                foreach (var p in queryProducts) {
                    Console.WriteLine($" {p.Record} | {p.CategoryName,12}");
                }
            } else {
                logger.Warn($"User query - {query} - returned zero results");
                Console.WriteLine("No results found");
            }
            Console.WriteLine("");
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            break;



            // Delete records
            // Delete from Product or Category table
            // Make sure to account for any orphaned records on other tables
            case '5':
            Console.Clear();
            Console.WriteLine("Delete what records? ('q' to quit)");
            Console.WriteLine("1) Product");
            Console.WriteLine("2) Category");
            char deleteOption = Inputs.GetChar("> ", new char[] {'1', '2', 'q', 'Q'});

            // Delete a product
            if (deleteOption == '1') {
                Console.Clear();
                var product = GetProduct(db, logger);
                if (product != null) {
                    db.DeleteProduct(product);
                    logger.Info($"Product: {product.ProductName} deleted");
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
            // Delete a category
            } else if (deleteOption == '2') {
                Console.Clear();
                var category = GetCategory(db, logger);
                if (category != null) {
                    var productsCategory = db.Products.Where(p => p.CategoryId == category.CategoryId).ToList();
                    int i = 0;
                    if (productsCategory.Any()) {
                        foreach (Product p in productsCategory) {
                            logger.Info($"Orphaned product: {p.ProductName} deleted");
                            db.DeleteProduct(p);
                            i += 1;
                        }
                    } else {
                        logger.Info("No orphaned products detected.");
                    }
                    db.DeleteCategory(category);
                    logger.Info($"Category: {category.CategoryName} deleted");
                    logger.Info($"{i} orphaned products deleted");
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                } 
            }
            break;

            case '6':
            Console.Clear();
            DisplayInventoryReport(db);
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            break;

        }
    } while (menuOptionsArray.Contains(option));
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");



// Displays a list of all products by their IDs
static void DisplayProduct(NWContext db, Logger logger) {
    char showDiscontinued = Inputs.GetChar("Include discontinued items? (y/n)", new char[] {'y', 'n'});
    if (showDiscontinued == 'y') {
        logger.Info("Displaying all products");
        var products = db.Products.OrderBy(p => p.ProductId);
        foreach (Product p in products) {
            if (p.Discontinued == true) {
                Console.ForegroundColor = ConsoleColor.Red;
                string formattedUnitPrice = string.Format("{0:C}", p.UnitPrice);
                Console.WriteLine($"{p.ProductId,3}: {p.ProductName,-40}");
                Console.ResetColor();
            } else {
                string formattedUnitPrice = string.Format("{0:C}", p.UnitPrice);
                Console.WriteLine($"{p.ProductId,3}: {p.ProductName,-40}");
            }
        }
    } else {
        logger.Info("Displaying only active products");
        var products = db.Products.OrderBy(p => p.ProductId).Where(p => p.Discontinued == false);
        foreach (Product p in products) {
            Console.WriteLine($"{p.ProductId,3}: {p.ProductName,-40}");
        }
    }
}

static void DiscontinuedProducts(NWContext db) {
    var products = db.Products.OrderBy(p => p.ProductId).Where(p => p.Discontinued == true);
    foreach (Product p in products) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{p.ProductId,3}: {p.ProductName,-40}");
        Console.ResetColor();
    }
}

// Returns a product ID
static Product GetProduct(NWContext db, Logger logger) {
    var products = db.Products.OrderBy(p => p.ProductId);
    foreach (Product p in products) {
        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
    }
    if (int.TryParse(Console.ReadLine(), out int ProductId)) {
        Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
    if (product != null) {
        return product;
    }
}
    logger.Error("Invalid Product ID");
    return null;
}

// Returns a product and verifies a unique name
static Product InputProduct(NWContext db, Logger logger) {
    Product product = new Product();
    product.ProductName = Inputs.GetString("Enter the product name: ");

    ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult> ();

    var isValid = Validator.TryValidateObject(product, context, results, true);
    if (isValid) {
        return product;
    } else {
        foreach (var result in results) {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
    }
    return null;
}



// Displays all categories
static void DisplayCategory(NWContext db) {
    var categories = db.Categories.OrderBy(c => c.CategoryId);
    foreach (Category c in categories) {
        Console.WriteLine($"{c.CategoryName,-20} | {c.Description}");
    }
}

// Displays all categories with their active products
static void DisplayCategoryProducts(NWContext db) {
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    foreach (var c in categories) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"{c.CategoryName}");
        Console.ResetColor();
        var productsCategory = db.Products.Where(p => p.CategoryId == c.CategoryId && p.Discontinued == false).ToList();
        if (productsCategory.Any()) {
            foreach (Product p in productsCategory) {
                Console.WriteLine($"  {p.ProductId}: {p.ProductName}");
            }
        } else {
            Console.WriteLine("No active products found in this category.");
        }
        Console.WriteLine();
    }
}

// Displays category and total assets
static void DisplayInventoryReport(NWContext db) {
    decimal assetsTotal = 0;
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    foreach (var c in categories) {
        decimal assetsCategory = 0;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"{c.CategoryName}");
        Console.ResetColor();
        var productsCategory = db.Products.Where(p => p.CategoryId == c.CategoryId && p.Discontinued == false).ToList();
        if (productsCategory.Any()) {

            foreach (Product p in productsCategory) {
                decimal assetsProduct = (decimal)p.UnitsInStock * (decimal)p.UnitPrice;
                Console.WriteLine($"  {p.ProductId,3}: {p.ProductName,-40} | Stock: {p.UnitsInStock,5} | Price: {p.UnitPrice,8:C} | Total: {assetsProduct,10:C}");
                assetsCategory += assetsProduct;
                assetsTotal += assetsProduct;

            }
        } else {
            Console.WriteLine("No active products found in this category.");
        }
        Console.WriteLine($"                                                                 Total category assets: {assetsCategory,12:C}");
        Console.WriteLine();
    }
    Console.WriteLine();
    Console.BackgroundColor = ConsoleColor.DarkGreen;
    Console.Write($"Total combined assets: {assetsTotal,12:C}");
    Console.ResetColor();
    Console.WriteLine();
}

// Returns a category ID
static Category GetCategory(NWContext db, Logger logger) {
    var categories = db.Categories.OrderBy(c => c.CategoryId);
    foreach (Category c in categories) {
        Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
    }
    Console.Write("Enter a category ID > ");
    if (int.TryParse(Console.ReadLine(), out int CategoryId)) {
        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
        if (category != null) {
            return category;
        }
    }
    logger.Error("Invalid Category ID");
    return null;
}

// Returns a category and verifies a unique name
static Category InputCategory(NWContext db, Logger logger) {
    Category category = new Category();
    category.CategoryName = Inputs.GetString("Enter the category name: ");

    ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult> ();

    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid) {
        return category;
    } else {
        foreach (var result in results) {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
    }
    return null;
}
