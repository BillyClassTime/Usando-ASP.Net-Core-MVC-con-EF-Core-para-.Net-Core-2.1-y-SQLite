# Usando ASP.Net Core MVC con EF Core para .Net Core 2.1 y SQLite

Para tener dos versiones de proyectos similares, uno con aplicación de consola y otro con aplicación Web ASP.Net MVC, realizaremos esta práctica, con SQL Lite.

Las tablas que utilizaremos serán Blog y Post de igual manera que lo hicimos con el proyecto de CSharp con una aplicación de consola y EF Core, mayor información [aquí](https://github.com/BillyClassTime/Usando-SQLite-con-CSharp).

## Pasos preliminares

### Creación del proyecto e instalación de paquetes de EF Core y el proveedor EF Core para SQLite

Desde la linea de comandos de Power Shell realizaremos lo siguientes pasos:

Iniciamos la creación del proyecto ASP.Net Core para netcore 2.1

```powershell
dotnet new mvc --auth None --no-https --framework netcoreapp2.1 
```

Añadimos las librerias de EF 6 Core compatibles con esta versión de NET Core

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Sqlite -v 2.1.2
dotnet add package Microsoft.EntityFrameworkCore.Design -v 2.1.2
```

El proyecto lo hemos creado en una carpeta que se llama **EntityFrameworkCore**, que será el namespace por defecto en toda esta práctica.

## Creación del Modelo, Controlador y de la Vista

### Creación del Modelo

En el proyecto que hemos creado añadimos una carpeta de Models para alojar dos clases cada una con el siguiente detalle:

**Blog**, Fichero Models\Blog.cs

```csharp
using System.Collections.Generic;

namespace EntityFrameworkCore.Models
{
public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; } = new List<Post>();
    }
}
```

**Post**, Fichero Models\Post.cs

```csharp
namespace EntityFrameworkCore.Models
{
public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
```

### Creación de la clase de Contexto de base de datos

La clase que gestionará las entidades de la base de datos y sus relaciones será BlogContext y se definirá según se detalla a continuación:

**BlogContext** Fichero Data\BlogContext.cs

```csharp
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Data
{
     public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options):base(options){}
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

       /*protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=blogging.db");*/

       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           modelBuilder.Entity<Blog>().ToTable("Blog");
           modelBuilder.Entity<Post>().ToTable("Post");
       }
    }
}
```

Hemos comentado la configuración de la cadena de conexión para independizar la clase de contexto de la base de datos del proveedor de gestión de la base de datos, con esto, podremos cambiar de SQLite a SQLServer u otra base de datos en cualquier momento.

### Clase de Inicialización de datos en el modelo

Para rellenar los datos en la base de datos SQLite, utilizamos la siguiente implementación:

**DBInitializer**, Fichero Data\DBInitializer.cs

```csharp
using EntityFrameworkCore.Models;
using System.Linq;
namespace EntityFrameworkCore.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BlogContext context)
        {
            context.Database.EnsureCreated();
            if (context.Blogs.Any())
            {
                return; // Db has been seeded
            }
            var blogs = new Blog[]
            {
                new Blog { Url = "http://blogs.msdn.com/adonet", BlogId = 1 },
                new Blog { Url = "https://devblogs.microsoft.com/dotnet", BlogId = 2 },
                new Blog { Url = "https://daddyr.blogspot.com/", BlogId = 3 },
                new Blog { Url = "https://billyclasstime.com/", BlogId = 4}
            };
            foreach (Blog b in blogs)
            {
                context.Blogs.Add(b);
            }
            context.SaveChanges();
            var post = new Post[]
            {
                new Post                    {
                    Title = "Hello World",
                    Content = "I wrote an app using EF Core!",
                    BlogId = 1
                },
                                    new Post                    {
                    Title = "EF Core vs EF Framework",
                    Content = "The difference bettewn two regular versions",
                    BlogId = 1
                },
                new Post                    {
                    Title = "ASP.Net Core using EF 6",
                    Content = "Has future the EF 6 with Dot Net 5?",
                    BlogId = 2
                },
                new Post                    {
                    Title = "Performance with EF 6",
                    Content = "Mits and realities with ORM ultra typeed",
                    BlogId = 3
                },
                                    new Post                    {
                    Title = "Xamarin and the Dot Net 5",
                    Content = "Is Dot Net 5 sure enought to implement mobil apps in IOS? ",
                    BlogId = 4
                }
            };
            context.SaveChanges();
        }
    }
}
```



### Creación del Controlador

El controlador para **Blog**, estará definido solo para las acciones **CRUD**, **(C)**reación, Letu**(R)**a, Act**(U)**alización y Borra**(D)**o, de cada uno de los datos de la tabla **Blog**.

Para la recuperación de todos los registros utilizaremos la acción Index y LINQ de EFCore.

#### Inyección de la clase de contexto de datos en el constructor del controlador

```csharp
private readonly BlogContext _context;
public BlogController(BlogContext context)
{
    _context = context;
}
```



#### (C)reación de registros en la base de datos

```csharp
public IActionResult Create()
{
    return View();
}

[HttpPost]
public async Task<IActionResult> Create(Blog blog)
{
    if (ModelState.IsValid)
    {
        _context.Add(blog);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(blog);
}
```

Definimos un metodo (C)reate para la acción GET y POST, en el POST utilizaremos la validacion del estado del modelo para sincronizar la vista con las campos de edición y demás validaciones, si no hay acciones pendientes de validacion el **ModelState.IsValid** será verdaro y podremos salvar los cambios del contexto y actualizar la base de datos.

#### Lectu(R)a de registros de la base de datos

Usaremos los métodos del controlador Details e Index para leer los datos de los registros de la base de datos pasando por la clase de context.

```csharp
// (R)ead -
public IActionResult Index()
{
    return View(_context.Blogs.ToList());
}
public IActionResult Details(int? id)
{
    if (id == null)
        return NotFound();
    var blog = _context.Blogs.FirstOrDefault(d => d.BlogId == id);
    if (blog == null)
        return NotFound();
    return View(blog);
}
```



#### Act(U)alización de datos de la Base de datos

Usaremos los metodos **Edit** con POST y GET para actualizar los datos de la base de datos:

```csharp
// (U)pdate
[HttpPost]
public IActionResult Edit(int? id, Blog blog)
{
    if (id == null)
        return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(blog);
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BlogExists(blog.BlogId))
                return NotFound();
            else
                throw;
        }
        return RedirectToAction(nameof(Index));
    }
    return View(blog);
}

public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
        return NotFound();
    var blog = await _context.Blogs.FindAsync(id);
    if (blog == null)
        return NotFound();
    return View(blog);
}
```



#### Borra(D)o de los registros de la base de datos

Para el borra(D)o de los registros usaremos los métodos **Delete**, y **DeleteConfirmed** segú indicamos a continuación:

```csharp
// (D)elete
public IActionResult Delete(int? id)
{
    if (id == null)
        return NotFound();
    var blog = _context.Blogs.FirstOrDefault(b => b.BlogId == id);
    if (blog == null)
        return NotFound();
    return View(blog);
}

[HttpPost, ActionName("Delete")]
public IActionResult DeleteConfirmed(int id)
{
    var blog = _context.Blogs.Find(id);
    _context.Blogs.Remove(blog);
    _context.SaveChanges();
    return RedirectToAction(nameof(Index));
}
```



#### Métodos adicionales

Utilizaremos en el controlador un método para validar si hay un registro en particular en el contexto de la base de datos, se usará para validar la existencia de los IDs, y lo nombraremos **BlogExists**.

```csharp
public bool BlogExists(int id)
{
    return _context.Blogs.Any(e => e.BlogId == id);
}
```

#### Utilización de los namespaces en el controlador

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.Data;
using System.Linq;
using System.Threading.Tasks;
```

### Creación de la vista

Para enlazar la vista principal "Index" al menú general de la aplicación, añadimos en la vista compartida de layout (**_Layout.cshtml**) líneas 35 la siguiente entrada:

```html
<li><a asp-area="" asp-controller="Blog" asp-action="Index">Blog</a></li>
```

A continuación crearemos sendas vistas para cada una de las acciones **CRUD** definidas en el controlador y realizar la funcionalidad de Creación, Lectura, Actualización y Borrado de los registros de la base de datos:

Defición de una vista para cada una de las acciones del controlador, según la (C)reación "Create", Lectu(R)a "Index" y "Details", Act(U)alización "Edit" y Borra(D)o, "Delete".

#### Vista de (C)reación

**Create**, Fichero \Views\Blog\Create.cshtml

```csharp
@model EntityFrameworkCore.Models.Blog

@{
    ViewData["Title"] = "Create";
}

<h1>Create</h1>

<h4>Blog</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="BlogId" class="control-label"></label>
                <input asp-for="BlogId" class="form-control" />
                <span asp-validation-for="BlogId" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Url" class="control-label"></label>
                <input asp-for="Url" class="form-control" />
                <span asp-validation-for="Url" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-action="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

#### Vista de Lectu(R)a

**Index**, Fichero \Views\Blog\Index.cshtml

```csharp
@model IEnumerable<EntityFrameworkCore.Models.Blog>

@{
    ViewData["Title"] = "Index";
}

<h2>Index</h2>

<p>
    <a asp-action="Create">Create New</a>

<table class="table">


    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.BlogId)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Url)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
            {
            <tr>
                <td>
                    @Html.DisplayFor(modelItem => item.BlogId)
                </td>
                <td>
                    @Html.DisplayFor(modelItem => item.Url)
                </td>
                <td>
                    <a asp-action="Edit" asp-route-id="@item.BlogId">Edit</a> |
                    <a asp-action="Details" asp-route-id="@item.BlogId">Details</a> |
                    <a asp-action="Delete" asp-route-id="@item.BlogId">Delete</a>
                </td>
            </tr>
            }
    </tbody>
</table>

</p>
```

**Details**, Fichero \Views\Blog\Details.cshtml

```csharp
@model EntityFrameworkCore.Models.Blog

@{
    ViewBag.Title = "Details";
}

<h1>Details</h1>

<div>
    <h4>Blog</h4>
    <hr />
    <dl class="row">
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.BlogId)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Url)
        </dd>
    </dl>
</div>
<div>
    <a asp-action="Edit" asp-route-id="@Model.BlogId">Edit</a> |
    <a asp-action="Index">Back to List</a>
</div
```

#### Vista de Act(U)alización

**Edit**, Fichero \Views\Blog\Edit.cshtml

```csharp
@model EntityFrameworkCore.Models.Blog

@{
    ViewBag.Title = "Edit";
}

<h1>Edit</h1>

<h4>Blog</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form asp-action="Edit">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <input type="hidden" asp-for="BlogId" />
            <div class="form-group">
                <label asp-for="BlogId" class="control-label"></label>
                <input asp-for="BlogId" class="form-control" />
                <span asp-validation-for="BlogId" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Url" class="control-label"></label>
                <input asp-for="Url" class="form-control" />
                <span asp-validation-for="Url" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Save" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-action="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

#### Vita de Borra(D)o

**Borrado**, Fichero \Views\Blog\Borrado.cshtml

```csharp
@model EntityFrameworkCore.Models.Blog

@{

    ViewBag.Title ="Delete";
}

<h1>Delete</h1>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Blog</h4>
    <hr />
    <dl class="row">
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.BlogId)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.BlogId)
        </dd>
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.Url)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Url)
        </dd>
    </dl>
    
    <form asp-action="Delete">
        <input type="hidden" asp-for="BlogId" />
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-action="Index">Back to List</a>
    </form>
</div>
```



### Configuracion del Middleware

En el middleware (Startup.cs y Program.cs) añadiremos 

- La inyección de dependencias del contexto de la base de datos 
- Configuraremos el proveedor de la base de datos y la cadena de conexión
- Inicialización de los datos en la base de datos

#### Inyección de dependencia del contexto de la base de datos 

Fichero: Startup.cs, Método: ConfigureServices

```csharp
services.AddDbContext<BlogContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))); 
```

#### Configuración del proveedor de la base de datos y la cadena de conexión

Fichero: appsettings.json

```json
{
  "ConnectionStrings": {

    "DefaultConnection":"Data Source=blogging.db"

  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Inicialización de los datos en la base de datos

Fichero: Program.cs Metodo:Main

```csharp
var host = CreateWebHostBuilder(args).Build();
CreateIfNotExists(host);
host.Run();
```

Fichero: Program.cs Metodo: CreateIfNotExists

```csharp
private static void CreateIfNotExists(IWebHost host)
{
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<BlogContext>();
            DbInitializer.Initialize(context);
        }
        catch (Exception )
        {
            // todo exception Handler
        }
    }
}
```



## Conclusión

Hemos utilizado visual studio code y .net Core 2.1 con las librerias de EF subyacentes para crear una aplicación ASP.Net Core MVC, un proyecto paralelo al que creamos con el mismo entorno pero para aplicación de consola. En ambos demostramos como es la funcionalidad de EF Core para la gestión CRUD de los registros en la Base de datos, como independizamos la gestión de la base de datos del código y como lo implementamos desde los dos enfoques.