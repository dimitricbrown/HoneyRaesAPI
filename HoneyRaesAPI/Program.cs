using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer> 
{ 
    new Customer()
    {
        Id = 1,
        Name = "Gena",
        Address = "123 NSS Lane"
    },
    new Customer()
    {
        Id = 2,
        Name = "Connie",
        Address = "456 NSS Lane"
    },
    new Customer()
    {
        Id = 3,
        Name = "Cris",
        Address = "789 NSS Lane"
    }
};

List<Employee> employees = new List<Employee> 
{
    new Employee()
    {
        Id = 1,
        Name = "Chris",
        Specialty = "Chef"
    },
    new Employee()
    {
        Id = 2,
        Name = "Carter",
        Specialty = "Assistant"
    }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Is there meat in the greens?",
        Emergency = false,
        DateCompleted = new DateTime(2023, 1, 2)
    },    
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "How many chicken breasts will feed 100 people?",
        Emergency = false,
    },    
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Can we book your services for August 15th?",
        Emergency = true,
        DateCompleted = new DateTime(2023, 7, 24)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        Description = "Can we get some fish soon in a couple of weekends?",
        Emergency = true,
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 1,
        Description = "What are the ingredients in the meatloaf?",
        Emergency = false,
        DateCompleted = new DateTime(2023, 1, 2)
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

// {id} part of the string is called a route parameter. They allow us to specify that some variable value will be present in the route.
app.MapGet("/serviceTickets/{id}", (int id) =>
{
    // linq method (FirstOrDefault) is being used
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.Select(x => new Employee { Id = x.Id, Name = x.Name, Specialty = x.Specialty })
    .FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.Select(x => new Customer { Id = x.Id, Name = x.Name, Address = x.Address })
    .FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    // linq method (FirstOrDefault) is being used
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Select(x => new ServiceTicket { CustomerId = x.CustomerId, EmployeeId = x.EmployeeId, DateCompleted = x.DateCompleted, Description = x.Description, Emergency = x.Emergency, Id = x.Id })
    .Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    // linq method (FirstOrDefault) is being used
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Select(x => new ServiceTicket { CustomerId = x.CustomerId, EmployeeId = x.EmployeeId, DateCompleted = x.DateCompleted, Description = x.Description, Emergency = x.Emergency, Id = x.Id })
    .Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.Ok(serviceTicket);
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.Run();
 