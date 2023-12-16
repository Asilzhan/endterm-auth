using EndtermAuth.Data;
using EndtermAuth.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EndtermAuth.Endpoints;

public static class PostsEndpoints
{
    public static RouteGroupBuilder MapPostsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("posts")
            .WithTags("Posts")
            .AllowAnonymous();
        
        group.MapPost("/", async (Post post, AppDbContext db) =>
        {
            post.DateOfCreation = DateTime.UtcNow; // Set the creation date
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            return Results.Created($"/posts/{post.Id}", post);
        });
        
        group.MapGet("/", async (AppDbContext db) =>
            await db.Posts.ToListAsync());

        group.MapGet("/{id}", async (int id, AppDbContext db) =>
            await db.Posts.FindAsync(id)
                is { } post
                ? Results.Ok(post)
                : Results.NotFound());
        
        group.MapPut("/{id}", async (int id, Post inputPost, AppDbContext db) =>
        {
            var post = await db.Posts.FindAsync(id);

            if (post is null) return Results.NotFound();

            post.Title = inputPost.Title;
            post.Text = inputPost.Text;
            post.Author = inputPost.Author;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
        
        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var post = await db.Posts.FindAsync(id);

            if (post is null) return Results.NotFound();

            db.Posts.Remove(post);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        return group;
    }
}