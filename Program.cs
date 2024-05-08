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
    char[] optionArray = new char[] {'1', '2', '3', '4', '5', 'q', 'Q'};
    char[] menuOptionsArray = new char[5];
    char[] optionYN = new char[] {'y', 'n', 'Y', 'N'};
    Array.Copy(optionArray, 0, menuOptionsArray, 0, 5);

    do {
        // Main user menu
        Console.Clear();
        Console.WriteLine("Enter an option or 'q' to quit:");
        Console.WriteLine("1) Add a record");
        Console.WriteLine("2) Edit a record");
        Console.WriteLine("3) Display products");
        Console.WriteLine("4) Search records");
        Console.WriteLine("5) Delete a record");
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

            // Creates a new product
            if (addOption == '1') {
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
            break;



            // Display records
            // Add options to search or display all records or search by category
            // Ask if user wants to search through discontinued products as well
            // Color discontinued products red
            case '3':
            Console.Clear();
            Console.WriteLine("Display what products? ('q' to quit)");
            Console.WriteLine("1) All Products");
            Console.WriteLine("2) Products by category");
            char displayOption = Inputs.GetChar("> ", new char[] {'1', '2', 'q', 'Q'});

            // Displays all products
            if (displayOption == '1'){
                DisplayProduct(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays products by category
            } else if (displayOption == '2') {
                var searchCategory = GetCategory(db, logger);
                logger.Info($"Searching products by: [{searchCategory.CategoryName}]");
                var productByCategory = db.Products.OrderBy(p => p.ProductId).Where(p => p.Category == searchCategory);
                foreach (Product p in productByCategory) {
                    if (p.Discontinued == true) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
                        Console.ResetColor();
                    } else {
                        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
                    }
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            } 
            break;


            // Search through products
            case '4':
            Console.Clear();
            break;



            // Delete records
            // Delete from Product or Category table
            // Make sure to account for any orphaned records on other tables
            case '5':
            Console.Clear();
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
static void DisplayProduct(NWContext db) {
    char showDiscontinued = Inputs.GetChar("Show discontinued items? (y/n)", new char[] {'y', 'n'});
    if (showDiscontinued == 'y') {
        var products = db.Products.OrderBy(p => p.ProductId);
        foreach (Product p in products) {
            if (p.Discontinued == true) {
                Console.ForegroundColor = ConsoleColor.Red;
                string formattedUnitPrice = string.Format("{0:C}", p.UnitPrice);
                Console.WriteLine($"{p.ProductId,3}: {p.ProductName,40} | Price: {formattedUnitPrice,10} | Quantity: {p.QuantityPerUnit,-25} | Stock: {p.UnitsInStock,4}");
                Console.ResetColor();
            } else {
                string formattedUnitPrice = string.Format("{0:C}", p.UnitPrice);
                Console.WriteLine($"{p.ProductId,3}: {p.ProductName,40} | Price: {formattedUnitPrice,10} | Quantity: {p.QuantityPerUnit,-25} | Stock: {p.UnitsInStock,4}");
            }
        }
    } else {
        var products = db.Products.OrderBy(p => p.ProductId).Where(p => p.Discontinued == false);
        foreach (Product p in products) {
            Console.WriteLine($"{p.ProductId}: {p.ProductName}");
        }
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



// Displays all categories
static void DisplayCategory(NWContext db) {
    var categories = db.Categories.OrderBy(c => c.CategoryId);
    foreach (Category c in categories) {
        Console.WriteLine($"{c.CategoryName}");
    }
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