namespace EndtermAuth.Data.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime DateOfCreation { get; set; }
    public string Author { get; set; }
}