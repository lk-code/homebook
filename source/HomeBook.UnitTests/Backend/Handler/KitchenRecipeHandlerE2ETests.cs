using System.Security.Claims;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Search;
using HomeBook.Backend.Data;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Sqlite;
using HomeBook.Backend.Data.Sqlite.Extensions;
using HomeBook.Backend.Extensions;
using HomeBook.Backend.Module.Kitchen.Contracts;
using HomeBook.Backend.Module.Kitchen.Handler;
using HomeBook.Backend.Module.Kitchen.Requests;
using HomeBook.Backend.Module.Kitchen.Responses;
using HomeBook.UnitTests.TestCore.Backend;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class KitchenRecipeHandlerE2ETests : TestBase
{
    private ILoggerFactory _loggerFactory;
    private SqliteConnection _keepAlive = null!;

    [SetUp]
    public void SetUpSubstitutes()
    {
        // create logger
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
        });

        // create sqlite in memory
        var connectionString = ConnectionStringBuilder.BuildInMemory();

        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();
    }

    [TearDown]
    public void TearDown()
    {
        // delete sqlite in memory
        _keepAlive.Close();

        // dispose logger
        _loggerFactory.Dispose();
    }

    [Test]
    public async Task RunRecipesFullLifecycle_Returns()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string testUserName = "testuser";
        string testUserPassword = "s3cr3tP@ssw0rd!";

        SearchRegistrationFactory srf = new();
        IConfigurationRoot configuration = CreateTestConfiguration();
        IServiceCollection serviceCollection = CreateTestServiceProvider(configuration);
        IServiceProvider serviceProvider = serviceCollection
            .AddBackendDataSqlite(configuration)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddDependenciesForRuntime(configuration, InstanceStatus.RUNNING)
            .AddBackendModulesForTestEnvironment(configuration, srf)
            .BuildServiceProvider();

        // apply migrations
        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE")!;
        await databaseMigrator.MigrateAsync(cancellationToken);

        // verify that migrations were applied
        var tables = SqliteHelper.GetAllTableNames(_keepAlive)
            .Split(Environment.NewLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        tables.ShouldContain("__EFMigrationsHistory");
        tables.ShouldContain("RecipeSteps");
        tables.ShouldContain("RecipeIngredients");
        tables.ShouldContain("Recipe2RecipeIngredient");
        tables.ShouldContain("Recipes");

        // create user
        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid testUserId = await userProvider.CreateUserAsync(testUserName,
            testUserPassword,
            cancellationToken);
        ClaimsPrincipal testuser = UserHelper.CreateTestUser(testUserId,
            testUserName);

        // create instances for tests
        IRecipesProvider recipesProvider = serviceProvider.GetRequiredService<IRecipesProvider>();

        // Act & Assert

        // get all recipes
        var recipesResult1 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse1 = recipesResult1.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse1.Value.ShouldNotBeNull();
        recipesResponse1.Value.Recipes.Length.ShouldBe(0);

        // create recipes
        var createRecipeResult1 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Gyros-Pita", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult1.ShouldBeOfType<Ok>();
        var createRecipeResult2 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Nana's Italian Roulade",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult2.ShouldBeOfType<Ok>();
        var createRecipeResult3 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Pancakes", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult3.ShouldBeOfType<Ok>();
        var createRecipeResult4 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Pasta à la Roma", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult4.ShouldBeOfType<Ok>();
        var createRecipeResult5 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Rührei mit Kräutern", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult5.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult2 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse2 = recipesResult2.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse2.Value.ShouldNotBeNull();
        recipesResponse2.Value.Recipes.Length.ShouldBe(5);
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Gyros-Pita"
                                                          && r.NormalizedName == "gyros-pita");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade"
                                                          && r.NormalizedName == "nanas-italian-roulade");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Pancakes"
                                                          && r.NormalizedName == "pancakes");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma"
                                                          && r.NormalizedName == "pasta-a-la-roma");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern"
                                                          && r.NormalizedName == "ruhrei-mit-krautern");

        var recipeToDelete = recipesResponse2.Value.Recipes.First(r => r.Name == "Pancakes");
        var deleteRecipeResult = await RecipeHandler.HandleDeleteRecipe(recipeToDelete.Id,
            testuser,
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        deleteRecipeResult.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult3 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse3 = recipesResult3.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse3.Value.ShouldNotBeNull();
        recipesResponse3.Value.Recipes.Length.ShouldBe(4);
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Gyros-Pita"
                                                          && r.NormalizedName == "gyros-pita");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade"
                                                          && r.NormalizedName == "nanas-italian-roulade");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma"
                                                          && r.NormalizedName == "pasta-a-la-roma");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern"
                                                          && r.NormalizedName == "ruhrei-mit-krautern");

        var recipeToUpdate = recipesResponse3.Value.Recipes.First(r => r.Name == "Gyros-Pita");
        var updateRecipeResult = await RecipeHandler.HandleUpdateRecipeName(recipeToUpdate.Id,
            testuser,
            new RecipeRenameRequest("Gyros Wrap"),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        updateRecipeResult.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult4 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse4 = recipesResult4.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse4.Value.ShouldNotBeNull();
        recipesResponse4.Value.Recipes.Length.ShouldBe(4);
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Gyros Wrap"
                                                          && r.NormalizedName == "gyros-wrap");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade"
                                                          && r.NormalizedName == "nanas-italian-roulade");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma"
                                                          && r.NormalizedName == "pasta-a-la-roma");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern"
                                                          && r.NormalizedName == "ruhrei-mit-krautern");
    }

    [Test]
    public async Task RunRecipesWithStepsAndIngredientsFullLifecycle_Returns()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string testUserName = "testuser";
        string testUserPassword = "s3cr3tP@ssw0rd!";

        // create configuration and service provider
        SearchRegistrationFactory srf = new();
        IConfigurationRoot configuration = CreateTestConfiguration();
        IServiceCollection serviceCollection = CreateTestServiceProvider(configuration);

        IServiceProvider serviceProvider = serviceCollection
            .AddBackendDataSqlite(configuration)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddDependenciesForRuntime(configuration, InstanceStatus.RUNNING)
            .AddBackendModulesForTestEnvironment(configuration, srf)
            .BuildServiceProvider();

        // apply migrations
        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE")!;
        await databaseMigrator.MigrateAsync(cancellationToken);

        // verify that migrations were applied
        var tables = SqliteHelper.GetAllTableNames(_keepAlive)
            .Split(Environment.NewLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        tables.ShouldContain("__EFMigrationsHistory");
        tables.ShouldContain("RecipeSteps");
        tables.ShouldContain("RecipeIngredients");
        tables.ShouldContain("Recipe2RecipeIngredient");
        tables.ShouldContain("Recipes");

        // create user
        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid testUserId = await userProvider.CreateUserAsync(testUserName,
            testUserPassword,
            cancellationToken);
        ClaimsPrincipal testuser = UserHelper.CreateTestUser(testUserId,
            testUserName);

        // create instances for tests
        IRecipesProvider recipesProvider = serviceProvider.GetRequiredService<IRecipesProvider>();

        // Act & Assert

        // get all recipes
        var recipesResult1 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse1 = recipesResult1.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse1.Value.ShouldNotBeNull();
        recipesResponse1.Value.Recipes.Length.ShouldBe(0);

        // create recipes
        var createRecipeResult1 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Gyros-Pita", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult1.ShouldBeOfType<Ok>();
        var createRecipeResult2 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Nana's Italian Roulade",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult2.ShouldBeOfType<Ok>();
        var createRecipeResult3 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Pancakes",
                "Leckere Pfannkuchen mit Schoko-Creme",
                4,
                [
                    new CreateRecipeIngredientRequest("Mehl", 500, "Gramm"),
                    new CreateRecipeIngredientRequest("Salz", 1, "Priese"),
                    new CreateRecipeIngredientRequest("Milch", 1, "Liter"),
                    new CreateRecipeIngredientRequest("Schokolade", 4, "EL"),
                ],
                [
                    new CreateRecipeStepRequest("Mehl mit Salz und Milch mischen.", 0, null),
                    new CreateRecipeStepRequest("Den Teig in eine Pfanne geben und knapp eine Minute anbacken lassen.",
                        1,
                        60),
                    new CreateRecipeStepRequest("Die Schokolade schmelzen lassen", 2, (5 * 60)),
                    new CreateRecipeStepRequest("Die Schokolade über den fertigen Pfannkuchen verteilen", 3, null)
                ],
                8,
                4,
                null,
                2750,
                "Dies ist ein Test-Kommentar",
                "Das habe ich mir selbst ausgedacht",
                [],
                []),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult3.ShouldBeOfType<Ok>();
        var createRecipeResult4 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Pasta à la Roma", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult4.ShouldBeOfType<Ok>();
        var createRecipeResult5 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Rührei mit Kräutern", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult5.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult2 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse2 = recipesResult2.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse2.Value.ShouldNotBeNull();
        recipesResponse2.Value.Recipes.Length.ShouldBe(5);
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Gyros-Pita"
                                                          && r.NormalizedName == "gyros-pita");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade"
                                                          && r.NormalizedName == "nanas-italian-roulade");

        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Pancakes"
                                                          && r.NormalizedName == "pancakes"
                                                          && r.Description == "Leckere Pfannkuchen mit Schoko-Creme"
                                                          && r.DurationWorkingMinutes == 8
                                                          && r.DurationCookingMinutes == 4
                                                          && r.DurationRestingMinutes == null
                                                          && r.CaloriesKcal == 2750
                                                          && r.Servings == 4
                                                          && r.Comments == "Dies ist ein Test-Kommentar"
                                                          && r.Source == "Das habe ich mir selbst ausgedacht");

        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma"
                                                          && r.NormalizedName == "pasta-a-la-roma");
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern"
                                                          && r.NormalizedName == "ruhrei-mit-krautern");

        var recipeToDelete = recipesResponse2.Value.Recipes.First(r => r.Name == "Pancakes");
        var deleteRecipeResult = await RecipeHandler.HandleDeleteRecipe(recipeToDelete.Id,
            testuser,
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        deleteRecipeResult.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult3 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse3 = recipesResult3.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse3.Value.ShouldNotBeNull();
        recipesResponse3.Value.Recipes.Length.ShouldBe(4);
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Gyros-Pita");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.NormalizedName == "gyros-pita");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.NormalizedName == "nanas-italian-roulade");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.NormalizedName == "pasta-a-la-roma");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern");
        recipesResponse3.Value.Recipes.ShouldContain(r => r.NormalizedName == "ruhrei-mit-krautern");

        var recipeToUpdate = recipesResponse3.Value.Recipes.First(r => r.Name == "Gyros-Pita");
        var updateRecipeResult = await RecipeHandler.HandleUpdateRecipeName(recipeToUpdate.Id,
            testuser,
            new RecipeRenameRequest("Gyros Wrap"),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        updateRecipeResult.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult4 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse4 = recipesResult4.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse4.Value.ShouldNotBeNull();
        recipesResponse4.Value.Recipes.Length.ShouldBe(4);
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Gyros Wrap");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.NormalizedName == "gyros-wrap");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Nana's Italian Roulade");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.NormalizedName == "nanas-italian-roulade");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Pasta à la Roma");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.NormalizedName == "pasta-a-la-roma");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.Name == "Rührei mit Kräutern");
        recipesResponse4.Value.Recipes.ShouldContain(r => r.NormalizedName == "ruhrei-mit-krautern");
    }

    [Test]
    public async Task RunRecipesWithStepsAndIngredientsWithFullUpdate_Returns()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string testUserName = "testuser";
        string testUserPassword = "s3cr3tP@ssw0rd!";

        // create configuration and service provider
        SearchRegistrationFactory srf = new();
        IConfigurationRoot configuration = CreateTestConfiguration();
        IServiceCollection serviceCollection = CreateTestServiceProvider(configuration);

        IServiceProvider serviceProvider = serviceCollection
            .AddBackendDataSqlite(configuration)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddDependenciesForRuntime(configuration, InstanceStatus.RUNNING)
            .AddBackendModulesForTestEnvironment(configuration, srf)
            .BuildServiceProvider();

        // apply migrations
        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE")!;
        await databaseMigrator.MigrateAsync(cancellationToken);

        // verify that migrations were applied
        var tables = SqliteHelper.GetAllTableNames(_keepAlive)
            .Split(Environment.NewLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        tables.ShouldContain("__EFMigrationsHistory");
        tables.ShouldContain("RecipeSteps");
        tables.ShouldContain("RecipeIngredients");
        tables.ShouldContain("Recipe2RecipeIngredient");
        tables.ShouldContain("Recipes");

        // create user
        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid testUserId = await userProvider.CreateUserAsync(testUserName,
            testUserPassword,
            cancellationToken);
        ClaimsPrincipal testuser = UserHelper.CreateTestUser(testUserId,
            testUserName);

        // create instances for tests
        IRecipesProvider recipesProvider = serviceProvider.GetRequiredService<IRecipesProvider>();

        // Act & Assert

        // get all recipes
        var recipesResult1 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse1 = recipesResult1.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse1.Value.ShouldNotBeNull();
        recipesResponse1.Value.Recipes.Length.ShouldBe(0);

        // create recipes
        var createRecipeResult1 = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Gyros-Pita", null, null, null, null, null, null, null, null, null, null, null, null),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createRecipeResult1.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult2 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse2 = recipesResult2.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse2.Value.ShouldNotBeNull();
        recipesResponse2.Value.Recipes.Length.ShouldBe(1);
        recipesResponse2.Value.Recipes.ShouldContain(r => r.Name == "Gyros-Pita"
                                                          && r.NormalizedName == "gyros-pita");

        // update complete recipe
        var recipeId = recipesResponse2.Value.Recipes.First().Id;
        var updateRequest = new RecipeRequest("Awesome Gyros Wrap",
            "Delicious homemade gyros wrap with fresh ingredients",
            4,
            [
                new CreateRecipeIngredientRequest("Gyros", 500, "Gramm"),
                new CreateRecipeIngredientRequest("Pita Bread", 4, "Stück"),
                new CreateRecipeIngredientRequest("Tzatziki Sauce", 200, "Gramm"),
            ],
            [
                new CreateRecipeStepRequest("Prepare the gyros meat.", 0, null),
                new CreateRecipeStepRequest("Warm the pita bread.", 1, 360),
                new CreateRecipeStepRequest("Assemble the wrap with meat and sauce.", 2, null),
                new CreateRecipeStepRequest("Serve and enjoy!", 3, null)
            ],
            25,
            45,
            10,
            1200,
            "Best served warm with a side of fries",
            "https://www.a-random-recipe-source.com/awesome-gyros-wrap",
            [],
            []
        );
        var updateResult1 = await RecipeHandler.HandleUpdateRecipe(recipeId,
            testuser,
            updateRequest,
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        var updateResponse1 = updateResult1.ShouldBeOfType<Ok>();

        // get all recipes
        var recipesResult3 = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipesResponse3 = recipesResult3.ShouldBeOfType<Ok<RecipesListResponse>>();
        recipesResponse3.Value.ShouldNotBeNull();
        recipesResponse3.Value.Recipes.Length.ShouldBe(1);
        recipesResponse3.Value.Recipes.ShouldContain(r => r.Name == "Awesome Gyros Wrap"
                                                          && r.NormalizedName == "awesome-gyros-wrap"
                                                          && r.Description
                                                          == "Delicious homemade gyros wrap with fresh ingredients"
                                                          && r.Servings == 4
                                                          && r.DurationWorkingMinutes == 25
                                                          && r.DurationCookingMinutes == 45
                                                          && r.DurationRestingMinutes == 10
                                                          && r.CaloriesKcal == 1200
                                                          && r.Comments == "Best served warm with a side of fries"
                                                          && r.Source
                                                          == "https://www.a-random-recipe-source.com/awesome-gyros-wrap");

        var recipeDetailsResult = await RecipeHandler.HandleGetRecipeById(recipeId,
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipeDetailsResponse = recipeDetailsResult.ShouldBeOfType<Ok<RecipeDetailResponse>>();
        recipeDetailsResponse.Value.ShouldNotBeNull();
        recipeDetailsResponse.Value.Id.ShouldBe(recipeId);
        recipeDetailsResponse.Value.Name.ShouldBe("Awesome Gyros Wrap");
        recipeDetailsResponse.Value.NormalizedName.ShouldBe("awesome-gyros-wrap");
        recipeDetailsResponse.Value.Description.ShouldBe("Delicious homemade gyros wrap with fresh ingredients");
        recipeDetailsResponse.Value.Servings.ShouldBe(4);
        recipeDetailsResponse.Value.DurationWorkingMinutes.ShouldBe(25);
        recipeDetailsResponse.Value.DurationCookingMinutes.ShouldBe(45);
        recipeDetailsResponse.Value.DurationRestingMinutes.ShouldBe(10);
        recipeDetailsResponse.Value.CaloriesKcal.ShouldBe(1200);
        recipeDetailsResponse.Value.Comments.ShouldBe("Best served warm with a side of fries");
        recipeDetailsResponse.Value.Source.ShouldBe("https://www.a-random-recipe-source.com/awesome-gyros-wrap");
        recipeDetailsResponse.Value.Ingredients.Length.ShouldBe(3);
        recipeDetailsResponse.Value.Ingredients.ShouldContain(i => i.Name == "Gyros"
                                                                   && i.NormalizedName == "gyros"
                                                                   && i.Quantity == 500
                                                                   && i.Unit == "Gramm");
        recipeDetailsResponse.Value.Ingredients.ShouldContain(i => i.Name == "Pita Bread"
                                                                   && i.NormalizedName == "pita-bread"
                                                                   && i.Quantity == 4
                                                                   && i.Unit == "Stück");
        recipeDetailsResponse.Value.Ingredients.ShouldContain(i => i.Name == "Tzatziki Sauce"
                                                                   && i.NormalizedName == "tzatziki-sauce"
                                                                   && i.Quantity == 200
                                                                   && i.Unit == "Gramm");
        recipeDetailsResponse.Value.Steps.Length.ShouldBe(4);
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 0
                                                             && s.Description == "Prepare the gyros meat."
                                                             && s.TimerDurationInSeconds == null);
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 1
                                                             && s.Description == "Warm the pita bread."
                                                             && s.TimerDurationInSeconds == 360);
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 2
                                                             && s.Description
                                                             == "Assemble the wrap with meat and sauce."
                                                             && s.TimerDurationInSeconds == null);
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 3
                                                             && s.Description == "Serve and enjoy!"
                                                             && s.TimerDurationInSeconds == null);
    }

    [Test]
    public async Task UpdateRecipeRelations_UpdatesOnlyJoinRelationsAndReplacesSteps()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string testUserName = "testuser";
        string testUserPassword = "s3cr3tP@ssw0rd!";

        SearchRegistrationFactory srf = new();
        IConfigurationRoot configuration = CreateTestConfiguration();
        IServiceCollection serviceCollection = CreateTestServiceProvider(configuration);
        IServiceProvider serviceProvider = serviceCollection
            .AddBackendDataSqlite(configuration)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddDependenciesForRuntime(configuration, InstanceStatus.RUNNING)
            .AddBackendModulesForTestEnvironment(configuration, srf)
            .BuildServiceProvider();

        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE")!;
        await databaseMigrator.MigrateAsync(cancellationToken);

        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid testUserId = await userProvider.CreateUserAsync(testUserName,
            testUserPassword,
            cancellationToken);
        ClaimsPrincipal testuser = UserHelper.CreateTestUser(testUserId,
            testUserName);

        IRecipesProvider recipesProvider = serviceProvider.GetRequiredService<IRecipesProvider>();
        IStorageProvider storageProvider = serviceProvider.GetRequiredService<IStorageProvider>();
        IMediaItemRepository mediaItemRepository = serviceProvider.GetRequiredService<IMediaItemRepository>();
        IDbContextFactory<AppDbContext> dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        Guid scopeId = await storageProvider.RegisterStorageScopeAsync("unittests/kitchen/recipe-relations",
            "UNITTESTS",
            cancellationToken);
        Guid mediaId1 = await mediaItemRepository.AddMediaItemAsync(scopeId,
            "recipe-image-1.jpg",
            cancellationToken);
        Guid mediaId2 = await mediaItemRepository.AddMediaItemAsync(scopeId,
            "recipe-image-2.jpg",
            cancellationToken);
        Guid mediaId3 = await mediaItemRepository.AddMediaItemAsync(scopeId,
            "recipe-image-3.jpg",
            cancellationToken);

        var createResult = await RecipeHandler.HandleCreateRecipe(testuser,
            new RecipeRequest("Tomato Soup",
                "Classic tomato soup",
                2,
                [
                    new CreateRecipeIngredientRequest("Tomatoes", 6, "Pieces"),
                    new CreateRecipeIngredientRequest("Salt", 1, "Teaspoon")
                ],
                [
                    new CreateRecipeStepRequest("Cut the tomatoes.", 0, null),
                    new CreateRecipeStepRequest("Cook everything for 20 minutes.", 1, 1200)
                ],
                10,
                20,
                0,
                320,
                null,
                null,
                [],
                [mediaId1, mediaId2]),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        createResult.ShouldBeOfType<Ok>();

        var listAfterCreateResult = await RecipeHandler.HandleGetRecipes("",
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var listAfterCreateResponse = listAfterCreateResult.ShouldBeOfType<Ok<RecipesListResponse>>();
        listAfterCreateResponse.Value.ShouldNotBeNull();
        Guid recipeId = listAfterCreateResponse.Value.Recipes.Single(r => r.Name == "Tomato Soup").Id;

        var updateResult = await RecipeHandler.HandleUpdateRecipe(recipeId,
            testuser,
            new RecipeRequest("Tomato Soup Deluxe",
                "Classic tomato soup with basil",
                4,
                [
                    new CreateRecipeIngredientRequest("Tomatoes", 8, "Pieces"),
                    new CreateRecipeIngredientRequest("Basil", 5, "Leaves")
                ],
                [
                    new CreateRecipeStepRequest("Blend the tomatoes with basil.", 0, null),
                    new CreateRecipeStepRequest("Serve with olive oil.", 1, null),
                    new CreateRecipeStepRequest("Enjoy immediately.", 2, null)
                ],
                15,
                25,
                0,
                410,
                "Use ripe tomatoes.",
                "https://example.invalid/tomato-soup-deluxe",
                [],
                [mediaId2, mediaId3]),
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            cancellationToken);
        updateResult.ShouldBeOfType<Ok>();

        var recipeDetailsResult = await RecipeHandler.HandleGetRecipeById(recipeId,
            _loggerFactory.CreateLogger<RecipeHandler>(),
            recipesProvider,
            userProvider,
            cancellationToken);
        var recipeDetailsResponse = recipeDetailsResult.ShouldBeOfType<Ok<RecipeDetailResponse>>();
        recipeDetailsResponse.Value.ShouldNotBeNull();
        recipeDetailsResponse.Value.Name.ShouldBe("Tomato Soup Deluxe");
        recipeDetailsResponse.Value.MediaIds.Length.ShouldBe(2);
        recipeDetailsResponse.Value.MediaIds.ShouldContain(mediaId2);
        recipeDetailsResponse.Value.MediaIds.ShouldContain(mediaId3);
        recipeDetailsResponse.Value.MediaIds.ShouldNotContain(mediaId1);
        recipeDetailsResponse.Value.Ingredients.Length.ShouldBe(2);
        recipeDetailsResponse.Value.Ingredients.ShouldContain(i => i.Name == "Tomatoes"
                                                                   && i.Quantity == 8
                                                                   && i.Unit == "Pieces");
        recipeDetailsResponse.Value.Ingredients.ShouldContain(i => i.Name == "Basil"
                                                                   && i.Quantity == 5
                                                                   && i.Unit == "Leaves");
        recipeDetailsResponse.Value.Ingredients.ShouldNotContain(i => i.Name == "Salt");
        recipeDetailsResponse.Value.Steps.Length.ShouldBe(3);
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 0
                                                             && s.Description == "Blend the tomatoes with basil.");
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 1
                                                             && s.Description == "Serve with olive oil.");
        recipeDetailsResponse.Value.Steps.ShouldContain(s => s.Position == 2
                                                             && s.Description == "Enjoy immediately.");

        await using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        (await dbContext.Recipe2MediaItems.CountAsync(x => x.RecipeId == recipeId, cancellationToken)).ShouldBe(2);
        (await dbContext.Recipe2RecipeIngredients.CountAsync(x => x.RecipeId == recipeId, cancellationToken))
            .ShouldBe(2);
        (await dbContext.RecipeSteps.CountAsync(x => x.RecipeId == recipeId, cancellationToken)).ShouldBe(3);
        (await dbContext.RecipeIngredients.CountAsync(cancellationToken)).ShouldBe(3);
        (await dbContext.RecipeIngredients.CountAsync(x => x.NormalizedName == "tomatoes", cancellationToken))
            .ShouldBe(1);
        (await dbContext.RecipeIngredients.CountAsync(x => x.NormalizedName == "salt", cancellationToken)).ShouldBe(1);
        (await dbContext.RecipeIngredients.CountAsync(x => x.NormalizedName == "basil", cancellationToken)).ShouldBe(1);
    }
}
