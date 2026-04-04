namespace Music.User.Shared;

public class UserDTO
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cognome { get; set; }
    public DateTime DataNascita { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public int NumeroCanzoni { get; set; }
}