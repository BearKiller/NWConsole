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

try
{
    var db = new NWContext();
    char option;

    // Update this array with each menu option added
    char[] optionArray = new char[] {'1', '2', '3', '4', 'q', 'Q'};
    char[] menuOptionsArray = new char[4];
    Array.Copy(optionArray, 0, menuOptionsArray, 0, 4);

    do
    {
        // Main user menu
        Console.WriteLine("Enter an option or 'q' to quit:");
        Console.WriteLine("1) Add a record");
        Console.WriteLine("2) Edit a record");
        Console.WriteLine("3) Display records");
        Console.WriteLine("4) Delete a record");
        option = Inputs.GetChar("> ", optionArray);
        logger.Info("User choice: {option}", option);

        switch(option) {

            // Add records
            // Choose whether to add a Product or Category
            case '1':
            Console.Clear();
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
            break; 

            // Delete records
            // Delete from Product or Category table
            // Make sure to account for any orphaned records on other tables
            case '4':
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



static Category GetCategory(NWContext db, Logger logger) {
    var categories = db.Categories.OrderBy(c => c.CategoryId);
    foreach (Category c in categories) {
        Console.WriteLine($"{c.CategoryName}");
    }
    if (int.TryParse(Console.ReadLine(), out int CategoryId)) {
        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
        if (category != null) {
            return category;
        }
    }
    logger.Error("Invalid Category ID");
    return null;
}