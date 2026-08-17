using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[Route("habits/{habitId}/tags")]
[ApiController]
public sealed class HabitsTagController(ApplicationDbContext dbContext) : ControllerBase
{
    public static readonly string Name = nameof(HabitsTagController).Replace("Controller", string.Empty);
    //habit/:id/tags/:tagId
    [HttpPut]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto upsertHabitTagsDto)
    {
        Habit? habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);
        if (habit == null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();
        if(currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        {
            return NoContent();
        }

        List<string> existingTagIds = await dbContext.Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();
        if(existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more tag Id is invalid.");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));

        string[] tagIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();

        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc = DateTime.UtcNow
        }));

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    //habit/:id/tags/:tagId
    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitsTag(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags
            .FirstOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);
        if(habitTag == null)
        {
            return NotFound();
        }

        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
