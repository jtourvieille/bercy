﻿# Création d'une Api REST

Nous allons voir dans ce dojo comment créer une Api REST en dotnet core 3.1. Vous pouvez vous servir du code présent dans ce repo.

La majeure partie des points évoqués ici sont issus du blog d'[Octo](https://blog.octo.com/designer-une-api-rest/) que je vous invite à lire.

Ici, nous allons retenir les points essentiels de cet article pour les appliquer à notre Api.

Nous allons procéder en 3 temps:
- D'abord, nous verrons comment designer notre Api, en balayant les règles les plus importantes de l'article.
- Ensuite, nous suivrons un tutoriel sur mesure pour déployer notre première Api.
- Enfin, vous serez en mesure d'ajouter votre propre implémentation d'une seconde Api.

# Conception

## Un peu de théorie
Commençons par regarder attentivement les points retenus de l'article d'Octo.

### Les verbes HTTP
> GET,POST,PATCH,PUT,DELETE 

L'idée est d'utiliser le verbe approprié à l'action intentée.

### Noms > verbes
> Pour décrire vos ressources, nous préconisons d’utiliser des noms concrets, pas de verbe

### Pluriel > singulier
> Nous préconisons donc d’utiliser le pluriel pour gérer les deux types de ressources
> Collection de ressources: /v1/users
> Instance d'une ressource: /v1/users/007

### Casse cohérente
> Nous recommandons d’utiliser pour les URI une _casse cohérente_, à choisir entre :
> **spinal-case** (mise en avant par la [RFC3986](http://tools.ietf.org/html/rfc3986#section-3.3))
> **snake_case** (fréquemment utilisée par les _Géants du Web_)

### Versionning

> Toute API sera amenée a évoluer dans le temps. Une API peut être versionée de différentes manières:
>Par _timestamp, par _numero de version_,…
>Dans le _path_, au début ou à la fin de l’_URI_
>En paramètre de la _request_
>Dans un _Header HTTP_
>Avec un versioning _facultatif_ ou _obligatoire_

>Nous préconisons de faire figurer un numéro de version obligatoire, sur un digit, au plus haut niveau du _path_ de _l’uri_.

>Exemple
>GET /v1/orders

### “Non-Resources” scenarios

>D’après la théorie RESTful, toute requête doit être vue et manipulée comme une ressource. Or en pratique, ce n’est pas toujours possible, surtout lorsqu’on parle d’opération comme la traduction, le calcul, la conversion, ou des services métiers parfois complexes inhérents à un SI.

>Dans ces cas là, votre opération doit être représentée par un verbe et non un nom. Par exemple:
>POST /convert?from=EURato=USD&amount=42
>200 OK
>{"result" : "54"}

>Pour correctement appréhender cette exception à la modélisation de votre API, le plus simple et de partir du principe que toute requête POST est une action, ayant un verbe par défaut lorsque celui ci n’est pas spécifié.

### Status Codes

>Nous préconisons fortement d’utiliser les codes de retour HTTP, de manière appropriée, sachant qu’il existe un code pour chaque cas d’utilisation courant.
>- SUCCESS
> -- 200 OK
> -- 201 Created
> CLIENT ERROR
> -- 400 BadRequest
> --401 Unauthorized
> --403 Forbidden
> --404 NotFound
> SERVER ERROR
> --500 ServerError

## La théorie appliquée à notre cas d'utilisation

On souhaite exposer les tranches par année, et le service de calcul de taxe.

### Les tranches
GET v1/tranches?year={year}

Retourne 200 OK, 400 BadRequest, 404 NotFound, 500 InternalServerError

### Le service de calcul
POST v1/ComputeTax body={json tbd}

Retourne 200 OK, 400 BadRequest, 500 InternalServerError

# On code!

## Ajouter un projet API

Add new project

Type = ASP.NET Core Web Application
Nom = Bercy.RestApi
Template = API

Lancer l'application et faire un test sur https://localhost:44342/weatherforecast avec Postman

Le retour attendu est 
```
[{"date":"2020-05-15T10:01:06.8387253+02:00","temperatureC":0,"temperatureF":32,"summary":"Hot"},{"date":"2020-05-16T10:01:06.8425003+02:00","temperatureC":46,"temperatureF":114,"summary":"Hot"},{"date":"2020-05-17T10:01:06.8425052+02:00","temperatureC":47,"temperatureF":116,"summary":"Cool"},{"date":"2020-05-18T10:01:06.8425137+02:00","temperatureC":-8,"temperatureF":18,"summary":"Hot"},{"date":"2020-05-19T10:01:06.8425142+02:00","temperatureC":47,"temperatureF":116,"summary":"Bracing"}]
```

## Ajouter un projet de test
Type = mstest dotnet core
Nom = Bercy.RestApi.Tests

Ajouter le package nuget "**Microsoft.AspNetCore.TestHost**"
Ajouter la référence vers le projet Bercy.RestApi

Copier-Coller le code suivant dans la classe de test:

```
namespace Bercy.RestApi.Tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTest1
    {
        private readonly HttpClient client;

        public UnitTest1()
        {
            var server = new TestServer(new WebHostBuilder().UseEnvironment("Developement").UseStartup<Startup>());
            this.client = server.CreateClient();
        }

        [TestMethod]
        public async Task CallWeatherForecast()
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), $"/weatherforecast");

            var response = await this.client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
```


Ceci permet de contacter la web api et de faire des tests dessus.

## Configurer l'API REST et faire un premier test

### Ajouter le versionning de route

Ajouter le package nuget "**Microsoft.AspNetCore.Mvc.Versioning**"
puis la ligne
```
services.AddApiVersioning();
```
 dans la méthode ConfigureServices de la classe Startup.
Puis changer le header de la classe WeatherForecastController par celui-ci:

```
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class WeatherForecastController : ControllerBase
```
	
Lancer le projet. On remarque que l'url est passée de 
https://localhost:44342/weatherforecast
à
https://localhost:44342/api/v1.0/weatherforecast

adapter le test.

### Ajouter la ressource Slice

On souhaite maintenant créer notre première ressource "slices" avec notre première route:

/slices?year=2019
permettant de retourner les tranches pour l'année

ajouter SlicesControllerShould

```
namespace Bercy.RestApi.Tests.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SlicesControllerShould
    {
        private readonly HttpClient client;

        public SlicesControllerShould()
        {
            var server = new TestServer(new WebHostBuilder().UseEnvironment("Developement").UseStartup<Startup>());
            this.client = server.CreateClient();
        }

        [TestMethod]
        public async Task GetSlicesForYear2019()
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"), $"/api/v1.0/slices?year=2019");

            var response = await this.client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
```

créer SlicesController comme suit:

```
namespace Bercy.RestApi.Controllers
{
    using Bercy.Slices;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SlicesController : ControllerBase
    {
        private readonly ISliceByYearProvider sliceByYearProvider;

        public SlicesController(ISliceByYearProvider sliceByYearProvider)
        {
            this.sliceByYearProvider = sliceByYearProvider;
        }

        [HttpGet]
        public IEnumerable<Slice> Get(int year)
        {
            return this.sliceByYearProvider.GetSlicesForYear(year);
        }
    }
}
```

N'oubliez pas de configurer le service ISliceByYearProvider:
```
public void ConfigureServices(IServiceCollection services)
{
	//...
	services.AddSingleton<ISliceByYearProvider, SliceByYearProvider>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISliceByYearProvider sliceByYearProvider)
{
    //...

    sliceByYearProvider.AddSlice(2019, new Slice { Low = 0, High = 10064, Rate = 0 });
    sliceByYearProvider.AddSlice(2019, new Slice { Low = 10065, High = 27794, Rate = 14 });
    sliceByYearProvider.AddSlice(2019, new Slice { Low = 27795, High = 74517, Rate = 30 });
}
```


#### DTO

Idéalement, on voudrait séparer notre objet slice décrit dans la librairie Bercy, de l'objet Slice qui nous sert à communiquer avec le client.
Pour ceci, on va créer un objet DTO (Data Tranfer Object) qui va correspondre à l'image de l'objet sous jacent.

Dans le projet Bercy.RestApi, créer un répertoire Dtos et y insérer une classe SliceDto

```
namespace Bercy.RestApi.Dtos
{
    public class SliceDto
    {
        public int Low { get; set; }
        public int High { get; set; }
        public double Rate { get; set; }
    }
}
```

C'est désormais cette classe qui sera visible pour le client. Il nous faut donc mettre à jour le controlleur:

```
namespace Bercy.RestApi.Controllers
{
    using Bercy.Slices;
    using System.Collections.Generic;
    using System.Linq;
    using Dtos;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SlicesController : ControllerBase
    {
        private readonly ISliceByYearProvider sliceByYearProvider;

        public SlicesController(ISliceByYearProvider sliceByYearProvider)
        {
            this.sliceByYearProvider = sliceByYearProvider;
        }

        [HttpGet]
        public IEnumerable<SliceDto> Get(int year)
        {
            return this.sliceByYearProvider.GetSlicesForYear(year).Select(slice =>
                new SliceDto {Low = slice.Low, High = slice.High, Rate = slice.Rate});
        }
    }
}
```

Ici, nous avons fait le mapping à la main. Dans le cas où on a peu de classes et où elles sont petites, cela est pertinent. Mais pour une utilisation intensive, cela devient problématique.
Nous allons donc introduire une librairie permettant d'automatiser le processus.

Installer la librairie "**AutoMapper.Extensions.Microsoft.DependencyInjection**" dans le projet Bercy.RestApi
Ajouter la ligne 
```
public void ConfigureServices(IServiceCollection services)
{
	//...
    services.AddAutoMapper(typeof(Startup));
}
```
		
A la racine du projet, ajouter une class AutoMapperProfile

```
namespace Bercy.RestApi
{
    using AutoMapper;
    using Dtos;
    using Slices;

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Slice, SliceDto>();
        }
    }
}
```

Puis utiliser IMapper pour faire les translations de type dans le controller:
```
namespace Bercy.RestApi.Controllers
{
    using Bercy.Slices;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Dtos;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SlicesController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ISliceByYearProvider sliceByYearProvider;

        public SlicesController(IMapper mapper, ISliceByYearProvider sliceByYearProvider)
        {
            this.mapper = mapper;
            this.sliceByYearProvider = sliceByYearProvider;
        }

        [HttpGet]
        public IEnumerable<SliceDto> Get(int year)
        {
            return this.sliceByYearProvider.GetSlicesForYear(year).Select(slice => this.mapper.Map<SliceDto>(slice));
        }
    }
}
```

### Documentation
Installer le package "**Swashbuckle.AspNetCore**" au projet Bercy.RestApi

Ajouter les lignes suivantes au ConfigureServices

```
public void ConfigureServices(IServiceCollection services)
{
	//...
	services.AddSwaggerGen(c =>
	{
		c.SwaggerDoc(
	        "v1.0", new OpenApiInfo
	        {
	            Title = "Bercy REST Api",
	            Version = "v1.0",
	            Description = "Bercy ASP.NET Core Web API",
	        });
     });
}
```
Puis configurer swagger:

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISliceByYearProvider sliceByYearProvider)
{
    //...

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Bercy REST Api V1.0");
    });
}
```

Dans les propriétés du projet Bercy.RestApi, onglet Debug, modifier Launch browser par la valeur suivante: **swagger**

### Commentaires

Modifier la classe SliceDto comme suit:

```
namespace Bercy.RestApi.Dtos
{
    /// <summary>
    /// Slice definition
    /// </summary>
    public class SliceDto
    {
        /// <summary>
        /// Gets or sets the slice low boundary
        /// </summary>
        public int Low { get; set; }

        /// <summary>
        /// Gets or sets the slice high boundary
        /// </summary>
        public int High { get; set; }

        /// <summary>
        /// Gets or sets the slice rate
        /// </summary>
        public double Rate { get; set; }
    }
}
```

puis ajouter les commentaires suivants:
```
/// <summary>
/// Gets the slices for a given year
/// </summary>
/// <param name="year">The year to be applied</param>
/// <returns>The list of slices for the year</returns>
[HttpGet]
public IEnumerable<SliceDto> Get(int year)
{
    return this.sliceByYearProvider.GetSlicesForYear(year).Select(slice => this.mapper.Map<SliceDto>(slice));
}
```
        
Ajouter la balise suivante au csproj

```
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Enfin, spécifier le chemin d'accès au xml de documentation

```
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
c.IncludeXmlComments(xmlPath);
```

Les commentaires sont désormais affichés dans l'interface swagger.

### Exemples

Pour aller plus loin, on peut introduire des exemples directement utilisables depuis l'interface swagger

Copier-coller le code suivant et observer le résultat dans swagger.

```
/// <summary>
/// Gets the slices for a given year
/// </summary>
/// <param name="year" example="2019">The year to be applied</param>
/// <returns>The list of slices for the year</returns>
/// <response code="200">Result is OK</response>
/// <response code="400">Input is invalid</response>
/// <response code="500">Internal error</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<SliceDto>), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(500)]
public IEnumerable<SliceDto> Get(int year)
{
    return this.sliceByYearProvider.GetSlicesForYear(year).Select(slice => this.mapper.Map<SliceDto>(slice));
}
```
		
Maintenant que notre api est bien configurée, étoffons les tests:

```
[TestMethod]
public async Task NotAllowBadYear_When_AskingForYearMinus5()
{
    var request = new HttpRequestMessage(new HttpMethod("GET"), $"/api/v1.0/slices?year=-5");

    var response = await this.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

    var responseJson = await response.Content.ReadAsStringAsync();

    var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(responseJson);

    Assert.AreEqual(StatusCodes.Status400BadRequest, problemDetails.Status);
    Assert.AreEqual("The given year is not acceptable.", problemDetails.Title);
}
```
		
Nous utilisons ici la classe Microsoft.AspNetCore.Mvc.ProblemDetails qui répond à la RFC 7807 https://tools.ietf.org/html/rfc7807 

L'utilisation dans l'implémentation se fait comme ceci:

```
/// <summary>
/// Gets the slices for a given year
/// </summary>
/// <param name="year" example="2019">The year to be applied</param>
/// <returns>The list of slices for the year</returns>
/// <response code="200">Result is OK</response>
/// <response code="400">Input is invalid</response>
/// <response code="500">Internal error</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<SliceDto>), 200)]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(500)]
public ActionResult<IEnumerable<SliceDto>> Get(int year)
{
    if (year < 0)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "The given year is not acceptable.",
            Detail = $"The year {year} is under zero.",
            Instance = HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails);
    }
    return new OkObjectResult(this.sliceByYearProvider.GetSlicesForYear(year).Select(slice => this.mapper.Map<SliceDto>(slice)));
}
```
		
Terminons par la gestion d'un autre type d'erreurs: lorsque l'année entrée est valide ( > 0 ), mais que notre librairie n'a pas de tranche pour cette année, nous souhaitons renvoyer une 400 avec un Title/Detail approprié.

On écrit donc le test suivant:

```
[TestMethod]
 public async Task NotAllowValidYear_When_AskingForValidYearWithNoData()
 {
     var request = new HttpRequestMessage(new HttpMethod("GET"), $"/api/v1.0/slices?year=2001");

     var response = await this.client.SendAsync(request);

     Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

     var responseJson = await response.Content.ReadAsStringAsync();

     var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(responseJson);

     Assert.AreEqual(StatusCodes.Status404NotFound, problemDetails.Status);
     Assert.AreEqual("The given year is acceptable, but no slice found.", problemDetails.Title);
 }
```

Et on modifie le code pour adapter l'implémentation
		
```
/// <summary>
/// Gets the slices for a given year
/// </summary>
/// <param name="year" example="2019">The year to be applied</param>
/// <returns>The list of slices for the year</returns>
/// <response code="200">Result is OK</response>
/// <response code="400">Input is invalid</response>
/// <response code="404">No data for the year</response>
/// <response code="500">Internal error</response>
[HttpGet]
[ProducesResponseType(typeof(IEnumerable<SliceDto>), 200)]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(500)]
public ActionResult<IEnumerable<SliceDto>> Get(int year)
{
    if (year < 0)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "The given year is not acceptable.",
            Detail = $"The year {year} is under zero.",
            Instance = HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails);
    }

    if (!this.sliceByYearProvider.Contains(year))
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "The given year is acceptable, but no slice found.",
            Detail = $"The year {year} is not configured.",
            Instance = HttpContext.Request.Path
        };
        return new NotFoundObjectResult(problemDetails);
    }

    return new OkObjectResult(this.sliceByYearProvider.GetSlicesForYear(year).Select(slice => this.mapper.Map<SliceDto>(slice)));
}
```
		
```
namespace Bercy.Slices
{
    using System.Collections.Generic;

    public interface ISliceByYearProvider
    {
        void AddSlice(int year, Slice slice);
        IEnumerable<Slice> GetSlicesForYear(int year);
        bool Contains(int year);
    }
}
```

```
namespace Bercy.Slices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SliceByYearProvider : ISliceByYearProvider
    {
        private readonly IDictionary<int, IEnumerable<Slice>> slicesByYear = new Dictionary<int, IEnumerable<Slice>>();

        public void AddSlice(int year, Slice slice)
        {
            if (slice == null)
            {
                throw new ArgumentNullException(nameof(slice));
            }

            if (slicesByYear.ContainsKey(year))
            {
                var existingSlices = slicesByYear[year].ToList();
                existingSlices.Add(slice);
                slicesByYear[year] = existingSlices;
            }
            else
            {
                slicesByYear[year] = new List<Slice>
                {
                    slice
                };
            }
        }

        public IEnumerable<Slice> GetSlicesForYear(int year)
        {
            return slicesByYear[year];
        }

        public bool Contains(int year)
        {
            return slicesByYear.ContainsKey(year);
        }
    }
}
```

# Et maintenant, la suite!

Nous avons vu en détail comment créer notre 1ere Api. Désormais, on souhaite appeler notre moteur de calcul. C'est le prochain défi, à vous de jouer!
