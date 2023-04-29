using Microsoft.AspNetCore.Http.HttpResults; // Results
using Microsoft.AspNetCore.Mvc; // [FromServices]
using Microsoft.AspNetCore.OpenApi; // WithOpenApi
using Packt.Shared; // AddNorthwindContext extension method

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddNorthwindContext();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapGet("/", () => "Hello World!").ExcludeFromDescription();

int pageSize = 10;
//Products endpoints
app.MapGet("api/products", ([FromServices] NorthwindContext db, [FromQuery] int? page) => db.Products.Where(product => (product.UnitsInStock > 0) && (!product.Discontinued)).Skip(((page ?? 1) - 1) * pageSize).Take(pageSize))
.WithName("GetProducts").WithOpenApi(operation =>
{
    operation.Description =
    "Get products with UnitsInStock > 0 and Discontinued = false.";
    operation.Summary = "Get in-stock products that are not discontinued.";
    return operation;
}).Produces<Product[]>(StatusCodes.Status200OK);

app.MapGet("api/products/outofstock", ([FromServices] NorthwindContext db) => db.Products.Where(product => (product.UnitsInStock == 0) && (!product.Discontinued)))
.WithName("GetProductsOutOfStock")
.WithOpenApi()
.Produces<Product[]>(StatusCodes.Status200OK);

app.MapGet("api/products/discontinued", ([FromServices] NorthwindContext db) => db.Products.Where(product => product.Discontinued))
.WithName("GetProductsDiscontinued")
.WithOpenApi().Produces<Product[]>(StatusCodes.Status200OK);

app.MapGet("api/products/{id:int}", async Task<Results<Ok<Product>, NotFound>> ([FromServices] NorthwindContext db, [FromRoute] int id) => await db.Products.FindAsync(id) is Product product ? TypedResults.Ok(product) : TypedResults.NotFound())
.WithName("GetProductById")
.WithOpenApi()
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("api/products/{name}", ([FromServices] NorthwindContext db, [FromRoute] string name) => db.Products.Where(p => p.ProductName.Contains(name)))
.WithName("GetProductsByName")
.WithOpenApi()
.Produces<Product[]>(StatusCodes.Status200OK);

app.MapPost("api/products", async ([FromBody] Product product, [FromServices] NorthwindContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"api/products/{product.ProductId}", product);
}).WithOpenApi().Produces<Product>(StatusCodes.Status201Created);

app.MapPut("api/products/{id:int}", async ([FromRoute] int id, [FromBody] Product product, [FromServices] NorthwindContext db) =>
{
    Product? foundProduct = await db.Products.FindAsync(id);
    if (foundProduct is null) return Results.NotFound();
    foundProduct.ProductName = product.ProductName;
    foundProduct.CategoryId = product.CategoryId;
    foundProduct.SupplierId = product.SupplierId;
    foundProduct.QuantityPerUnit = product.QuantityPerUnit;
    foundProduct.UnitsInStock = product.UnitsInStock;
    foundProduct.UnitsOnOrder = product.UnitsOnOrder;
    foundProduct.ReorderLevel = product.ReorderLevel;
    foundProduct.UnitPrice = product.UnitPrice;
    foundProduct.Discontinued = product.Discontinued;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithOpenApi().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.MapDelete("api/products/{id:int}", async ([FromRoute] int id, [FromServices] NorthwindContext db) =>
{
    if (await db.Products.FindAsync(id) is Product product)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
}).WithOpenApi().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);


//Categories endpoints
app.MapGet("api/categories", ([FromServices] NorthwindContext db) => db.Categories)
.WithName("GetCategories")
.WithOpenApi()
.Produces<Category[]>(StatusCodes.Status200OK);

app.MapGet("api/categories/{id:int}", async ([FromServices] NorthwindContext db, [FromRoute] int id) =>
{
    if (await db.Categories.FindAsync(id) is Category category)
    {
        return Results.Ok(category);
    }
    return Results.NotFound();
})
.WithName("GetCategoryById")
.WithOpenApi()
.Produces<Category>(StatusCodes.Status200OK)
.Produces<Category>(StatusCodes.Status404NotFound);

app.MapPost("api/categories", async ([FromServices] NorthwindContext db, [FromBody] Category category) =>
{
    await db.Categories.AddAsync(category);
    await db.SaveChangesAsync();
    return Results.Created($"api/categories/{category.CategoryId}", category);
})
.WithName("PostCategory")
.WithOpenApi()
.Produces<Category>(StatusCodes.Status201Created);

app.MapPut("api/categories/{id:int}", async ([FromServices] NorthwindContext db, [FromRoute] int id, [FromBody] Category category) =>
{
    Category? foundCategory = await db.Categories.FindAsync(id);
    if (foundCategory is null) return Results.NotFound();
    foundCategory.CategoryName = category.CategoryName;
    foundCategory.Description = category.Description;
    foundCategory.Picture = category.Picture;
    foundCategory.Products = category.Products;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithOpenApi().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.MapPatch("api/categories/{id:int}", async ([FromServices] NorthwindContext db, [FromRoute] int id, [FromBody] Category category) =>
{
    if (await db.Categories.FindAsync(id) is Category foundCategory)
    {
        foundCategory.CategoryName = category.CategoryName;
        foundCategory.Description = category.Description;
        foundCategory.Picture = category.Picture;
        foundCategory.Products = category.Products;
        db.Categories.Update(foundCategory);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
})
.WithName("UpdateCategoryById")
.WithOpenApi()
.Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.MapDelete("api/categories/{id:int}", async ([FromServices] NorthwindContext db, [FromRoute] int id) =>
{
    if (await db.Categories.FindAsync(id) is Category category)
    {
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
})
.WithName("DeleteCategoryById")
.WithOpenApi()
.Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

//Customer endpoints
app.MapGet("api/customers", ([FromServices] NorthwindContext db, [FromQuery] int? page) =>
{
    if (page != null && page == 0)
    {
        return Results.BadRequest("Badly formate request.There is no page number 0.");
    }
    else
    {
        return Results.Ok(db.Customers?.Skip(((page ?? 1) - 1) * pageSize).Take(pageSize));
    }
})
.WithName("GetCustomers")
.WithOpenApi()
.Produces<Customer[]>(StatusCodes.Status200OK);

app.MapGet("api/customers/{id}", async ([FromServices] NorthwindContext db, [FromRoute] string id) =>
{
    if (await db.Customers.FindAsync(id) is Customer customer)
    {
        return Results.Ok(customer);
    }
    return Results.NotFound();
})
.WithName("GetCustomerById")
.WithOpenApi()
.Produces<Customer>(StatusCodes.Status200OK)
.Produces<Customer>(StatusCodes.Status404NotFound);

app.MapPost("api/customers", async ([FromServices] NorthwindContext db, [FromBody] Customer customer) =>
{
    await db.Customers.AddAsync(customer);
    await db.SaveChangesAsync();
    return Results.Created($"api/customers/{customer.CustomerId}", customer);
})
.WithName("PostCustomer")
.WithOpenApi()
.Produces<Customer>(StatusCodes.Status201Created);

app.MapPut("api/customers/{id}", async ([FromServices] NorthwindContext db, [FromRoute] string id, [FromBody] Customer customer) =>
{
    Customer? foundCustomer = await db.Customers.FindAsync(id);
    if (foundCustomer is null) return Results.NotFound();
    foundCustomer.Address = customer.Address;
    foundCustomer.City = customer.City;
    foundCustomer.CompanyName = customer.CompanyName;
    foundCustomer.ContactName = customer.ContactName;
    foundCustomer.ContactTitle = customer.ContactTitle;
    foundCustomer.Country = customer.Country;
    foundCustomer.CustomerTypes = customer.CustomerTypes;
    foundCustomer.Fax = customer.Fax;
    foundCustomer.Orders = customer.Orders;
    foundCustomer.Phone = customer.Phone;
    foundCustomer.PostalCode = customer.PostalCode;
    foundCustomer.Region = customer.Region;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithOpenApi().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.MapPatch("api/customers/{id}", async ([FromServices] NorthwindContext db, [FromRoute] string id, [FromBody] Customer customer) =>
{
    if (await db.Customers.FindAsync(id) is Customer foundCustomer)
    {
        foundCustomer.Address = customer.Address;
        foundCustomer.City = customer.City;
        foundCustomer.CompanyName = customer.CompanyName;
        foundCustomer.ContactName = customer.ContactName;
        foundCustomer.ContactTitle = customer.ContactTitle;
        foundCustomer.Country = customer.Country;
        foundCustomer.CustomerTypes = customer.CustomerTypes;
        foundCustomer.Fax = customer.Fax;
        foundCustomer.Orders = customer.Orders;
        foundCustomer.Phone = customer.Phone;
        foundCustomer.PostalCode = customer.PostalCode;
        foundCustomer.Region = customer.Region;
        db.Customers.Update(foundCustomer);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
})
.WithName("UpdateCustomerById")
.WithOpenApi()
.Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.MapDelete("api/customers/{id}", async ([FromServices] NorthwindContext db, [FromRoute] string id) =>
{
    if (await db.Customers.FindAsync(id) is Customer customer)
    {
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
})
.WithName("DeleteCustomerById")
.WithOpenApi()
.Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status204NoContent);

app.Run();