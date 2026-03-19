using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.Module.Kitchen.Handler;
using HomeBook.Backend.Module.Kitchen.Responses;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HomeBook.Backend.Module.Kitchen.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointBuilder MapRecipeEndpoints(this IEndpointBuilder builder)
    {
        builder.AddEndpoint(routeBuilder =>
        {
            RouteGroupBuilder group = routeBuilder
                .MapGroup("/recipes")
                .WithDescription("Endpoints to manage recipes informations")
                .RequireAuthorization();

            group.MapGet("/", RecipeHandler.HandleGetRecipes)
                .WithName("GetRecipes")
                .WithDescription(new Description(
                    "returns recipes matching the search filter",
                    "HTTP 200: Recipes were found",
                    "HTTP 401: User is not authorized",
                    "HTTP 500: Unknown error while getting recipes"))
                .RequireAuthorization()
                .Produces<RecipesListResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapGet("/{id:guid}", RecipeHandler.HandleGetRecipeById)
                .WithName("GetRecipeById")
                .WithDescription(new Description(
                    "returns recipe by id",
                    "HTTP 200: Recipes were found",
                    "HTTP 404: Recipe not found",
                    "HTTP 401: User is not authorized",
                    "HTTP 500: Unknown error while getting recipes"))
                .RequireAuthorization()
                .Produces<RecipeDetailResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapGet("/{id:guid}/images", RecipeHandler.HandleGetImagesByRecipeId)
                .WithName("GetImagesByRecipeId")
                .WithDescription(new Description(
                    "returns recipe by id",
                    "HTTP 200: Recipes were found",
                    "HTTP 404: Recipe not found",
                    "HTTP 401: User is not authorized",
                    "HTTP 500: Unknown error while getting recipes"))
                .RequireAuthorization()
                .Produces<RecipeImagesResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapPost("/", RecipeHandler.HandleCreateRecipe)
                .WithName("CreateRecipe")
                .WithDescription(new Description(
                    "creates a new recipe",
                    "HTTP 201: Recipe was created",
                    "HTTP 400: Invalid request data",
                    "HTTP 401: User is not authorized",
                    "HTTP 500: Unknown error while creating recipe"))
                .RequireAuthorization()
                .Produces(StatusCodes.Status201Created)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapPut("/{id:guid}", RecipeHandler.HandleUpdateRecipe)
                .WithName("UpdateRecipe")
                .WithDescription(new Description(
                    "updates an existing recipe",
                    "HTTP 200: Recipe was updated",
                    "HTTP 400: Invalid request data",
                    "HTTP 401: User is not authorized",
                    "HTTP 404: Recipe not found",
                    "HTTP 500: Unknown error while updating recipe"))
                .RequireAuthorization()
                .Produces(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapPatch("/{id:guid}", RecipeHandler.HandleUpdateRecipeName)
                .WithName("UpdateRecipeName")
                .WithDescription(new Description(
                    "updates the name of an existing recipe",
                    "HTTP 200: Recipe Name was updated",
                    "HTTP 400: Invalid request data",
                    "HTTP 401: User is not authorized",
                    "HTTP 404: Recipe not found",
                    "HTTP 500: Unknown error while updating recipe"))
                .RequireAuthorization()
                .Produces(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces<string>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/{id:guid}", RecipeHandler.HandleDeleteRecipe)
                .WithName("DeleteRecipe")
                .WithDescription(new Description(
                    "deletes an existing recipe",
                    "HTTP 200: Recipe was deleted",
                    "HTTP 401: User is not authorized",
                    "HTTP 404: Saving goal not found",
                    "HTTP 500: Unknown error while deleting recipe"))
                .RequireAuthorization()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<string>(StatusCodes.Status404NotFound)
                .Produces<string>(StatusCodes.Status500InternalServerError);
        });

        return builder;
    }
}
