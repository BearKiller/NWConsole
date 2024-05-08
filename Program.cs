﻿using NLog;
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
        Console.WriteLine("3) Display records");
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
            Console.WriteLine("Edit what records? ('q' to quit)");
            Console.WriteLine("1) Products");
            Console.WriteLine("2) Categories");
            char editOption = Inputs.GetChar("> ", new char[] {'1', '2', 'q', 'Q'});

            // Edits a product
            if (editOption == '1'){
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
            Console.WriteLine("1) All Products");
            Console.WriteLine("2) Products by category");
            Console.WriteLine("3) All categories");
            char displayOption = Inputs.GetChar("> ", new char[] {'1', '2', '3', 'q', 'Q'});

            // Displays all products
            if (displayOption == '1'){
                Console.Clear();
                DisplayProduct(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays products by category
            } else if (displayOption == '2') {
                var searchCategory = GetCategory(db, logger);
                Console.Clear();
                logger.Info($"Searching products by: [{searchCategory.CategoryName}]");
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{searchCategory.CategoryName}");
                Console.ResetColor();
                var productByCategory = db.Products.OrderBy(p => p.ProductId).Where(p => p.Category == searchCategory);
                foreach (Product p in productByCategory) {
                    if (p.Discontinued == false) {
                        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
                    } 
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

            // Displays list of categories
            } else if (displayOption == '3') {
                Console.Clear();
                DisplayCategory(db);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            break;


            // Search through products
            case '4':
            Console.Clear();
            string query = Inputs.GetString("Search for product name > ");
            var queryProducts = from p in db.Products
                join c in db.Categories on p.CategoryId equals c.CategoryId
                where p.ProductName.Contains(query)
                select new { Record = $"{p.ProductId,3}: {p.ProductName,-40} | {p.UnitPrice,8:C} | {p.QuantityPerUnit,-20} | {p.UnitsInStock,4}", CategoryName = c.CategoryName};
                
            Console.WriteLine($"{queryProducts.Count()} products match your search criteria");
            Console.BackgroundColor= ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ID                Product                        Price         Quantity          Stock    Category  ");
            Console.ResetColor();
            foreach (var p in queryProducts) {
                Console.WriteLine($" {p.Record} | {p.CategoryName,12}");
            }
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
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