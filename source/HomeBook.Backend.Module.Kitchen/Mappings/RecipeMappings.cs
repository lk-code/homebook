using HomeBook.Backend.Abstractions.Models.UserManagement;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Module.Kitchen.Models;
using HomeBook.Backend.Module.Kitchen.Requests;
using HomeBook.Backend.Module.Kitchen.Responses;

namespace HomeBook.Backend.Module.Kitchen.Mappings;

public static class RecipeMappings
{
    public static RecipeResultDto ToDto(this Data.Entities.Recipe r)
    {
        return new RecipeResultDto(
            r.Id,
            r.UserId,
            r.Name,
            r.NormalizedName,
            r.Description,
            r.Servings,
            r.DurationWorkingMinutes,
            r.DurationCookingMinutes,
            r.DurationRestingMinutes,
            r.CaloriesKcal,
            r.Comments,
            r.Source,
            r.Recipe2RecipeIngredients.Select(i => i.ToDto()).ToArray(),
            r.Steps.Select(s => s.ToDto()).ToArray(),
            r.Recipe2MediaItems
                .OrderBy(x => x.Index)
                .Select(x => x.ToDto())
                .ToArray());
    }

    public static RecipeMediaItemDto ToDto(this Recipe2MediaItems relation) =>
        new(relation.MediaItemId,
            relation.Index);

    public static RecipeIngredientDto ToDto(this Data.Entities.Recipe2RecipeIngredient r2ri)
    {
        return new RecipeIngredientDto(r2ri.IngredientId,
            r2ri.RecipeIngredient.Name,
            r2ri.RecipeIngredient.NormalizedName,
            r2ri.Quantity,
            r2ri.Unit);
    }

    public static RecipeStepDto ToDto(this Data.Entities.RecipeStep rs)
    {
        return new RecipeStepDto(
            rs.RecipeId,
            rs.Position,
            rs.Description,
            rs.TimerDurationInSeconds);
    }

    public static async Task<RecipeResponse> ToResponseAsync(this RecipeResultDto r,
        Func<Guid, Task<UserInfo?>> getUserInfoAsync)
    {
        string? username = null;
        if (r.UserId.HasValue)
        {
            UserInfo? userInfo = await getUserInfoAsync(r.UserId.Value);
            username = userInfo?.Username;
        }

        Guid? heroMediaId = null;
        if (r.MediaItems.Any())
            heroMediaId = r.MediaItems
                .OrderBy(x => x.Index)
                .Select(x => (Guid?)x.MediaItemId)
                .FirstOrDefault();

        return new RecipeResponse(r.Id,
            username,
            r.Name,
            r.NormalizedName,
            r.Description,
            r.Servings,
            r.DurationWorkingMinutes,
            r.DurationCookingMinutes,
            r.DurationRestingMinutes,
            r.CaloriesKcal,
            r.Comments,
            r.Source,
            heroMediaId);
    }

    public static async Task<RecipeDetailResponse> ToDetailResponseAsync(this RecipeResultDto r,
        Func<Guid, Task<UserInfo?>> getUserInfoAsync)
    {
        string? username = null;
        if (r.UserId.HasValue)
        {
            UserInfo? userInfo = await getUserInfoAsync(r.UserId.Value);
            username = userInfo?.Username;
        }

        return new RecipeDetailResponse(r.Id,
            username,
            r.Name,
            r.NormalizedName,
            r.Description,
            r.Servings,
            r.MediaIds,
            r.MediaItems
                .OrderBy(x => x.Index)
                .Select(x => x.ToResponse())
                .ToArray(),
            r.Ingredients.Select(x => x.ToResponse()).ToArray(),
            r.Steps.Select(x => x.ToResponse()).ToArray(),
            r.DurationWorkingMinutes,
            r.DurationCookingMinutes,
            r.DurationRestingMinutes,
            r.CaloriesKcal,
            r.Comments,
            r.Source);
    }

    public static RecipeIngredientResponse ToResponse(this RecipeIngredientDto ri)
    {
        return new RecipeIngredientResponse(ri.Id,
            ri.Name,
            ri.NormalizedName,
            ri.Quantity,
            ri.Unit);
    }

    public static RecipeStepResponse ToResponse(this RecipeStepDto rs)
    {
        return new RecipeStepResponse(
            rs.RecipeId,
            rs.Position,
            rs.Description,
            rs.TimerDurationInSeconds);
    }

    public static RecipeMediaItemResponse ToResponse(this RecipeMediaItemDto mediaItem) =>
        new(mediaItem.MediaItemId,
            mediaItem.Index);

    public static RecipeRequestDto ToDto(this RecipeRequest r,
        Guid? recipeId,
        Guid userId)
    {
        RecipeMediaItemRequestDto[] mediaItems = (r.MediaItems ?? [])
            .Select(x => x.ToDto())
            .ToArray();
        if (mediaItems.Length == 0)
        {
            mediaItems = (r.MediaIds ?? [])
                .Select((mediaItemId, index) => new RecipeMediaItemRequestDto(mediaItemId, index))
                .ToArray();
        }

        RecipeRequestDto dto = new(recipeId,
            userId,
            r.Name,
            r.Description,
            r.Servings,
            mediaItems,
            (r.Ingredients ?? []).Select(i => i.ToDto()).ToArray(),
            (r.Steps ?? []).Select(i => i.ToDto()).ToArray(),
            r.DurationWorkingMinutes,
            r.DurationCookingMinutes,
            r.DurationRestingMinutes,
            r.CaloriesKcal,
            r.Comments,
            r.Source
        );

        return dto;
    }

    public static RecipeMediaItemRequestDto ToDto(this CreateRecipeMediaItemRequest r)
    {
        RecipeMediaItemRequestDto dto = new(r.MediaItemId,
            r.Index);

        return dto;
    }

    public static RecipeIngredientRequestDto ToDto(this CreateRecipeIngredientRequest r)
    {
        RecipeIngredientRequestDto dto = new(r.Name,
            r.Quantity,
            r.Unit
        );

        return dto;
    }

    public static RecipeStepRequestDto ToDto(this CreateRecipeStepRequest r)
    {
        RecipeStepRequestDto dto = new(r.Description,
            r.Position,
            r.TimerDurationInSeconds
        );

        return dto;
    }

    public static Recipe ToEntity(this RecipeRequestDto dto)
    {
        Recipe entity = new()
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationWorkingMinutes = dto.DurationWorkingMinutes,
            DurationCookingMinutes = dto.DurationCookingMinutes,
            DurationRestingMinutes = dto.DurationRestingMinutes,
            CaloriesKcal = dto.CaloriesKcal,
            Servings = dto.Servings,
            Comments = dto.Comments,
            Source = dto.Source,
            UserId = dto.UserId,
            Recipe2RecipeIngredients = (dto.Ingredients ?? []).Select(i => i.ToEntity(dto.Id)).ToArray(),
            Steps = (dto.Steps ?? []).Select(i => i.ToEntity(dto.Id)).ToArray(),
        };

        if (dto.Id.HasValue)
            entity.Id = dto.Id.Value;

        return entity;
    }

    public static Recipe2RecipeIngredient ToEntity(this RecipeIngredientRequestDto dto, Guid? recipeId)
    {
        Recipe2RecipeIngredient entity = new()
        {
            RecipeIngredient = new RecipeIngredient
            {
                Name = dto.Name
            },
            Quantity = dto.Quantity,
            Unit = dto.Unit
        };

        if (recipeId.HasValue)
            entity.RecipeId = recipeId.Value;

        return entity;
    }

    public static RecipeStep ToEntity(this RecipeStepRequestDto dto, Guid? recipeId)
    {
        RecipeStep entity = new()
        {
            Position = dto.Position,
            Description = dto.Description,
            TimerDurationInSeconds = dto.TimerDurationInSeconds
        };

        if (recipeId.HasValue)
            entity.RecipeId = recipeId.Value;

        return entity;
    }
}
