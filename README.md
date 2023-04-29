# Minimal API example with Northwind Database and using as a starting point the minimal api project from the book "Apps and Services With NET 7"
The example doesn't use exception handling and also doesn't implement more complex configuration in order to simplify the example.
The example shows the implementation for the GET, POST, PATCH, PUT, DELETE http operations for the Products, Category and Customers tables in the Northwind sample database.
In orther to use the example you need to configure a running instance of the Northwind database and configure the connection strings acordingly in NorthwindContextExtensions.cs.
Minimal APIs offer a simplyfied API dotnet project that allows to faster create an Rest API (and also to create a Rest API that is a bit more performant).
Minimal APIs offer a better project template for creating APIs that are not that complex and are better suited for using it as a microservice (because microservices allow you to split the app domain in smaller more manageable chunks).
With the .Net 8 it will be posible to create ahead of time compile minimal apis that will have slight execution speed advantage but the most important aspect is they will use a lot less RAM memory and thus be more cost efective.
